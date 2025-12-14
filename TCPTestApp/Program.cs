using JsonOverTCP.Client;
using JsonOverTCP.Core;
using JsonOverTCP.Server;


Console.WriteLine("Run as [s]erver or [c]lient?");
string? choice = Console.ReadLine()?.ToLower();

if (choice == "s")
{
    await RunServer();
}
else if (choice == "c")
{
    await RunClient();
}


static async Task RunServer()
{
    var server = new TcpServerWrapper();

    server.ClientConnected += (s, e) =>
    {
        Console.WriteLine($"✓ Client connected: {e.ClientId}");
    };

    server.ClientDisconnected += (s, e) =>
    {
        Console.WriteLine($"✗ Client disconnected: {e.ClientId}");
    };

    server.MessageReceived += async (s, e) =>
    {
        if (e.Packet.Type == "user_message")
        {
            var message = MessageProtocol.ExtractPayload<UserMessage>(e.Packet);
            if (message == null)
            {
                Console.WriteLine("❌ Failed to deserialize user message. Packet may be malformed.");
                return;
            }
            Console.WriteLine($"[{message.Username}]: {message.Text}");

            // Broadcast to all clients
            await server.BroadcastAsync("user_message", message);
        }
    };

    server.ErrorOccurred += (s, e) =>
    {
        Console.WriteLine($"❌ Server error: {e.Exception.Message}");
    };

    try
    {
        Console.WriteLine("Starting server on port 5000...");
        await server.StartAsync(5000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to start server: {ex.Message}");
    }
}

static async Task RunClient()
{
    var client = new TcpClientWrapper();

    client.Connected += (s, e) =>
    {
        Console.WriteLine("✓ Connected to server!");
    };

    client.Disconnected += (s, e) =>
    {
        Console.WriteLine("✗ Disconnected from server");
    };

    client.MessageReceived += (s, e) =>
    {
        if (e.Packet.Type == "user_message")
        {
            var message = MessageProtocol.ExtractPayload<UserMessage>(e.Packet);
            Console.WriteLine($"[{message?.Username}]: {message?.Text}");
        }
    };

    client.ErrorOccurred += (s, e) =>
    {
        Console.WriteLine($"❌ Client error: {e.Exception.Message}");
    };

    try
    {
        Console.WriteLine("Connecting to server...");
        await client.ConnectAsync("localhost", 5000);

        Console.WriteLine("Enter your username:");
        string? username = Console.ReadLine();

        Console.WriteLine("Connected! Type messages (exit to quit):");
        while (true)
        {
            string? text = Console.ReadLine();
            if (text?.ToLower() == "exit")
                break;

            var message = new UserMessage
            {
                Username = username ?? "Anonymous",
                Text = text ?? string.Empty
            };
            await client.SendMessageAsync("user_message", message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        client.Disconnect();
    }
}

/// <summary>
/// Represents a user message in the chat.
/// </summary>
public class UserMessage
{
    /// <summary>
    /// Gets or sets the username of the message sender.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a system-generated message.
/// </summary>
public class SystemMessage
{
    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of system message.
    /// </summary>
    public string Type { get; set; } = string.Empty;
}