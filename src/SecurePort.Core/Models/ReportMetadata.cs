namespace SecurePort.Core.Models;

/// <summary>
/// Contains metadata about a stored report for search and organization.
/// </summary>
public sealed record ReportMetadata
{
    /// <summary>Unique identifier for this report.</summary>
    public required Guid ReportId { get; init; }

    /// <summary>The display title of the report.</summary>
    public required string Title { get; init; }

    /// <summary>When the report was created.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>The application version that generated this report.</summary>
    public required string ApplicationVersion { get; init; }

    /// <summary>The operating system platform.</summary>
    public required string OperatingSystem { get; init; }

    /// <summary>The export format used.</summary>
    public required Enums.ExportFormat Format { get; init; }

    /// <summary>The file size in bytes.</summary>
    public required long FileSizeBytes { get; init; }

    /// <summary>SHA-256 checksum of the report file.</summary>
    public required string Checksum { get; init; }

    /// <summary>The scan session ID this report is based on.</summary>
    public required Guid ScanId { get; init; }

    /// <summary>The scan profile used.</summary>
    public string? ScanProfile { get; init; }

    /// <summary>The target host scanned.</summary>
    public required string TargetHost { get; init; }

    /// <summary>File path where the report is stored.</summary>
    public required string FilePath { get; init; }

    /// <summary>Whether this report has been archived.</summary>
    public required bool IsArchived { get; init; }

    /// <summary>User-assigned tags.</summary>
    public required IReadOnlyList<string> Tags { get; init; }
}
