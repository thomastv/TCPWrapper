# JsonOverTCP.Tests

This is the unit and integration test project for the JsonOverTCP library.

## Test Structure

The test project is organized into the following categories:

### Core Tests (`Core/`)
- **JsonSerializerHelperTests.cs**: Tests for JSON serialization/deserialization functionality
- **PacketTests.cs**: Tests for the Packet data structure
- **MessageProtocolTests.cs**: Tests for message encoding/decoding and protocol handling

### Event Tests (`Events/`)
- **EventArgsTests.cs**: Tests for all event argument classes including:
  - ClientConnectedEventArgs
  - ClientDisconnectedEventArgs
  - MessageReceivedEventArgs
  - PacketReceivedEventArgs
  - ErrorEventArgs

### Integration Tests (`Integration/`)
- **TcpClientWrapperIntegrationTests.cs**: Integration tests for TCP client functionality
- **ServerClientIntegrationTests.cs**: End-to-end tests for client-server communication scenarios

## Running the Tests

### Using .NET CLI

Run all tests:
```bash
dotnet test
```

Run tests with detailed output:
```bash
dotnet test --verbosity normal
```

Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~JsonSerializerHelperTests"
```

### Using Visual Studio

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to execute all tests
3. Or right-click individual tests/test classes to run specific tests

### Using VS Code

1. Install the ".NET Core Test Explorer" extension
2. Tests will appear in the Test Explorer sidebar
3. Click the play button to run tests

## Test Categories

### Unit Tests
Unit tests focus on testing individual components in isolation:
- Core functionality (serialization, packets, protocol)
- Event argument classes
- Individual method behaviors

### Integration Tests
Integration tests verify complete scenarios involving network communication:
- Client connection and disconnection
- Message sending and receiving
- Server broadcasting
- Multi-client scenarios

**Note**: Integration tests require actual network connectivity and may take longer to execute.

## Dependencies

The test project uses the following testing frameworks and libraries:

- **XUnit 2.6.2**: Main testing framework
- **FluentAssertions 6.12.0**: For expressive assertions
- **Moq 4.20.70**: For creating mock objects (available for future use)
- **Microsoft.NET.Test.Sdk 17.8.0**: Test platform
- **coverlet.collector 6.0.0**: Code coverage collection

## Test Coverage

The test suite covers:
- ✅ JSON serialization and deserialization
- ✅ Packet creation and manipulation
- ✅ Message encoding and decoding
- ✅ Event argument classes
- ✅ Client-server connection establishment
- ✅ Message transmission (client to server and server to client)
- ✅ Broadcasting to multiple clients
- ✅ Disconnection handling
- ✅ Error scenarios

## CI/CD Integration

These tests can be easily integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
```

## Contributing

When adding new features to JsonOverTCP, please:
1. Add corresponding unit tests for the new functionality
2. Add integration tests if the feature involves network communication
3. Ensure all existing tests pass
4. Aim for high code coverage

## Troubleshooting

### Integration Tests Failing
- Ensure ports 15000-15001 are not in use by other applications
- Check firewall settings allow local TCP connections
- Some tests may be timing-sensitive; increase delay values if needed

### Build Errors
- Ensure .NET 10.0 SDK is installed
- Restore NuGet packages: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`
