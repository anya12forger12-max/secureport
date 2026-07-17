using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Represents the difference for a single port between two scans.
/// </summary>
public sealed record PortDifference
{
    /// <summary>The port number.</summary>
    public required int Port { get; init; }

    /// <summary>The protocol.</summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>The previous state, or null if the port is new.</summary>
    public PortState? PreviousState { get; init; }

    /// <summary>The current state, or null if the port was removed.</summary>
    public PortState? CurrentState { get; init; }

    /// <summary>The previous service name.</summary>
    public string? PreviousService { get; init; }

    /// <summary>The current service name.</summary>
    public string? CurrentService { get; init; }

    /// <summary>The previous response time.</summary>
    public TimeSpan? PreviousResponseTime { get; init; }

    /// <summary>The current response time.</summary>
    public TimeSpan? CurrentResponseTime { get; init; }

    /// <summary>The type of change detected.</summary>
    public required DifferenceType DifferenceType { get; init; }
}

/// <summary>
/// Classifies the type of difference between two scan results for a port.
/// </summary>
public enum DifferenceType
{
    /// <summary>The port existed in both scans with no changes.</summary>
    Unchanged,

    /// <summary>The port is newly open in the current scan.</summary>
    NewlyOpen,

    /// <summary>The port was open but is now closed or unavailable.</summary>
    NewlyClosed,

    /// <summary>The service on the port has changed.</summary>
    ServiceChanged,

    /// <summary>The response time has changed significantly.</summary>
    ResponseTimeChanged,

    /// <summary>The port was present in the previous scan but not in the current scan.</summary>
    Removed,

    /// <summary>The port is present in the current scan but not in the previous scan.</summary>
    Added
}

/// <summary>
/// Represents a complete comparison between two scan sessions.
/// </summary>
public sealed record ScanComparisonResult
{
    /// <summary>The previous scan session ID.</summary>
    public required Guid PreviousScanId { get; init; }

    /// <summary>The current scan session ID.</summary>
    public required Guid CurrentScanId { get; init; }

    /// <summary>List of port-level differences.</summary>
    public required IReadOnlyList<PortDifference> Differences { get; init; }

    /// <summary>Number of newly open ports.</summary>
    public required int NewlyOpenCount { get; init; }

    /// <summary>Number of newly closed ports.</summary>
    public required int NewlyClosedCount { get; init; }

    /// <summary>Number of service changes.</summary>
    public required int ServiceChangesCount { get; init; }

    /// <summary>Number of unchanged ports.</summary>
    public required int UnchangedCount { get; init; }

    /// <summary>Difference in scan duration between the two scans.</summary>
    public TimeSpan? DurationDifference { get; init; }

    /// <summary>Timestamp when comparison was performed.</summary>
    public required DateTimeOffset ComparedAt { get; init; }
}
