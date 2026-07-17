namespace SecurePort.Core.Enums;

/// <summary>
/// Specifies the file format used when exporting scan results.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JavaScript Object Notation format.
    /// </summary>
    JSON,

    /// <summary>
    /// Comma-Separated Values format.
    /// </summary>
    CSV,

    /// <summary>
    /// Extensible Markup Language format.
    /// </summary>
    XML,

    /// <summary>
    /// HyperText Markup Language report format.
    /// </summary>
    HTML,

    /// <summary>
    /// Portable Document Format report.
    /// </summary>
    PDF,

    /// <summary>
    /// Plain text format report.
    /// </summary>
    TXT,

    /// <summary>
    /// Markdown format report.
    /// </summary>
    Markdown
}
