using System.Text;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Generates chart-ready data sets from scan results for visualization.
/// </summary>
public sealed class VisualizationEngine : IVisualizationEngine
{
    public ChartDataSet GeneratePortStateChart(IReadOnlyList<ResultEntry> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var stateGroups = results
            .GroupBy(r => r.State)
            .Select(g => new ChartDataPoint
            {
                Label = g.Key.ToString(),
                Value = g.Count(),
                Tooltip = $"{g.Key}: {g.Count()} ports"
            })
            .OrderByDescending(d => d.Value)
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Port state distribution: ");
        summary.Append(string.Join(", ", stateGroups.Select(d => $"{d.Label} ({d.Value})")));

        return new ChartDataSet
        {
            Title = "Open vs Closed Ports",
            Series = new[]
            {
                new ChartSeries { Name = "Port States", DataPoints = stateGroups }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GenerateServiceDistributionChart(IReadOnlyList<ResultEntry> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var serviceGroups = results
            .Where(r => !string.IsNullOrEmpty(r.ServiceName) && r.State == PortState.Open)
            .GroupBy(r => r.ServiceName!)
            .Select(g => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Count(),
                Tooltip = $"{g.Key}: {g.Count()} instances"
            })
            .OrderByDescending(d => d.Value)
            .ToList();

        if (serviceGroups.Count == 0)
        {
            serviceGroups.Add(new ChartDataPoint
            {
                Label = "No services detected",
                Value = 0,
                Tooltip = "No open services found"
            });
        }

        var summary = new StringBuilder();
        summary.Append("Service distribution: ");
        summary.Append(string.Join(", ", serviceGroups.Take(10).Select(d => $"{d.Label} ({d.Value})")));
        if (serviceGroups.Count > 10)
            summary.Append($" and {serviceGroups.Count - 10} more");

        return new ChartDataSet
        {
            Title = "Service Distribution",
            Series = new[]
            {
                new ChartSeries { Name = "Services", DataPoints = serviceGroups }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GeneratePortRangeChart(IReadOnlyList<ResultEntry> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var ranges = new (string Label, int Min, int Max)[]
        {
            ("1-1024", 1, 1024),
            ("1025-4096", 1025, 4096),
            ("4097-8192", 4097, 8192),
            ("8193-16384", 8193, 16384),
            ("16385-32768", 16385, 32768),
            ("32769-49152", 32769, 49152),
            ("49153-65535", 49153, 65535)
        };

        var dataPoints = ranges
            .Select(r => new ChartDataPoint
            {
                Label = r.Label,
                Value = results.Count(res => res.Port >= r.Min && res.Port <= r.Max),
                Tooltip = $"Ports {r.Label}: {results.Count(res => res.Port >= r.Min && res.Port <= r.Max)}"
            })
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Port range distribution: ");
        summary.Append(string.Join(", ", dataPoints.Where(d => d.Value > 0).Select(d => $"{d.Label} ({d.Value})")));

        return new ChartDataSet
        {
            Title = "Port Range Distribution",
            Series = new[]
            {
                new ChartSeries { Name = "Port Ranges", DataPoints = dataPoints }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GenerateResponseTimeChart(IReadOnlyList<ResultEntry> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var buckets = new (string Label, double MinMs, double MaxMs)[]
        {
            ("< 10ms", 0, 10),
            ("10-50ms", 10, 50),
            ("50-100ms", 50, 100),
            ("100-500ms", 100, 500),
            ("500ms-1s", 500, 1000),
            ("1-5s", 1000, 5000),
            ("> 5s", 5000, double.MaxValue)
        };

        var dataPoints = buckets
            .Select(b =>
            {
                var count = results.Count(r =>
                    r.ResponseTime.HasValue &&
                    r.ResponseTime.Value.TotalMilliseconds >= b.MinMs &&
                    r.ResponseTime.Value.TotalMilliseconds < b.MaxMs);
                return new ChartDataPoint
                {
                    Label = b.Label,
                    Value = count,
                    Tooltip = $"{b.Label}: {count} ports"
                };
            })
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Response time distribution: ");
        summary.Append(string.Join(", ", dataPoints.Where(d => d.Value > 0).Select(d => $"{d.Label} ({d.Value})")));

        return new ChartDataSet
        {
            Title = "Response Time Distribution",
            Series = new[]
            {
                new ChartSeries { Name = "Response Times", DataPoints = dataPoints }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GenerateScanDurationTrendChart(IReadOnlyList<ScanSession> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);

        var dataPoints = sessions
            .Where(s => s.Duration.HasValue && s.Status == ScanStatus.Completed)
            .OrderBy(s => s.StartTime)
            .Select(s => new ChartDataPoint
            {
                Label = s.StartTime.ToString("MM/dd HH:mm"),
                Value = s.Duration!.Value.TotalSeconds,
                Tooltip = $"{s.Target.Host}: {s.Duration.Value.TotalSeconds:F1}s"
            })
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Scan duration trend: ");
        if (dataPoints.Count > 0)
        {
            summary.Append($"{dataPoints.Count} scans, ");
            summary.Append($"average {dataPoints.Average(d => d.Value):F1}s");
        }
        else
        {
            summary.Append("No completed scans with duration data");
        }

        return new ChartDataSet
        {
            Title = "Scan Duration Trend",
            Series = new[]
            {
                new ChartSeries { Name = "Duration (seconds)", DataPoints = dataPoints }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GenerateServicesFrequencyChart(IReadOnlyList<ResultEntry> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var dataPoints = results
            .Where(r => !string.IsNullOrEmpty(r.ServiceName))
            .GroupBy(r => r.ServiceName!)
            .Select(g => new ChartDataPoint
            {
                Label = g.Key,
                Value = g.Count(),
                Tooltip = $"{g.Key}: {g.Count()} occurrences"
            })
            .OrderByDescending(d => d.Value)
            .Take(20)
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Services frequency: ");
        summary.Append(string.Join(", ", dataPoints.Take(5).Select(d => $"{d.Label} ({d.Value})")));
        if (dataPoints.Count > 5)
            summary.Append($" and {dataPoints.Count - 5} more");

        return new ChartDataSet
        {
            Title = "Services Frequency",
            Series = new[]
            {
                new ChartSeries { Name = "Frequency", DataPoints = dataPoints }
            },
            AccessibilitySummary = summary.ToString()
        };
    }

    public ChartDataSet GenerateHistoricalTimelineChart(IReadOnlyList<ScanSession> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);

        var dataPoints = sessions
            .Where(s => s.Status == ScanStatus.Completed)
            .OrderBy(s => s.StartTime)
            .GroupBy(s => s.StartTime.Date)
            .Select(g => new ChartDataPoint
            {
                Label = g.Key.ToString("yyyy-MM-dd"),
                Value = g.Count(),
                Tooltip = $"{g.Key:yyyy-MM-dd}: {g.Count()} scans"
            })
            .ToList();

        var summary = new StringBuilder();
        summary.Append("Historical scan timeline: ");
        if (dataPoints.Count > 0)
        {
            summary.Append($"{dataPoints.Sum(d => (int)d.Value)} total scans across {dataPoints.Count} days");
        }
        else
        {
            summary.Append("No completed scan data available");
        }

        return new ChartDataSet
        {
            Title = "Historical Scan Timeline",
            Series = new[]
            {
                new ChartSeries { Name = "Scans per Day", DataPoints = dataPoints }
            },
            AccessibilitySummary = summary.ToString()
        };
    }
}
