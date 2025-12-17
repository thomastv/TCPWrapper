using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;
using System.Text;

namespace JsonOverTCP.Tests.Core;

public class MessageProtocolTests
{
    private class TestMessage
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void EncodeMessage_ValidPacket_ReturnsEncodedBytes()
    {
        // Arrange
        var packet = new Packet("TestType", "TestPayload");

        // Act
        var bytes = MessageProtocol.EncodeMessage(packet);

        // Assert
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);
        
        var decoded = Encoding.UTF8.GetString(bytes);
        decoded.Should().Contain("type");  // property name
        decoded.Should().Contain("TestType");
        decoded.Should().Contain("TestPayload");
        decoded.Should().EndWith("\n");
    }

    [Fact]
    public void DecodeMessage_ValidBytes_ReturnsPacket()
    {
        // Arrange
        var originalPacket = new Packet("TestType", "TestPayload");
        var bytes = MessageProtocol.EncodeMessage(originalPacket);

        // Act
        var decodedPacket = MessageProtocol.DecodeMessage(bytes);

        // Assert
        decodedPacket.Should().NotBeNull();
        decodedPacket!.Type.Should().Be("TestType");
        decodedPacket.Payload.Should().Be("TestPayload");
    }

    [Fact]
    public void DecodeMessage_NullBytes_ReturnsNull()
    {
        // Act
        var packet = MessageProtocol.DecodeMessage(null!);

        // Assert
        packet.Should().BeNull();
    }

    [Fact]
    public void DecodeMessage_EmptyBytes_ReturnsNull()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var packet = MessageProtocol.DecodeMessage(bytes);

        // Assert
        packet.Should().BeNull();
    }

    [Fact]
    public void CreatePacket_ValidObjectPayload_ReturnsPacketWithSerializedPayload()
    {
        // Arrange
        var messageType = "TestMessage";
        var payload = new TestMessage { Name = "Test", Value = 42 };

        // Act
        var packet = MessageProtocol.CreatePacket(messageType, payload);

        // Assert
        packet.Should().NotBeNull();
        packet.Type.Should().Be(messageType);
        packet.Payload.Should().NotBeNullOrEmpty();
        packet.Payload.Should().Contain("Test");
        packet.Payload.Should().Contain("42");
    }

    [Fact]
    public void ExtractPayload_ValidPacket_ReturnsDeserializedObject()
    {
        // Arrange
        var messageType = "TestMessage";
        var originalPayload = new TestMessage { Name = "Test", Value = 42 };
        var packet = MessageProtocol.CreatePacket(messageType, originalPayload);

        // Act
        var extractedPayload = MessageProtocol.ExtractPayload<TestMessage>(packet);

        // Assert
        extractedPayload.Should().NotBeNull();
        extractedPayload!.Name.Should().Be("Test");
        extractedPayload.Value.Should().Be(42);
    }

    [Fact]
    public void ExtractPayload_NullPacket_ReturnsNull()
    {
        // Act
        var payload = MessageProtocol.ExtractPayload<TestMessage>(null!);

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void ExtractPayload_EmptyPayload_ReturnsNull()
    {
        // Arrange
        var packet = new Packet("TestType", string.Empty);

        // Act
        var payload = MessageProtocol.ExtractPayload<TestMessage>(packet);

        // Assert
        payload.Should().BeNull();
    }

    [Fact]
    public void EncodeAndDecode_RoundTrip_PreservesData()
    {
        // Arrange
        var originalPayload = new TestMessage { Name = "RoundTrip", Value = 99 };
        var originalPacket = MessageProtocol.CreatePacket("Test", originalPayload);

        // Act
        var bytes = MessageProtocol.EncodeMessage(originalPacket);
        var decodedPacket = MessageProtocol.DecodeMessage(bytes);
        var extractedPayload = MessageProtocol.ExtractPayload<TestMessage>(decodedPacket!);

        // Assert
        extractedPayload.Should().NotBeNull();
        extractedPayload!.Name.Should().Be("RoundTrip");
        extractedPayload.Value.Should().Be(99);
    }

    [Fact]
    public void DecodeMessage_InvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "invalid json {";
        var bytes = Encoding.UTF8.GetBytes(invalidJson);

        // Act
        var packet = MessageProtocol.DecodeMessage(bytes);

        // Assert
        packet.Should().BeNull();
    }
}
