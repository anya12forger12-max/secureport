namespace SecurePort.Core.Models;

/// <summary>
/// Defines criteria for searching stored reports.
/// </summary>
public sealed record ReportSearchCriteria
{
    /// <summary>Filter by date range start (inclusive).</summary>
    public DateTimeOffset? DateStart { get; init; }

    /// <summary>Filter by date range end (inclusive).</summary>
    public DateTimeOffset? DateEnd { get; init; }

    /// <summary>Filter by target host (partial match).</summary>
    public string? TargetHost { get; init; }

    /// <summary>Filter by report ID.</summary>
    public Guid? ReportId { get; init; }

    /// <summary>Filter by scan session ID.</summary>
    public Guid? ScanId { get; init; }

    /// <summary>Filter by export format.</summary>
    public Enums.ExportFormat? Format { get; init; }

    /// <summary>Filter by scan profile (partial match).</summary>
    public string? ScanProfile { get; init; }

    /// <summary>Search keywords (matched against title and tags).</summary>
    public string? Keywords { get; init; }

    /// <summary>Filter by tag.</summary>
    public string? Tag { get; init; }

    /// <summary>Filter by archived status.</summary>
    public bool? IsArchived { get; init; }

    /// <summary>Sort field for results.</summary>
    public ReportSortField SortBy { get; init; } = ReportSortField.CreatedAt;

    /// <summary>Whether to sort descending.</summary>
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Fields available for sorting report metadata.
/// </summary>
public enum ReportSortField
{
    CreatedAt,
    Title,
    TargetHost,
    Format,
    FileSizeBytes
}
