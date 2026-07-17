using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Represents the result of scanning an individual port on a target host.
/// </summary>
public sealed record ScanResult
{
    /// <summary>
    /// The hostname or IP address that was scanned.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// The port number that was scanned.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// The observed state of the port after scanning.
    /// </summary>
    public required PortState State { get; init; }

    /// <summary>
    /// The name of the service detected on the port, if identified.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Banner information captured from the service, if available.
    /// </summary>
    public string? BannerInfo { get; init; }

    /// <summary>
    /// The time elapsed waiting for the port response, if measured.
    /// </summary>
    public TimeSpan? ResponseTime { get; init; }

    /// <summary>
    /// The protocol used to scan this port.
    /// </summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>
    /// The timestamp when this port was scanned.
    /// </summary>
    public required DateTimeOffset ScannedAt { get; init; }
}
