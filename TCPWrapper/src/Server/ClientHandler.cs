using System.Net.Sockets;
using JsonOverTCP.Core;

namespace JsonOverTCP.Server;

/// <summary>
/// Handles individual client connections.
/// </summary>
internal class ClientHandler(
    string clientId,
    TcpClient tcpClient,
    int bufferSize,
    Action<string, Packet> onMessageReceived,
    Action<string> onClientDisconnected,
    Action<Exception> onErrorOccurred)
{
    private readonly string _clientId = clientId;
    private readonly TcpClient _tcpClient = tcpClient;
    private readonly NetworkStream _networkStream = tcpClient.GetStream();
    private readonly int _bufferSize = bufferSize;
    private readonly Action<string, Packet> _onMessageReceived = onMessageReceived;
    private readonly Action<string> _onClientDisconnected = onClientDisconnected;
    private readonly Action<Exception> _onErrorOccurred = onErrorOccurred;

    /// <summary>
    /// Handles the client connection and listens for messages.
    /// </summary>
    public async Task HandleClientAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[_bufferSize];
        var messageBuffer = new List<byte>();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead == 0)
                {
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
                            _onMessageReceived(_clientId, packet);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _onErrorOccurred(ex);
            }
        }
        finally
        {
            Disconnect();
            _onClientDisconnected(_clientId);
        }
    }

    /// <summary>
    /// Sends a message to the client.
    /// </summary>
    public async Task SendMessageAsync<T>(string messageType, T payload, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var packet = MessageProtocol.CreatePacket(messageType, payload);
            var data = MessageProtocol.EncodeMessage(packet);

            await _networkStream.WriteAsync(data, 0, data.Length, cancellationToken);
            await _networkStream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _onErrorOccurred(ex);
            throw;
        }
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    public void Disconnect()
    {
        _networkStream?.Close();
        _tcpClient?.Close();
    }
}
