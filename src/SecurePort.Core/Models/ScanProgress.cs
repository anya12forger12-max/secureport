namespace SecurePort.Core.Models;

/// <summary>
/// Provides real-time progress information for an ongoing scan session.
/// </summary>
public sealed record ScanProgress
{
    /// <summary>
    /// The unique identifier of the scan session being tracked.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// The total number of ports to be scanned.
    /// </summary>
    public required int TotalPorts { get; init; }

    /// <summary>
    /// The number of ports scanned so far.
    /// </summary>
    public required int ScannedPorts { get; init; }

    /// <summary>
    /// The number of open ports discovered so far.
    /// </summary>
    public required int OpenPorts { get; init; }

    /// <summary>
    /// The port number currently being scanned.
    /// </summary>
    public required int CurrentPort { get; init; }

    /// <summary>
    /// The total elapsed time since the scan started.
    /// </summary>
    public required TimeSpan ElapsedTime { get; init; }

    /// <summary>
    /// The estimated time remaining until the scan completes, if calculable.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// The scan completion percentage from 0 to 100.
    /// </summary>
    public required double PercentageComplete { get; init; }
}
