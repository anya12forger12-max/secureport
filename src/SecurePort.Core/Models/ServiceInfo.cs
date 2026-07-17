using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Contains metadata about a well-known network service identified by port and protocol.
/// </summary>
public sealed record ServiceInfo
{
    /// <summary>
    /// The port number associated with this service.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// The protocol used by this service.
    /// </summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>
    /// The common name of the service.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// A brief description of the service and its purpose.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The functional category this service belongs to.
    /// </summary>
    public required ServiceCategory Category { get; init; }
}
