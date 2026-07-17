namespace SecurePort.Core.Exceptions;

/// <summary>
/// Base exception class for all errors originating from the SecurePort application.
/// </summary>
public class SecurePortException : Exception
{
    /// <summary>
    /// Gets the error code that identifies the type of failure.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurePortException"/> class.
    /// </summary>
    public SecurePortException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurePortException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SecurePortException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurePortException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SecurePortException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurePortException"/> class with a specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code identifying the type of failure.</param>
    /// <param name="message">The message that describes the error.</param>
    public SecurePortException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurePortException"/> class with a specified error code, message, and inner exception.
    /// </summary>
    /// <param name="errorCode">The error code identifying the type of failure.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SecurePortException(string errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
