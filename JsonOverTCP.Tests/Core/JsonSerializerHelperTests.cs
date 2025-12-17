using JsonOverTCP.Core;
using Xunit;
using FluentAssertions;

namespace JsonOverTCP.Tests.Core;

public class JsonSerializerHelperTests
{
    private class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsActive { get; set; }
    }

    [Fact]
    public void Serialize_ValidObject_ReturnsJsonString()
    {
        // Arrange
        var testData = new TestData
        {
            Name = "Test",
            Value = 42,
            IsActive = true
        };

        // Act
        var json = JsonSerializerHelper.Serialize(testData);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("name");  // property name
        json.Should().Contain("Test");
        json.Should().Contain("42");
        json.Should().Contain("true");
    }

    [Fact]
    public void Serialize_NullObject_ReturnsEmptyString()
    {
        // Arrange
        TestData? testData = null;

        // Act
        var json = JsonSerializerHelper.Serialize(testData!);

        // Assert
        json.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsObject()
    {
        // Arrange
        var json = "{\"name\":\"Test\",\"value\":42,\"isActive\":true}";

        // Act
        var result = JsonSerializerHelper.Deserialize<TestData>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(42);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_EmptyString_ReturnsNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = JsonSerializerHelper.Deserialize<TestData>(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_InvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "invalid json {";

        // Act
        var result = JsonSerializerHelper.Deserialize<TestData>(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SerializePacket_ValidPacket_ReturnsJsonString()
    {
        // Arrange
        var packet = new Packet
        {
            Type = "TestType",
            Payload = "TestPayload",
            Timestamp = 1234567890
        };

        // Act
        var json = JsonSerializerHelper.SerializePacket(packet);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("type");  // property name
        json.Should().Contain("TestType");
        json.Should().Contain("TestPayload");
        json.Should().Contain("1234567890");
    }

    [Fact]
    public void DeserializePacket_ValidJson_ReturnsPacket()
    {
        // Arrange
        var json = "{\"type\":\"TestType\",\"payload\":\"TestPayload\",\"timestamp\":1234567890}";

        // Act
        var packet = JsonSerializerHelper.DeserializePacket(json);

        // Assert
        packet.Should().NotBeNull();
        packet!.Type.Should().Be("TestType");
        packet.Payload.Should().Be("TestPayload");
        packet.Timestamp.Should().Be(1234567890);
    }

    [Fact]
    public void DeserializePacket_EmptyString_ReturnsNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var packet = JsonSerializerHelper.DeserializePacket(json);

        // Assert
        packet.Should().BeNull();
    }

    [Fact]
    public void DeserializePacket_InvalidJson_ReturnsNull()
    {
        // Arrange
        var json = "invalid json {";

        // Act
        var packet = JsonSerializerHelper.DeserializePacket(json);

        // Assert
        packet.Should().BeNull();
    }

    [Fact]
    public void Deserialize_CaseInsensitive_ReturnsObject()
    {
        // Arrange
        var json = "{\"NAME\":\"Test\",\"VALUE\":42,\"ISACTIVE\":true}";

        // Act
        var result = JsonSerializerHelper.Deserialize<TestData>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(42);
        result.IsActive.Should().BeTrue();
    }
}
