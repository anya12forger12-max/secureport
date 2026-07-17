namespace SecurePort.Core.Exceptions;

/// <summary>
/// Represents errors related to authentication, authorization, or cryptographic operations.
/// </summary>
public class SecurityException : SecurePortException
{
    /// <summary>
    /// Gets the security operation that failed, if specified.
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class.
    /// </summary>
    public SecurityException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the security error.</param>
    public SecurityException(string message) : base("SECURITY_ERROR", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the security error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SecurityException(string message, Exception innerException) : base("SECURITY_ERROR", message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with the failing operation and error message.
    /// </summary>
    /// <param name="operation">The security operation that failed.</param>
    /// <param name="message">The message that describes the security error.</param>
    public SecurityException(string operation, string message) : base("SECURITY_ERROR", message)
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityException"/> class with full context.
    /// </summary>
    /// <param name="operation">The security operation that failed.</param>
    /// <param name="message">The message that describes the security error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SecurityException(string operation, string message, Exception innerException) : base("SECURITY_ERROR", message, innerException)
    {
        Operation = operation;
    }
}
