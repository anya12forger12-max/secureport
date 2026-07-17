namespace SecurePort.Core.Enums;

/// <summary>
/// Defines the severity levels for application logging.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Highly detailed tracing information for diagnostics.
    /// </summary>
    Trace,

    /// <summary>
    /// Diagnostic information useful during development.
    /// </summary>
    Debug,

    /// <summary>
    /// General operational messages about application flow.
    /// </summary>
    Info,

    /// <summary>
    /// Potential issues that do not prevent normal operation.
    /// </summary>
    Warning,

    /// <summary>
    /// Errors that prevent a specific operation from completing.
    /// </summary>
    Error,

    /// <summary>
    /// Critical failures that may cause application termination.
    /// </summary>
    Critical
}
