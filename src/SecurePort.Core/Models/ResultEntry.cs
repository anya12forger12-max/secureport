using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Represents a single result entry with extended metadata for results management.
/// </summary>
public sealed record ResultEntry
{
    /// <summary>Unique identifier for this result entry.</summary>
    public required Guid Id { get; init; }

    /// <summary>The scan session this result belongs to.</summary>
    public required Guid ScanId { get; init; }

    /// <summary>The hostname or IP address scanned.</summary>
    public required string Target { get; init; }

    /// <summary>The port number.</summary>
    public required int Port { get; init; }

    /// <summary>The network protocol used.</summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>The observed state of the port.</summary>
    public required PortState State { get; init; }

    /// <summary>Detected service name, if identified.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Brief description of the service.</summary>
    public string? ServiceDescription { get; init; }

    /// <summary>Time elapsed waiting for port response.</summary>
    public TimeSpan? ResponseTime { get; init; }

    /// <summary>Banner information captured from the service.</summary>
    public string? Banner { get; init; }

    /// <summary>Confidence level of the service detection (0-100).</summary>
    public required int DetectionConfidence { get; init; }

    /// <summary>User-assigned tags.</summary>
    public required IReadOnlyList<string> Tags { get; init; }

    /// <summary>User notes for this result.</summary>
    public string? Notes { get; init; }

    /// <summary>Whether this result is marked as a favorite.</summary>
    public required bool IsFavorite { get; init; }

    /// <summary>The scan profile name used for this scan.</summary>
    public string? ScanProfile { get; init; }

    /// <summary>The date component of the scan timestamp.</summary>
    public required DateTimeOffset ScanDate { get; init; }

    /// <summary>Duration of the scan that produced this result.</summary>
    public TimeSpan? ScanDuration { get; init; }
}
