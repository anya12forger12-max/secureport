namespace SecurePort.Core.Enums;

/// <summary>
/// Represents the observed state of a port after scanning.
/// </summary>
public enum PortState
{
    /// <summary>
    /// The port is open and accepting connections.
    /// </summary>
    Open,

    /// <summary>
    /// The port is closed and rejecting connections.
    /// </summary>
    Closed,

    /// <summary>
    /// The port state could not be determined due to filtering.
    /// </summary>
    Filtered,

    /// <summary>
    /// The port state is unknown.
    /// </summary>
    Unknown,

    /// <summary>
    /// The port did not respond within the timeout period.
    /// </summary>
    Timeout
}
