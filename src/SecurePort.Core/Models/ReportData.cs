namespace SecurePort.Core.Models;

/// <summary>
/// Contains structured data prepared for report generation.
/// </summary>
public sealed record ReportData
{
    /// <summary>The report title.</summary>
    public required string Title { get; init; }

    /// <summary>The report generation timestamp.</summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Scan sessions included in the report.</summary>
    public required IReadOnlyList<ScanSession> Sessions { get; init; }

    /// <summary>Results included in the report.</summary>
    public required IReadOnlyList<ResultEntry> Results { get; init; }

    /// <summary>Statistics for the report.</summary>
    public required ResultsStatistics Statistics { get; init; }

    /// <summary>Chart data for visualizations.</summary>
    public required IReadOnlyList<ChartDataSet> Charts { get; init; }

    /// <summary>Any comparisons performed for the report.</summary>
    public IReadOnlyList<ScanComparisonResult>? Comparisons { get; init; }

    /// <summary>User notes included in the report.</summary>
    public IReadOnlyList<string>? Notes { get; init; }

    /// <summary>Tags filter applied to generate the report.</summary>
    public IReadOnlyList<string>? AppliedFilters { get; init; }
}
