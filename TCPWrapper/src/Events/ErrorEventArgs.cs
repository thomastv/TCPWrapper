namespace JsonOverTCP;

/// <summary>
/// Event arguments for errors.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ErrorEventArgs class.
/// </remarks>
public class ErrorEventArgs(Exception exception) : EventArgs
{
    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; } = exception;
}
