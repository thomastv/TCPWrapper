using System.Text.Json;

namespace JsonOverTCP.Core;

/// <summary>
/// Handles JSON serialization and deserialization with custom options.
/// </summary>
public static class JsonSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an object to JSON string.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize<T>(T obj) where T : class
    {
        if (obj == null)
            return string.Empty;

        return System.Text.Json.JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>Deserialized object or null if deserialization fails.</returns>
    public static T? Deserialize<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes a packet to JSON string.
    /// </summary>
    public static string SerializePacket(Packet packet)
    {
        return System.Text.Json.JsonSerializer.Serialize(packet, DefaultOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a Packet.
    /// </summary>
    public static Packet? DeserializePacket(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Packet>(json, DefaultOptions);
        }
        catch
        {
            return null;
        }
    }
}
