# JsonOverTCP

A lightweight .NET library for easy JSON serialization and transmission over TCP connections. This library provides simple wrappers for TCP client and server implementations that handle the complexity of message framing, JSON serialization, and event-driven communication.

## Features

- **Easy-to-use TCP Client Wrapper** - Simplified TCP client with async/await support
- **Event-Driven Server** - Non-blocking TCP server with client connection management
- **JSON Message Protocol** - Automatic serialization/deserialization of C# objects to JSON
- **Packet Type System** - Specify message types for easy message routing
- **Async/Await Support** - Fully asynchronous operations using modern C# patterns
- **Error Handling** - Comprehensive error event callbacks
- **Message Framing** - Automatic message delimiting and parsing
- **Broadcast Support** - Send messages to all or multiple specific clients

## Installation

Add this library to your .NET project:

```bash
dotnet add reference JsonOverTCP.csproj
```

## Quick Start

### Server Example

```csharp
using JsonOverTCP.Server;

// Create a server instance
var server = new TcpServerWrapper();

// Subscribe to events
server.ClientConnected += (s, e) => 
    Console.WriteLine($"Client connected: {e.ClientId}");

server.MessageReceived += (s, e) => 
{
    Console.WriteLine($"Message from {e.ClientId}: {e.Packet.Type}");
    // Extract and deserialize the payload
    var data = MessageProtocol.ExtractPayload<MyMessage>(e.Packet);
};

server.ErrorOccurred += (s, e) => 
    Console.WriteLine($"Error: {e.Exception.Message}");

// Start server on port 5000
await server.StartAsync(5000);

// Broadcast to all clients
await server.BroadcastAsync("notification", new { message = "Hello everyone!" });
```

### Client Example

```csharp
using JsonOverTCP.Client;
using JsonOverTCP.Core;

// Create a client instance
var client = new TcpClientWrapper();

// Subscribe to events
client.Connected += (s, e) => 
    Console.WriteLine("Connected to server");

client.MessageReceived += (s, e) => 
{
    Console.WriteLine($"Received: {e.Packet.Type}");
    var data = MessageProtocol.ExtractPayload<MyMessage>(e.Packet);
};

// Connect to server
await client.ConnectAsync("localhost", 5000);

// Send a message
var myData = new { name = "John", age = 30 };
await client.SendMessageAsync("user_info", myData);

// Disconnect
client.Disconnect();
```

## Core Concepts

### Packet Structure

Messages are transmitted using the `Packet` class, which contains:

- **Type**: A string identifier for the message type (e.g., "user_login", "notification")
- **Payload**: JSON-serialized object data
- **Timestamp**: Unix timestamp of packet creation

### Message Protocol

The `MessageProtocol` class handles:
- Creating packets from objects: `MessageProtocol.CreatePacket<T>(type, payload)`
- Encoding packets for transmission: `MessageProtocol.EncodeMessage(packet)`
- Decoding received bytes: `MessageProtocol.DecodeMessage(data)`
- Extracting payloads: `MessageProtocol.ExtractPayload<T>(packet)`

### JSON Serialization

The `JsonSerializer` class provides:
- `Serialize<T>(object)` - Serialize an object to JSON
- `Deserialize<T>(json)` - Deserialize JSON to an object
- `SerializePacket(packet)` - Serialize a packet
- `DeserializePacket(json)` - Deserialize a packet

## Advanced Usage

### Custom Message Types

Define your message classes:

```csharp
public class UserLoginMessage
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class ServerResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}
```

Send and receive:

```csharp
// Client side
var loginMsg = new UserLoginMessage { Username = "alice", Password = "secret" };
await client.SendMessageAsync("user_login", loginMsg);

// Server side
server.MessageReceived += async (s, e) =>
{
    if (e.Packet.Type == "user_login")
    {
        var loginData = MessageProtocol.ExtractPayload<UserLoginMessage>(e.Packet);
        var response = new ServerResponse 
        { 
            Success = true, 
            Message = "Login successful" 
        };
        await server.SendToClientAsync(e.ClientId, "login_response", response);
    }
};
```

### Broadcasting to Specific Clients

```csharp
// Send to all clients
await server.BroadcastAsync("announcement", new { text = "Server maintenance in 5 minutes" });

// Send to all except the sender
await server.BroadcastExceptAsync(senderId, "user_joined", new { userId = senderId });

// Send to specific client
await server.SendToClientAsync(clientId, "private_message", message);
```

### Error Handling

```csharp
client.ErrorOccurred += (s, e) =>
{
    Console.WriteLine($"Client error: {e.Exception.Message}");
    // Handle reconnection logic here
};

server.ErrorOccurred += (s, e) =>
{
    Console.WriteLine($"Server error: {e.Exception.Message}");
};
```

### Cancellation Support

All async operations support cancellation tokens:

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30));

try
{
    await client.ConnectAsync("localhost", 5000, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Connection attempt timed out");
}
```

## Configuration

### Buffer Size

Customize the buffer size for different network conditions:

```csharp
// Default is 8192 bytes
var client = new TcpClientWrapper(bufferSize: 16384);
var server = new TcpServerWrapper(bufferSize: 16384);
```

### Message Format

Messages are encoded as UTF-8 JSON followed by a newline character. This allows for clear message boundaries and easy parsing.

## Performance Considerations

- **Async Operations**: All I/O operations are asynchronous, preventing thread blocking
- **Efficient Message Parsing**: Messages are parsed as complete lines
- **Configurable Buffer Size**: Adjust based on your message sizes
- **Connection Pooling**: Server automatically manages multiple client connections

## Thread Safety

- `TcpClientWrapper` and `TcpServerWrapper` are designed for single-threaded async usage
- Use proper synchronization if multiple threads access the same instance
- Event handlers are invoked on thread pool threads; ensure thread-safe event handling

## Limitations

- Messages must be complete (delimited by newline) before processing
- No built-in message ordering guarantee for very high-throughput scenarios
- No encryption or authentication (add your own layer if needed)
- Maximum message size is limited by available memory

## License

This library is provided as-is for educational and commercial use.
