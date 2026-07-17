using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Prepares structured report data from scan results without performing the actual export.
/// </summary>
public interface IReportPreparationEngine
{
    /// <summary>
    /// Prepares report data from the specified scan sessions and results.
    /// </summary>
    ReportData PrepareReport(
        string title,
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results,
        IReadOnlyList<string>? appliedFilters = null);

    /// <summary>
    /// Prepares a comparison report from two scan sessions.
    /// </summary>
    ReportData PrepareComparisonReport(
        string title,
        ScanSession previous,
        ScanSession current,
        IReadOnlyList<ResultEntry> previousResults,
        IReadOnlyList<ResultEntry> currentResults);
}
