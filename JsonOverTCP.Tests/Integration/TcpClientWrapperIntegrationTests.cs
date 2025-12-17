using JsonOverTCP.Client;
using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;
using System.Net.Sockets;
using System.Net;

namespace JsonOverTCP.Tests.Integration;

/// <summary>
/// Integration tests for TcpClientWrapper.
/// Note: These tests require actual network connectivity and may need to be run separately.
/// </summary>
public class TcpClientWrapperIntegrationTests : IDisposable
{
    private TcpListener? _testServer;
    private const int TestPort = 15000;
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _testServer?.Stop();
    }

    [Fact]
    public async Task ConnectAsync_ValidServer_ConnectsSuccessfully()
    {
        // Arrange
        StartTestServer();
        var client = new TcpClientWrapper();
        var connectedEventRaised = false;
        client.Connected += (s, e) => connectedEventRaised = true;

        // Act
        await client.ConnectAsync("localhost", TestPort, _cts.Token);

        // Assert
        await Task.Delay(100); // Give time for event to fire
        client.IsConnected.Should().BeTrue();
        connectedEventRaised.Should().BeTrue();

        // Cleanup
        client.Disconnect();
        client.Dispose();
    }

    [Fact]
    public async Task ConnectAsync_InvalidServer_ThrowsException()
    {
        // Arrange
        var client = new TcpClientWrapper();
        var errorRaised = false;
        Exception? capturedException = null;

        client.ErrorOccurred += (s, e) =>
        {
            errorRaised = true;
            capturedException = e.Exception;
        };

        // Act & Assert
        await Assert.ThrowsAsync<SocketException>(async () =>
            await client.ConnectAsync("localhost", 9999, _cts.Token));

        // The error event should be raised
        errorRaised.Should().BeTrue();
        capturedException.Should().NotBeNull();

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task SendMessageAsync_WhenConnected_SendsMessage()
    {
        // Arrange
        StartTestServer();
        var client = new TcpClientWrapper();
        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(100); // Ensure connection is established

        var testMessage = new TestMessage { Name = "Test", Value = 42 };

        // Act
        await client.SendMessageAsync("test", testMessage, _cts.Token);

        // Assert
        client.IsConnected.Should().BeTrue();

        // Cleanup
        client.Disconnect();
        client.Dispose();
    }

    [Fact]
    public async Task SendMessageAsync_WhenNotConnected_ThrowsException()
    {
        // Arrange
        var client = new TcpClientWrapper();
        var testMessage = new TestMessage { Name = "Test", Value = 42 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await client.SendMessageAsync("test", testMessage, _cts.Token));

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public async Task Disconnect_WhenConnected_DisconnectsSuccessfully()
    {
        // Arrange
        StartTestServer();
        var client = new TcpClientWrapper();
        var disconnectedEventRaised = false;
        client.Disconnected += (s, e) => disconnectedEventRaised = true;

        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(100);

        // Act
        client.Disconnect();
        await Task.Delay(100); // Give time for event to fire

        // Assert
        client.IsConnected.Should().BeFalse();
        disconnectedEventRaised.Should().BeTrue();

        // Cleanup
        client.Dispose();
    }

    private void StartTestServer()
    {
        _testServer = new TcpListener(IPAddress.Loopback, TestPort);
        _testServer.Start();

        // Accept connections in the background
        _ = Task.Run(async () =>
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var client = await _testServer.AcceptTcpClientAsync(_cts.Token);
                    _ = HandleTestClient(client);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch
            {
                // Ignore other exceptions in test server
            }
        }, _cts.Token);
    }

    private async Task HandleTestClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[8192];

            while (!_cts.Token.IsCancellationRequested && client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token);
                if (bytesRead == 0) break;

                // Echo back the data
                await stream.WriteAsync(buffer, 0, bytesRead, _cts.Token);
            }
        }
        catch
        {
            // Ignore exceptions in test client handler
        }
        finally
        {
            client.Close();
        }
    }

    private class TestMessage
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
