namespace JsonOverTCP;

/// <summary>
/// Event arguments for when a client connects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ClientConnectedEventArgs class.
/// </remarks>
public class ClientConnectedEventArgs(string clientId, string remoteEndPoint) : EventArgs
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public string ClientId { get; } = clientId;

    /// <summary>
    /// Gets the client's remote endpoint.
    /// </summary>
    public string RemoteEndPoint { get; } = remoteEndPoint;
}
