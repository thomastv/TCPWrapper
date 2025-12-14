using JsonOverTCP.Core;

namespace JsonOverTCP;

/// <summary>
/// Event arguments for when a message is received on the server.
/// </summary>
/// <remarks>
/// Initializes a new instance of the MessageReceivedEventArgs class.
/// </remarks>
public class MessageReceivedEventArgs(string clientId, Packet packet) : EventArgs
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public string ClientId { get; } = clientId;

    /// <summary>
    /// Gets the received packet.
    /// </summary>
    public Packet Packet { get; } = packet;
}
