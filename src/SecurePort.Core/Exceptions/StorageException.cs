namespace SecurePort.Core.Exceptions;

/// <summary>
/// Represents errors that occur during data storage or retrieval operations.
/// </summary>
public class StorageException : SecurePortException
{
    /// <summary>
    /// Gets the path of the storage resource involved in the error, if applicable.
    /// </summary>
    public string? ResourcePath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class.
    /// </summary>
    public StorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the storage error.</param>
    public StorageException(string message) : base("STORAGE_ERROR", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the storage error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StorageException(string message, Exception innerException) : base("STORAGE_ERROR", message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with the resource path and error message.
    /// </summary>
    /// <param name="resourcePath">The file or resource path involved in the error.</param>
    /// <param name="message">The message that describes the storage error.</param>
    public StorageException(string resourcePath, string message) : base("STORAGE_ERROR", message)
    {
        ResourcePath = resourcePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageException"/> class with full context.
    /// </summary>
    /// <param name="resourcePath">The file or resource path involved in the error.</param>
    /// <param name="message">The message that describes the storage error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public StorageException(string resourcePath, string message, Exception innerException) : base("STORAGE_ERROR", message, innerException)
    {
        ResourcePath = resourcePath;
    }
}
