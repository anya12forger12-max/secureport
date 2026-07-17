using SecurePort.Core.Exceptions;

namespace SecurePort.Networking.Exceptions;

/// <summary>
/// Represents errors that occur during networking operations such as
/// DNS resolution or socket communication.
/// </summary>
public class NetworkException : SecurePortException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class.
    /// </summary>
    public NetworkException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NetworkException(string message) : base("NETWORK_ERROR", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NetworkException(string message, Exception innerException) : base("NETWORK_ERROR", message, innerException)
    {
    }
}
