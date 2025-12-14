namespace JsonOverTCP;

/// <summary>
/// Event arguments for when a client disconnects.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ClientDisconnectedEventArgs class.
/// </remarks>
public class ClientDisconnectedEventArgs(string clientId) : EventArgs
{
    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    public string ClientId { get; } = clientId;
}
