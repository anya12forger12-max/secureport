namespace SecurePort.Core.Enums;

/// <summary>
/// Specifies the type of network scan to perform.
/// </summary>
public enum ScanType
{
    /// <summary>
    /// TCP Connect scan that completes the three-way handshake.
    /// </summary>
    ConnectScan,

    /// <summary>
    /// SYN half-open scan that does not complete the handshake.
    /// </summary>
    SYNScan,

    /// <summary>
    /// ICMP ping sweep to discover live hosts.
    /// </summary>
    PingSweep
}
