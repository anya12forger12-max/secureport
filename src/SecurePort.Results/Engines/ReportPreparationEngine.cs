using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Prepares structured report data from scan results for later export.
/// Does not perform actual file export (that is handled by Part 2B).
/// </summary>
public sealed class ReportPreparationEngine : IReportPreparationEngine
{
    private readonly IStatisticsEngine _statisticsEngine;
    private readonly IVisualizationEngine _visualizationEngine;
    private readonly IScanComparisonEngine _comparisonEngine;

    public ReportPreparationEngine(
        IStatisticsEngine statisticsEngine,
        IVisualizationEngine visualizationEngine,
        IScanComparisonEngine comparisonEngine)
    {
        _statisticsEngine = statisticsEngine ?? throw new ArgumentNullException(nameof(statisticsEngine));
        _visualizationEngine = visualizationEngine ?? throw new ArgumentNullException(nameof(visualizationEngine));
        _comparisonEngine = comparisonEngine ?? throw new ArgumentNullException(nameof(comparisonEngine));
    }

    public ReportData PrepareReport(
        string title,
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results,
        IReadOnlyList<string>? appliedFilters = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Report title cannot be null or empty.", nameof(title));

        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(results);

        var statistics = _statisticsEngine.ComputeStatistics(sessions, results);

        var charts = new List<ChartDataSet>
        {
            _visualizationEngine.GeneratePortStateChart(results),
            _visualizationEngine.GenerateServiceDistributionChart(results),
            _visualizationEngine.GeneratePortRangeChart(results),
            _visualizationEngine.GenerateResponseTimeChart(results),
            _visualizationEngine.GenerateServicesFrequencyChart(results)
        };

        if (sessions.Count > 1)
        {
            charts.Add(_visualizationEngine.GenerateScanDurationTrendChart(sessions));
            charts.Add(_visualizationEngine.GenerateHistoricalTimelineChart(sessions));
        }

        var notes = results
            .Where(r => !string.IsNullOrEmpty(r.Notes))
            .Select(r => r.Notes!)
            .ToList();

        return new ReportData
        {
            Title = title,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sessions = sessions,
            Results = results,
            Statistics = statistics,
            Charts = charts,
            Notes = notes.Count > 0 ? notes : null,
            AppliedFilters = appliedFilters
        };
    }

    public ReportData PrepareComparisonReport(
        string title,
        ScanSession previous,
        ScanSession current,
        IReadOnlyList<ResultEntry> previousResults,
        IReadOnlyList<ResultEntry> currentResults)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Report title cannot be null or empty.", nameof(title));

        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(previousResults);
        ArgumentNullException.ThrowIfNull(currentResults);

        var comparison = _comparisonEngine.Compare(previous, current);

        var allResults = previousResults.Concat(currentResults).ToList();
        var allSessions = new[] { previous, current };

        var statistics = _statisticsEngine.ComputeStatistics(allSessions, allResults);

        var charts = new List<ChartDataSet>
        {
            _visualizationEngine.GeneratePortStateChart(currentResults),
            _visualizationEngine.GenerateServiceDistributionChart(currentResults)
        };

        return new ReportData
        {
            Title = title,
            GeneratedAt = DateTimeOffset.UtcNow,
            Sessions = allSessions,
            Results = allResults,
            Statistics = statistics,
            Charts = charts,
            Comparisons = new[] { comparison }
        };
    }
}
