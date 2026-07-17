using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Generates chart-ready data sets from scan results for visualization.
/// </summary>
public interface IVisualizationEngine
{
    /// <summary>Generates open vs closed ports chart data.</summary>
    ChartDataSet GeneratePortStateChart(IReadOnlyList<ResultEntry> results);

    /// <summary>Generates service distribution chart data.</summary>
    ChartDataSet GenerateServiceDistributionChart(IReadOnlyList<ResultEntry> results);

    /// <summary>Generates port range distribution chart data.</summary>
    ChartDataSet GeneratePortRangeChart(IReadOnlyList<ResultEntry> results);

    /// <summary>Generates response time distribution chart data.</summary>
    ChartDataSet GenerateResponseTimeChart(IReadOnlyList<ResultEntry> results);

    /// <summary>Generates scan duration trend chart data.</summary>
    ChartDataSet GenerateScanDurationTrendChart(IReadOnlyList<ScanSession> sessions);

    /// <summary>Generates services frequency chart data.</summary>
    ChartDataSet GenerateServicesFrequencyChart(IReadOnlyList<ResultEntry> results);

    /// <summary>Generates historical scan timeline chart data.</summary>
    ChartDataSet GenerateHistoricalTimelineChart(IReadOnlyList<ScanSession> sessions);
}
