namespace SecurePort.Core.Exceptions;

/// <summary>
/// Represents errors that occur when input data fails validation rules.
/// </summary>
public class ValidationException : SecurePortException
{
    /// <summary>
    /// Gets the name of the field that failed validation.
    /// </summary>
    public string? FieldName { get; }

    /// <summary>
    /// Gets the invalid value that was provided.
    /// </summary>
    public object? InvalidValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the validation failure.</param>
    public ValidationException(string message) : base("VALIDATION_ERROR", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the validation failure.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, Exception innerException) : base("VALIDATION_ERROR", message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class with the failing field and invalid value.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="invalidValue">The value that was provided.</param>
    /// <param name="message">The message that describes the validation failure.</param>
    public ValidationException(string fieldName, object? invalidValue, string message) : base("VALIDATION_ERROR", message)
    {
        FieldName = fieldName;
        InvalidValue = invalidValue;
    }
}
