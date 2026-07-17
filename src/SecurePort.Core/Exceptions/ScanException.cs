namespace SecurePort.Core.Exceptions;

/// <summary>
/// Represents errors that occur during network scan execution.
/// </summary>
public class ScanException : SecurePortException
{
    /// <summary>
    /// Gets the identifier of the scan session where the error occurred, if available.
    /// </summary>
    public Guid? SessionId { get; }

    /// <summary>
    /// Gets the port number being scanned when the error occurred, if applicable.
    /// </summary>
    public int? Port { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanException"/> class.
    /// </summary>
    public ScanException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ScanException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ScanException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanException"/> class with context about the failing scan.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the scan session.</param>
    /// <param name="message">The message that describes the error.</param>
    public ScanException(Guid sessionId, string message) : base("SCAN_ERROR", message)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanException"/> class with full scan context.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the scan session.</param>
    /// <param name="port">The port number being scanned when the error occurred.</param>
    /// <param name="message">The message that describes the error.</param>
    public ScanException(Guid sessionId, int port, string message) : base("SCAN_ERROR", message)
    {
        SessionId = sessionId;
        Port = port;
    }
}
