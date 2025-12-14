using System.Net;
using System.Net.Sockets;
using JsonOverTCP.Core;

namespace JsonOverTCP.Server;

/// <summary>
/// Wrapper for TCP server that handles multiple client connections with JSON messaging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TcpServerWrapper class.
/// </remarks>
/// <param name="bufferSize">The buffer size for receiving data. Default is 8192 bytes.</param>
public class TcpServerWrapper(int bufferSize = 8192) : IDisposable
{
    private TcpListener? _listener;
    private bool _isRunning = false;
    private readonly int _bufferSize = bufferSize;
    private readonly Dictionary<string, ClientHandler> _connectedClients = new Dictionary<string, ClientHandler>();

    /// <summary>
    /// Event raised when a new client connects.
    /// </summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>
    /// Event raised when a client disconnects.
    /// </summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>
    /// Event raised when a message is received.
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Gets a value indicating whether the server is running.
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Gets the number of connected clients.
    /// </summary>
    public int ConnectedClientCount => _connectedClients.Count;

    /// <summary>
    /// Starts the server listening on the specified port.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="backlog">Maximum number of pending connections. Default is 128.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task StartAsync(int port, int backlog = 128, CancellationToken cancellationToken = default)
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start(backlog);
            _isRunning = true;

            await AcceptClientConnectionsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _isRunning = false;
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    /// <summary>
    /// Accepts incoming client connections.
    /// </summary>
    private async Task AcceptClientConnectionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient = await _listener!.AcceptTcpClientAsync(cancellationToken);
                string clientId = Guid.NewGuid().ToString();

                var clientHandler = new ClientHandler(
                    clientId,
                    tcpClient,
                    _bufferSize,
                    OnMessageReceived,
                    OnClientDisconnected,
                    OnErrorOccurred
                );

                _connectedClients[clientId] = clientHandler;
                ClientConnected?.Invoke(this, new ClientConnectedEventArgs(clientId, tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown"));

                _ = clientHandler.HandleClientAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            }
        }
    }

    /// <summary>
    /// Sends a message to a specific client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="messageType">The message type identifier.</param>
    /// <param name="payload">The object to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task SendToClientAsync<T>(string clientId, string messageType, T payload, CancellationToken cancellationToken = default) where T : class
    {
        if (!_connectedClients.TryGetValue(clientId, out var handler))
            throw new InvalidOperationException($"Client {clientId} is not connected.");

        await handler.SendMessageAsync(messageType, payload, cancellationToken);
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    public async Task BroadcastAsync<T>(string messageType, T payload, CancellationToken cancellationToken = default) where T : class
    {
        var tasks = _connectedClients.Values
            .Select(handler => handler.SendMessageAsync(messageType, payload, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends a message to all clients except the specified one.
    /// </summary>
    public async Task BroadcastExceptAsync<T>(string excludeClientId, string messageType, T payload, CancellationToken cancellationToken = default) where T : class
    {
        var tasks = _connectedClients
            .Where(kvp => kvp.Key != excludeClientId)
            .Select(kvp => kvp.Value.SendMessageAsync(messageType, payload, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Stops the server and disconnects all clients.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;

        foreach (var client in _connectedClients.Values)
        {
            client.Disconnect();
        }

        _connectedClients.Clear();
        _listener?.Stop();
    }

    private void OnMessageReceived(string clientId, Packet packet)
    {
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(clientId, packet));
    }

    private void OnClientDisconnected(string clientId)
    {
        if (_connectedClients.ContainsKey(clientId))
        {
            _connectedClients.Remove(clientId);
        }
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(clientId));
    }

    private void OnErrorOccurred(Exception ex)
    {
        ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
    }

    /// <summary>
    /// Releases all resources used by the server.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _listener?.Stop();
    }
}
