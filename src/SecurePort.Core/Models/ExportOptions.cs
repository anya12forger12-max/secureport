using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Specifies the options and format for exporting scan results to a file.
/// </summary>
public sealed record ExportOptions
{
    /// <summary>
    /// The file format to use for the exported report.
    /// </summary>
    public required ExportFormat Format { get; init; }

    /// <summary>
    /// Whether to include closed ports in the exported results.
    /// </summary>
    public required bool IncludeClosedPorts { get; init; }

    /// <summary>
    /// Whether to include timestamp information for each port result.
    /// </summary>
    public required bool IncludeTimestamps { get; init; }

    /// <summary>
    /// The destination file path for the exported report.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Whether to include scan session metadata such as duration and configuration.
    /// </summary>
    public required bool IncludeScanMetadata { get; init; }
}
