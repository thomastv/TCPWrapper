using System;

namespace JsonOverTCP.Core;

/// <summary>
/// Handles streaming message parsing with delimiter-based message framing.
/// Efficiently manages message buffers and extracts complete messages from TCP streams.
/// </summary>
public class MessageStreamParser
{
    private byte[] _messageBuffer;
    private int _messageBufferStart = 0;
    private int _messageBufferLength = 0;

    /// <summary>
    /// Initializes a new instance of the MessageStreamParser.
    /// </summary>
    /// <param name="initialBufferSize">Initial buffer size for message accumulation.</param>
    public MessageStreamParser(int initialBufferSize = 16384)
    {
        _messageBuffer = new byte[initialBufferSize];
    }

    /// <summary>
    /// Adds new data to the parser and extracts any complete messages.
    /// </summary>
    /// <param name="newData">The new data received from the stream.</param>
    /// <param name="onMessageFound">Callback invoked for each complete message found.</param>
    public void ProcessData(ReadOnlySpan<byte> newData, Action<ReadOnlySpan<byte>> onMessageFound)
    {
        if (newData.Length == 0)
            return;

        // Ensure we have enough space in the message buffer
        EnsureBufferCapacity(newData.Length);

        // Copy new data to message buffer
        newData.CopyTo(_messageBuffer.AsSpan(_messageBufferStart + _messageBufferLength));
        _messageBufferLength += newData.Length;

        // Extract complete messages
        ExtractCompleteMessages(onMessageFound);
    }

    /// <summary>
    /// Processes any remaining partial data in the buffer after a stream ends.
    /// </summary>
    /// <param name="onMessageFound">Callback invoked for the final message if it exists and is non-empty.</param>
    public void ProcessRemainingData(Action<ReadOnlySpan<byte>> onMessageFound)
    {
        if (_messageBufferLength > 0)
        {
            var remainingData = _messageBuffer.AsSpan(_messageBufferStart, _messageBufferLength);
            if (!remainingData.IsEmpty)
            {
                onMessageFound(remainingData);
            }
            Clear();
        }
    }

    /// <summary>
    /// Clears all buffered data.
    /// </summary>
    public void Clear()
    {
        _messageBufferStart = 0;
        _messageBufferLength = 0;
    }

    /// <summary>
    /// Gets the current number of bytes in the buffer waiting to be processed.
    /// </summary>
    public int BufferedBytes => _messageBufferLength;

    private void EnsureBufferCapacity(int additionalBytes)
    {
        int requiredSpace = _messageBufferLength + additionalBytes;

        // Check if we need to compact or grow the buffer
        if (requiredSpace > _messageBuffer.Length - _messageBufferStart)
        {
            // Compact the buffer by moving data to the beginning
            if (_messageBufferLength > 0 && _messageBufferStart > 0)
            {
                var validData = _messageBuffer.AsSpan(_messageBufferStart, _messageBufferLength);
                validData.CopyTo(_messageBuffer.AsSpan());
                _messageBufferStart = 0;
            }
            
            // If still not enough space, grow the buffer
            if (requiredSpace > _messageBuffer.Length)
            {
                var newSize = Math.Max(_messageBuffer.Length * 2, requiredSpace);
                var newBuffer = new byte[newSize];
                
                if (_messageBufferLength > 0)
                {
                    _messageBuffer.AsSpan(_messageBufferStart, _messageBufferLength).CopyTo(newBuffer);
                }
                
                _messageBuffer = newBuffer;
                _messageBufferStart = 0;
            }
        }
    }

    private void ExtractCompleteMessages(Action<ReadOnlySpan<byte>> onMessageFound)
    {
        while (_messageBufferLength > 0)
        {
            var currentData = _messageBuffer.AsSpan(_messageBufferStart, _messageBufferLength);
            int delimiterIndex = currentData.IndexOf(MessageProtocol.MessageDelimiterByte);
            
            if (delimiterIndex < 0)
                break;

            // Extract message (excluding delimiter)
            if (delimiterIndex > 0)
            {
                var messageSpan = currentData[..delimiterIndex];
                onMessageFound(messageSpan);
            }

            // Efficiently "remove" processed data by advancing the start position
            int bytesToRemove = delimiterIndex + 1;
            _messageBufferStart += bytesToRemove;
            _messageBufferLength -= bytesToRemove;
        }
    }
}