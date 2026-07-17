using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Represents an immutable target specification for a network port scan.
/// </summary>
public sealed record ScanTarget
{
    /// <summary>
    /// The hostname or IP address to scan.
    /// </summary>
    public required string Host { get; init; }

    /// <summary>
    /// The first port number in the scan range (inclusive).
    /// </summary>
    public required int PortStart { get; init; }

    /// <summary>
    /// The last port number in the scan range (inclusive).
    /// </summary>
    public required int PortEnd { get; init; }

    /// <summary>
    /// The network protocol to use for scanning.
    /// </summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>
    /// The type of scan technique to apply.
    /// </summary>
    public required ScanType ScanType { get; init; }

    /// <summary>
    /// The maximum time to wait for a response from each port.
    /// </summary>
    public required TimeSpan Timeout { get; init; }

    /// <summary>
    /// The maximum number of concurrent connections allowed during the scan.
    /// </summary>
    public required int MaxConcurrentConnections { get; init; }

    /// <summary>
    /// The timestamp when this scan target was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
