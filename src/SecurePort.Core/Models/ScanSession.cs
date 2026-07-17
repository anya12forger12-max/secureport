using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Represents a complete scan session with its configuration, results, and metadata.
/// </summary>
public sealed record ScanSession
{
    /// <summary>
    /// The unique identifier for this scan session.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The target configuration used for this scan.
    /// </summary>
    public required ScanTarget Target { get; init; }

    /// <summary>
    /// The collection of individual port scan results.
    /// </summary>
    public required IReadOnlyList<ScanResult> Results { get; init; }

    /// <summary>
    /// The current status of the scan session.
    /// </summary>
    public required ScanStatus Status { get; init; }

    /// <summary>
    /// The timestamp when the scan was started.
    /// </summary>
    public required DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// The timestamp when the scan finished, or null if still running.
    /// </summary>
    public DateTimeOffset? EndTime { get; init; }

    /// <summary>
    /// The total number of ports that were scanned.
    /// </summary>
    public required int TotalPortsScanned { get; init; }

    /// <summary>
    /// The number of ports found to be open.
    /// </summary>
    public required int OpenPortsFound { get; init; }

    /// <summary>
    /// The number of ports found to be filtered.
    /// </summary>
    public required int FilteredPortsFound { get; init; }

    /// <summary>
    /// The number of ports found to be closed.
    /// </summary>
    public required int ClosedPortsFound { get; init; }

    /// <summary>
    /// The number of errors encountered during the scan.
    /// </summary>
    public required int ErrorsEncountered { get; init; }

    /// <summary>
    /// The total duration of the scan, or null if not yet completed.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// An error message if the scan failed, or null if successful.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
