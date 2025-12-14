using System.Text;

namespace JsonOverTCP.Core;

/// <summary>
/// Handles message framing and protocol for TCP transmission.
/// </summary>
public static class MessageProtocol
{
    private const string MessageDelimiter = "\n";
    private static readonly Encoding Encoding = Encoding.UTF8;

    /// <summary>
    /// Encodes a packet into bytes for transmission.
    /// </summary>
    /// <param name="packet">The packet to encode.</param>
    /// <returns>Byte array ready for transmission.</returns>
    public static byte[] EncodeMessage(Packet packet)
    {
        var json = JsonSerializer.SerializePacket(packet);
        var bytes = Encoding.GetBytes(json + MessageDelimiter);
        return bytes;
    }

    /// <summary>
    /// Decodes received bytes into a packet.
    /// </summary>
    /// <param name="data">The received bytes.</param>
    /// <returns>Decoded packet or null if invalid.</returns>
    public static Packet? DecodeMessage(byte[] data)
    {
        if (data == null || data.Length == 0)
            return null;

        var json = Encoding.GetString(data).Trim();
        return JsonSerializer.DeserializePacket(json);
    }

    /// <summary>
    /// Creates a packet from a message type and object payload.
    /// </summary>
    /// <param name="messageType">The message type identifier.</param>
    /// <param name="payload">The object to serialize as payload.</param>
    /// <returns>A Packet containing the serialized message.</returns>
    public static Packet CreatePacket<T>(string messageType, T payload) where T : class
    {
        var payloadJson = JsonSerializer.Serialize(payload);
        return new Packet(messageType, payloadJson);
    }

    /// <summary>
    /// Extracts the payload from a packet and deserializes it to type T.
    /// </summary>
    /// <param name="packet">The packet to extract from.</param>
    /// <returns>Deserialized payload or null if deserialization fails.</returns>
    public static T? ExtractPayload<T>(Packet packet) where T : class
    {
        if (packet == null || string.IsNullOrWhiteSpace(packet.Payload))
            return null;

        return JsonSerializer.Deserialize<T>(packet.Payload);
    }
}
