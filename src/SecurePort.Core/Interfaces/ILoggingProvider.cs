using SecurePort.Core.Enums;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides structured logging capabilities for the application.
/// </summary>
public interface ILoggingProvider
{
    /// <summary>
    /// Logs a trace-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Trace(string message, params object[] args);

    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Debug(string message, params object[] args);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Info(string message, params object[] args);

    /// <summary>
    /// Logs a warning-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Warning(string message, params object[] args);

    /// <summary>
    /// Logs an error-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Error(string message, params object[] args);

    /// <summary>
    /// Logs an error-level message with an associated exception.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Error(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a critical-level message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Critical(string message, params object[] args);

    /// <summary>
    /// Logs a message at the specified level.
    /// </summary>
    /// <param name="level">The severity level of the message.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional format arguments for the message.</param>
    void Log(LogLevel level, string message, params object[] args);

    /// <summary>
    /// Gets the minimum log level currently being captured.
    /// </summary>
    LogLevel MinimumLevel { get; }

    /// <summary>
    /// Sets the minimum log level to capture.
    /// </summary>
    /// <param name="level">The minimum severity level.</param>
    void SetMinimumLevel(LogLevel level);
}
