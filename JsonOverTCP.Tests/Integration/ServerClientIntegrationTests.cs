using JsonOverTCP.Server;
using JsonOverTCP.Client;
using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;

namespace JsonOverTCP.Tests.Integration;

/// <summary>
/// Integration tests for TcpServerWrapper and complete client-server scenarios.
/// </summary>
public class ServerClientIntegrationTests : IDisposable
{
    private const int TestPort = 15001;
    private readonly CancellationTokenSource _cts = new();

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    [Fact]
    public async Task ServerAcceptsClientConnection()
    {
        // Arrange
        var server = new TcpServerWrapper();
        var clientConnectedEventRaised = false;
        string? connectedClientId = null;

        server.ClientConnected += (s, e) =>
        {
            clientConnectedEventRaised = true;
            connectedClientId = e.ClientId;
        };

        _ = server.StartAsync(TestPort, cancellationToken: _cts.Token);
        await Task.Delay(200); // Give server time to start

        var client = new TcpClientWrapper();

        // Act
        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(300); // Give time for connection event

        // Assert
        server.IsRunning.Should().BeTrue();
        server.ConnectedClientCount.Should().Be(1);
        clientConnectedEventRaised.Should().BeTrue();
        connectedClientId.Should().NotBeNullOrEmpty();

        // Cleanup
        client.Disconnect();
        client.Dispose();
        server.Stop();
        server.Dispose();
    }

    [Fact]
    public async Task ClientSendsMessageToServer()
    {
        // Arrange
        var server = new TcpServerWrapper();
        Packet? receivedPacket = null;
        var messageReceivedEvent = new TaskCompletionSource<bool>();

        server.MessageReceived += (s, e) =>
        {
            receivedPacket = e.Packet;
            messageReceivedEvent.TrySetResult(true);
        };

        _ = server.StartAsync(TestPort, cancellationToken: _cts.Token);
        await Task.Delay(200);

        var client = new TcpClientWrapper();
        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(200);

        var testMessage = new TestMessage { Name = "Hello", Value = 123 };

        // Act
        await client.SendMessageAsync("test_message", testMessage, _cts.Token);

        // Wait for message to be received (with timeout)
        var received = await Task.WhenAny(messageReceivedEvent.Task, Task.Delay(2000));

        // Assert
        (received == messageReceivedEvent.Task).Should().BeTrue("Message should be received within timeout");
        receivedPacket.Should().NotBeNull();
        receivedPacket!.Type.Should().Be("test_message");
        
        var extractedMessage = MessageProtocol.ExtractPayload<TestMessage>(receivedPacket);
        extractedMessage.Should().NotBeNull();
        extractedMessage!.Name.Should().Be("Hello");
        extractedMessage.Value.Should().Be(123);

        // Cleanup
        client.Disconnect();
        client.Dispose();
        server.Stop();
        server.Dispose();
    }

    [Fact]
    public async Task ServerSendsMessageToClient()
    {
        // Arrange
        var server = new TcpServerWrapper();
        string? clientId = null;
        var clientConnectedEvent = new TaskCompletionSource<bool>();

        server.ClientConnected += (s, e) =>
        {
            clientId = e.ClientId;
            clientConnectedEvent.TrySetResult(true);
        };

        _ = server.StartAsync(TestPort, cancellationToken: _cts.Token);
        await Task.Delay(200);

        var client = new TcpClientWrapper();
        Packet? receivedPacket = null;
        var messageReceivedEvent = new TaskCompletionSource<bool>();

        client.MessageReceived += (s, e) =>
        {
            receivedPacket = e.Packet;
            messageReceivedEvent.TrySetResult(true);
        };

        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        
        // Wait for client to be connected on server
        await Task.WhenAny(clientConnectedEvent.Task, Task.Delay(2000));
        clientId.Should().NotBeNull();
        await Task.Delay(200); // Additional stabilization

        var testMessage = new TestMessage { Name = "ServerMessage", Value = 456 };

        // Act
        await server.SendToClientAsync(clientId!, "server_message", testMessage, _cts.Token);

        // Wait for message to be received
        var received = await Task.WhenAny(messageReceivedEvent.Task, Task.Delay(2000));

        // Assert
        (received == messageReceivedEvent.Task).Should().BeTrue("Message should be received within timeout");
        receivedPacket.Should().NotBeNull();
        receivedPacket!.Type.Should().Be("server_message");
        
        var extractedMessage = MessageProtocol.ExtractPayload<TestMessage>(receivedPacket);
        extractedMessage.Should().NotBeNull();
        extractedMessage!.Name.Should().Be("ServerMessage");
        extractedMessage.Value.Should().Be(456);

        // Cleanup
        client.Disconnect();
        client.Dispose();
        server.Stop();
        server.Dispose();
    }

    [Fact]
    public async Task ServerBroadcastsToMultipleClients()
    {
        // Arrange
        var server = new TcpServerWrapper();
        _ = server.StartAsync(TestPort, cancellationToken: _cts.Token);
        await Task.Delay(200);

        var client1 = new TcpClientWrapper();
        var client2 = new TcpClientWrapper();
        
        Packet? receivedPacket1 = null;
        Packet? receivedPacket2 = null;
        var messageReceived1 = new TaskCompletionSource<bool>();
        var messageReceived2 = new TaskCompletionSource<bool>();

        client1.MessageReceived += (s, e) =>
        {
            receivedPacket1 = e.Packet;
            messageReceived1.TrySetResult(true);
        };

        client2.MessageReceived += (s, e) =>
        {
            receivedPacket2 = e.Packet;
            messageReceived2.TrySetResult(true);
        };

        await client1.ConnectAsync("localhost", TestPort, _cts.Token);
        await client2.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(300); // Ensure both clients are connected

        server.ConnectedClientCount.Should().Be(2);

        var broadcastMessage = new TestMessage { Name = "Broadcast", Value = 999 };

        // Act
        await server.BroadcastAsync("broadcast_message", broadcastMessage, _cts.Token);

        // Wait for both messages
        var received1 = await Task.WhenAny(messageReceived1.Task, Task.Delay(2000));
        var received2 = await Task.WhenAny(messageReceived2.Task, Task.Delay(2000));

        // Assert
        (received1 == messageReceived1.Task).Should().BeTrue();
        (received2 == messageReceived2.Task).Should().BeTrue();
        
        receivedPacket1.Should().NotBeNull();
        receivedPacket2.Should().NotBeNull();
        
        receivedPacket1!.Type.Should().Be("broadcast_message");
        receivedPacket2!.Type.Should().Be("broadcast_message");

        // Cleanup
        client1.Disconnect();
        client2.Disconnect();
        client1.Dispose();
        client2.Dispose();
        server.Stop();
        server.Dispose();
    }

    [Fact]
    public async Task ServerDetectsClientDisconnection()
    {
        // Arrange
        var server = new TcpServerWrapper();
        string? disconnectedClientId = null;
        var disconnectEvent = new TaskCompletionSource<bool>();

        server.ClientDisconnected += (s, e) =>
        {
            disconnectedClientId = e.ClientId;
            disconnectEvent.TrySetResult(true);
        };

        _ = server.StartAsync(TestPort, cancellationToken: _cts.Token);
        await Task.Delay(200);

        var client = new TcpClientWrapper();
        await client.ConnectAsync("localhost", TestPort, _cts.Token);
        await Task.Delay(300);

        server.ConnectedClientCount.Should().Be(1);

        // Act
        client.Disconnect();
        client.Dispose();

        // Wait for disconnect event
        var received = await Task.WhenAny(disconnectEvent.Task, Task.Delay(2000));

        // Assert
        (received == disconnectEvent.Task).Should().BeTrue("Disconnect event should fire");
        disconnectedClientId.Should().NotBeNullOrEmpty();

        await Task.Delay(200); // Give time for cleanup
        server.ConnectedClientCount.Should().Be(0);

        // Cleanup
        server.Stop();
        server.Dispose();
    }

    private class TestMessage
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
