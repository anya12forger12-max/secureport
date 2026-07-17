using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Computes comprehensive statistics from scan sessions and result entries.
/// </summary>
public sealed class StatisticsEngine : IStatisticsEngine
{
    public ResultsStatistics ComputeStatistics(
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results)
    {
        if (sessions is null)
            throw new ArgumentNullException(nameof(sessions));

        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var completedSessions = sessions.Where(s => s.Status == ScanStatus.Completed).ToList();

        var totalScans = completedSessions.Count;
        var totalPortsScanned = results.Count;
        var openCount = results.Count(r => r.State == PortState.Open);
        var closedCount = results.Count(r => r.State == PortState.Closed);
        var otherCount = totalPortsScanned - openCount - closedCount;

        var durations = completedSessions
            .Where(s => s.Duration.HasValue)
            .Select(s => s.Duration!.Value)
            .ToList();

        var averageDuration = durations.Count > 0
            ? durations.Aggregate((a, b) => a + b) / durations.Count
            : (TimeSpan?)null;

        var responseTimes = results
            .Where(r => r.ResponseTime.HasValue)
            .Select(r => r.ResponseTime!.Value)
            .ToList();

        var avgResponseTime = responseTimes.Count > 0
            ? responseTimes.Aggregate((a, b) => a + b) / responseTimes.Count
            : (TimeSpan?)null;

        var fastest = responseTimes.Count > 0 ? responseTimes.Min() : (TimeSpan?)null;
        var slowest = responseTimes.Count > 0 ? responseTimes.Max() : (TimeSpan?)null;

        var mostCommonServices = results
            .Where(r => !string.IsNullOrEmpty(r.ServiceName))
            .GroupBy(r => r.ServiceName!)
            .Select(g => new ServiceFrequency { ServiceName = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .ToList();

        var mostFrequentPorts = results
            .GroupBy(r => r.Port)
            .Select(g => new PortFrequency { Port = g.Key, Count = g.Count() })
            .OrderByDescending(p => p.Count)
            .ToList();

        var scanFrequency = completedSessions
            .GroupBy(s => s.StartTime.Date)
            .Select(g => new DailyScanCount
            {
                Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                Count = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new ResultsStatistics
        {
            TotalScans = totalScans,
            TotalPortsScanned = totalPortsScanned,
            OpenPortsCount = openCount,
            ClosedPortsCount = closedCount,
            OtherPortsCount = otherCount,
            AverageScanDuration = averageDuration,
            AverageResponseTime = avgResponseTime,
            FastestResponse = fastest,
            SlowestResponse = slowest,
            MostCommonServices = mostCommonServices,
            MostFrequentPorts = mostFrequentPorts,
            ScanFrequencyOverTime = scanFrequency
        };
    }
}
