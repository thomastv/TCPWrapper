using System.Text.Json.Serialization;

namespace JsonOverTCP.Core;

/// <summary>
/// Represents a JSON packet with a type identifier and payload.
/// </summary>
public class Packet
{
    /// <summary>
    /// Gets or sets the packet type identifier.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON payload as a string.
    /// </summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of packet creation.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Initializes a new instance of the Packet class.
    /// </summary>
    public Packet()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Initializes a new instance of the Packet class with type and payload.
    /// </summary>
    public Packet(string type, string payload) : this()
    {
        Type = type;
        Payload = payload;
    }
}
