using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;

namespace JsonOverTCP.Tests.Core;

public class PacketTests
{
    [Fact]
    public void Constructor_Default_SetsTimestamp()
    {
        // Act
        var packet = new Packet();

        // Assert
        packet.Type.Should().BeEmpty();
        packet.Payload.Should().BeEmpty();
        packet.Timestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_WithTypeAndPayload_SetsPropertiesAndTimestamp()
    {
        // Arrange
        var type = "TestType";
        var payload = "TestPayload";

        // Act
        var packet = new Packet(type, payload);

        // Assert
        packet.Type.Should().Be(type);
        packet.Payload.Should().Be(payload);
        packet.Timestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Timestamp_CreatedInSequence_IsIncreasing()
    {
        // Act
        var packet1 = new Packet();
        Thread.Sleep(10); // Ensure timestamp difference
        var packet2 = new Packet();

        // Assert
        packet2.Timestamp.Should().BeGreaterThan(packet1.Timestamp);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var packet = new Packet();
        var type = "NewType";
        var payload = "NewPayload";
        var timestamp = 9999999999L;

        // Act
        packet.Type = type;
        packet.Payload = payload;
        packet.Timestamp = timestamp;

        // Assert
        packet.Type.Should().Be(type);
        packet.Payload.Should().Be(payload);
        packet.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Constructor_WithEmptyStrings_SetsEmptyStrings()
    {
        // Arrange
        var type = string.Empty;
        var payload = string.Empty;

        // Act
        var packet = new Packet(type, payload);

        // Assert
        packet.Type.Should().BeEmpty();
        packet.Payload.Should().BeEmpty();
    }
}
