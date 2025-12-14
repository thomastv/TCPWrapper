using JsonOverTCP.Core;

namespace JsonOverTCP;

/// <summary>
/// Event arguments for when a packet is received.
/// </summary>
public class PacketReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the received packet.
    /// </summary>
    public Packet Packet { get; }

    /// <summary>
    /// Initializes a new instance of the PacketReceivedEventArgs class.
    /// </summary>
    public PacketReceivedEventArgs(Packet packet)
    {
        Packet = packet;
    }
}
