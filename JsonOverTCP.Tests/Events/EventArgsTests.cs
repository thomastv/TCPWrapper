using JsonOverTCP;
using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;

namespace JsonOverTCP.Tests.Events;

public class ClientConnectedEventArgsTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var clientId = "client-123";
        var remoteEndPoint = "192.168.1.1:5000";

        // Act
        var eventArgs = new ClientConnectedEventArgs(clientId, remoteEndPoint);

        // Assert
        eventArgs.ClientId.Should().Be(clientId);
        eventArgs.RemoteEndPoint.Should().Be(remoteEndPoint);
    }

    [Fact]
    public void Constructor_WithEmptyValues_SetsEmptyValues()
    {
        // Arrange
        var clientId = string.Empty;
        var remoteEndPoint = string.Empty;

        // Act
        var eventArgs = new ClientConnectedEventArgs(clientId, remoteEndPoint);

        // Assert
        eventArgs.ClientId.Should().BeEmpty();
        eventArgs.RemoteEndPoint.Should().BeEmpty();
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var eventArgs = new ClientConnectedEventArgs("client-123", "192.168.1.1:5000");

        // Assert
        var clientIdProperty = typeof(ClientConnectedEventArgs).GetProperty(nameof(ClientConnectedEventArgs.ClientId));
        var remoteEndPointProperty = typeof(ClientConnectedEventArgs).GetProperty(nameof(ClientConnectedEventArgs.RemoteEndPoint));

        clientIdProperty.Should().NotBeNull();
        clientIdProperty!.CanWrite.Should().BeFalse();
        
        remoteEndPointProperty.Should().NotBeNull();
        remoteEndPointProperty!.CanWrite.Should().BeFalse();
    }
}

public class ClientDisconnectedEventArgsTests
{
    [Fact]
    public void Constructor_SetsClientId()
    {
        // Arrange
        var clientId = "client-456";

        // Act
        var eventArgs = new ClientDisconnectedEventArgs(clientId);

        // Assert
        eventArgs.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void Constructor_WithEmptyClientId_SetsEmptyClientId()
    {
        // Arrange
        var clientId = string.Empty;

        // Act
        var eventArgs = new ClientDisconnectedEventArgs(clientId);

        // Assert
        eventArgs.ClientId.Should().BeEmpty();
    }

    [Fact]
    public void ClientId_IsReadOnly()
    {
        // Arrange
        var eventArgs = new ClientDisconnectedEventArgs("client-456");

        // Assert
        var property = typeof(ClientDisconnectedEventArgs).GetProperty(nameof(ClientDisconnectedEventArgs.ClientId));
        property.Should().NotBeNull();
        property!.CanWrite.Should().BeFalse();
    }
}

public class MessageReceivedEventArgsTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var clientId = "client-789";
        var packet = new Packet("TestType", "TestPayload");

        // Act
        var eventArgs = new MessageReceivedEventArgs(clientId, packet);

        // Assert
        eventArgs.ClientId.Should().Be(clientId);
        eventArgs.Packet.Should().BeSameAs(packet);
    }

    [Fact]
    public void Properties_AreReadOnly()
    {
        // Arrange
        var packet = new Packet("TestType", "TestPayload");
        var eventArgs = new MessageReceivedEventArgs("client-789", packet);

        // Assert
        var clientIdProperty = typeof(MessageReceivedEventArgs).GetProperty(nameof(MessageReceivedEventArgs.ClientId));
        var packetProperty = typeof(MessageReceivedEventArgs).GetProperty(nameof(MessageReceivedEventArgs.Packet));

        clientIdProperty.Should().NotBeNull();
        clientIdProperty!.CanWrite.Should().BeFalse();
        
        packetProperty.Should().NotBeNull();
        packetProperty!.CanWrite.Should().BeFalse();
    }
}

public class PacketReceivedEventArgsTests
{
    [Fact]
    public void Constructor_SetsPacket()
    {
        // Arrange
        var packet = new Packet("TestType", "TestPayload");

        // Act
        var eventArgs = new PacketReceivedEventArgs(packet);

        // Assert
        eventArgs.Packet.Should().BeSameAs(packet);
    }

    [Fact]
    public void Packet_IsReadOnly()
    {
        // Arrange
        var packet = new Packet("TestType", "TestPayload");
        var eventArgs = new PacketReceivedEventArgs(packet);

        // Assert
        var property = typeof(PacketReceivedEventArgs).GetProperty(nameof(PacketReceivedEventArgs.Packet));
        property.Should().NotBeNull();
        property!.CanWrite.Should().BeFalse();
    }
}

public class ErrorEventArgsTests
{
    [Fact]
    public void Constructor_SetsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var eventArgs = new ErrorEventArgs(exception);

        // Assert
        eventArgs.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void Exception_IsReadOnly()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var eventArgs = new ErrorEventArgs(exception);

        // Assert
        var property = typeof(ErrorEventArgs).GetProperty(nameof(ErrorEventArgs.Exception));
        property.Should().NotBeNull();
        property!.CanWrite.Should().BeFalse();
    }
}
