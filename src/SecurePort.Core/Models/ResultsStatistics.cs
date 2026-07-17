namespace SecurePort.Core.Models;

/// <summary>
/// Contains computed statistics about scan results.
/// </summary>
public sealed record ResultsStatistics
{
    /// <summary>Total number of scans included in statistics.</summary>
    public required int TotalScans { get; init; }

    /// <summary>Total number of ports scanned across all scans.</summary>
    public required int TotalPortsScanned { get; init; }

    /// <summary>Total number of open ports found.</summary>
    public required int OpenPortsCount { get; init; }

    /// <summary>Total number of closed ports found.</summary>
    public required int ClosedPortsCount { get; init; }

    /// <summary>Total number of filtered/unknown ports.</summary>
    public required int OtherPortsCount { get; init; }

    /// <summary>Average scan duration across all completed scans.</summary>
    public TimeSpan? AverageScanDuration { get; init; }

    /// <summary>Average response time across all results with measured response times.</summary>
    public TimeSpan? AverageResponseTime { get; init; }

    /// <summary>The fastest recorded response time.</summary>
    public TimeSpan? FastestResponse { get; init; }

    /// <summary>The slowest recorded response time.</summary>
    public TimeSpan? SlowestResponse { get; init; }

    /// <summary>Most commonly detected services with their counts.</summary>
    public required IReadOnlyList<ServiceFrequency> MostCommonServices { get; init; }

    /// <summary>Most frequently scanned ports with their counts.</summary>
    public required IReadOnlyList<PortFrequency> MostFrequentPorts { get; init; }

    /// <summary>Scan frequency grouped by date.</summary>
    public required IReadOnlyList<DailyScanCount> ScanFrequencyOverTime { get; init; }
}

/// <summary>
/// Represents the frequency of a detected service across scan results.
/// </summary>
public sealed record ServiceFrequency
{
    /// <summary>The service name.</summary>
    public required string ServiceName { get; init; }

    /// <summary>The number of times this service was detected.</summary>
    public required int Count { get; init; }
}

/// <summary>
/// Represents how often a specific port has been scanned.
/// </summary>
public sealed record PortFrequency
{
    /// <summary>The port number.</summary>
    public required int Port { get; init; }

    /// <summary>The number of times this port was scanned.</summary>
    public required int Count { get; init; }
}

/// <summary>
/// Represents the number of scans performed on a specific day.
/// </summary>
public sealed record DailyScanCount
{
    /// <summary>The date.</summary>
    public required DateTimeOffset Date { get; init; }

    /// <summary>The number of scans performed on this date.</summary>
    public required int Count { get; init; }
}
