using System.Net.Sockets;
using JsonOverTCP.Core;

namespace JsonOverTCP.Client;

/// <summary>
/// Wrapper for TCP client connections with JSON message support.
/// </summary>
/// <remarks>
/// Initializes a new instance of the TcpClientWrapper class.
/// </remarks>
/// <param name="bufferSize">The buffer size for receiving data. Default is 8192 bytes.</param>
public class TcpClientWrapper(int bufferSize = 8192) : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private readonly int _bufferSize = bufferSize;
    private bool _isConnected = false;

    /// <summary>
    /// Event raised when a message is received.
    /// </summary>
    public event EventHandler<PacketReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Event raised when connection is established.
    /// </summary>
    public event EventHandler? Connected;

    /// <summary>
    /// Event raised when connection is lost.
    /// </summary>
    public event EventHandler? Disconnected;

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    public event EventHandler<ErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// Gets a value indicating whether the client is connected.
    /// </summary>
    public bool IsConnected => _isConnected && _tcpClient?.Connected == true;

    /// <summary>
    /// Connects to a remote server.
    /// </summary>
    /// <param name="host">The hostname or IP address.</param>
    /// <param name="port">The port number.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, cancellationToken);
            _networkStream = _tcpClient.GetStream();
            _isConnected = true;
            Connected?.Invoke(this, EventArgs.Empty);

            // Start listening for incoming messages
            _ = ListenForMessagesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    /// <summary>
    /// Sends a JSON message to the server.
    /// </summary>
    /// <param name="messageType">The type identifier for the message.</param>
    /// <param name="payload">The object to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task SendMessageAsync<T>(string messageType, T payload, CancellationToken cancellationToken = default) where T : class
    {
        if (!IsConnected)
            throw new InvalidOperationException("Client is not connected.");

        try
        {
            var packet = MessageProtocol.CreatePacket(messageType, payload);
            var data = MessageProtocol.EncodeMessage(packet);
            
            await _networkStream!.WriteAsync(data, 0, data.Length, cancellationToken);
            await _networkStream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    /// <summary>
    /// Sends a raw packet to the server.
    /// </summary>
    public async Task SendPacketAsync(Packet packet, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Client is not connected.");

        try
        {
            var data = MessageProtocol.EncodeMessage(packet);
            await _networkStream!.WriteAsync(data, 0, data.Length, cancellationToken);
            await _networkStream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    /// <summary>
    /// Disconnects from the server.
    /// </summary>
    public void Disconnect()
    {
        _isConnected = false;
        _networkStream?.Close();
        _tcpClient?.Close();
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Listens for incoming messages asynchronously.
    /// </summary>
    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[_bufferSize];
        var messageBuffer = new List<byte>();

        try
        {
            while (IsConnected && !cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await _networkStream!.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0)
                {
                    Disconnect();
                    break;
                }

                messageBuffer.AddRange(buffer.Take(bytesRead));

                // Process complete messages (delimited by newline)
                while (messageBuffer.Count > 0)
                {
                    int delimiterIndex = messageBuffer.IndexOf((byte)'\n');
                    if (delimiterIndex < 0)
                        break;

                    var messageBytes = messageBuffer.Take(delimiterIndex).ToArray();
                    messageBuffer.RemoveRange(0, delimiterIndex + 1);

                    if (messageBytes.Length > 0)
                    {
                        var packet = MessageProtocol.DecodeMessage(messageBytes);
                        if (packet != null)
                        {
                            MessageReceived?.Invoke(this, new PacketReceivedEventArgs(packet));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ErrorOccurred?.Invoke(this, new ErrorEventArgs(ex));
            }
            Disconnect();
        }
    }

    /// <summary>
    /// Releases all resources used by the client.
    /// </summary>
    public void Dispose()
    {
        Disconnect();
        _networkStream?.Dispose();
        _tcpClient?.Dispose();
    }
}
