using System.Collections.Immutable;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Utilities.Helpers;

/// <summary>
/// Provides a lookup table of well-known network services indexed by port number and protocol.
/// Used for service identification during and after scan operations.
/// </summary>
public static class ServiceLookup
{
    private static readonly ImmutableDictionary<int, ServiceInfo> TcpServices;
    private static readonly ImmutableDictionary<int, ServiceInfo> UdpServices;

    /// <summary>
    /// Static constructor initializes the service lookup tables with well-known services.
    /// </summary>
    static ServiceLookup()
    {
        var tcpBuilder = ImmutableDictionary<int, ServiceInfo>.Empty.ToBuilder();
        var udpBuilder = ImmutableDictionary<int, ServiceInfo>.Empty.ToBuilder();

        RegisterTcpService(tcpBuilder, 20, "FTP Data", "File Transfer Protocol data channel", ServiceCategory.FileTransfer);
        RegisterTcpService(tcpBuilder, 21, "FTP Control", "File Transfer Protocol control channel", ServiceCategory.FileTransfer);
        RegisterTcpService(tcpBuilder, 22, "SSH", "Secure Shell remote access protocol", ServiceCategory.Remote);
        RegisterTcpService(tcpBuilder, 23, "Telnet", "Telnet remote access protocol (unencrypted)", ServiceCategory.Remote);
        RegisterTcpService(tcpBuilder, 25, "SMTP", "Simple Mail Transfer Protocol", ServiceCategory.Mail);
        RegisterTcpService(tcpBuilder, 53, "DNS", "Domain Name System", ServiceCategory.System);
        RegisterTcpService(tcpBuilder, 80, "HTTP", "Hypertext Transfer Protocol", ServiceCategory.Web);
        RegisterTcpService(tcpBuilder, 110, "POP3", "Post Office Protocol version 3", ServiceCategory.Mail);
        RegisterTcpService(tcpBuilder, 111, "RPCbind", "Remote Procedure Call portmapper", ServiceCategory.System);
        RegisterTcpService(tcpBuilder, 135, "MSRPC", "Microsoft Remote Procedure Call", ServiceCategory.System);
        RegisterTcpService(tcpBuilder, 139, "NetBIOS", "NetBIOS Session Service", ServiceCategory.System);
        RegisterTcpService(tcpBuilder, 143, "IMAP", "Internet Message Access Protocol", ServiceCategory.Mail);
        RegisterTcpService(tcpBuilder, 443, "HTTPS", "Hypertext Transfer Protocol Secure", ServiceCategory.Web);
        RegisterTcpService(tcpBuilder, 445, "SMB", "Server Message Block", ServiceCategory.FileTransfer);
        RegisterTcpService(tcpBuilder, 993, "IMAPS", "IMAP over SSL/TLS", ServiceCategory.Mail);
        RegisterTcpService(tcpBuilder, 995, "POP3S", "POP3 over SSL/TLS", ServiceCategory.Mail);
        RegisterTcpService(tcpBuilder, 1433, "MSSQL", "Microsoft SQL Server", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 1434, "MSSQL Monitor", "Microsoft SQL Server Browser", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 1521, "Oracle DB", "Oracle Database Listener", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 3306, "MySQL", "MySQL Database", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 3389, "RDP", "Remote Desktop Protocol", ServiceCategory.Remote);
        RegisterTcpService(tcpBuilder, 5432, "PostgreSQL", "PostgreSQL Database", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 5900, "VNC", "Virtual Network Computing", ServiceCategory.Remote);
        RegisterTcpService(tcpBuilder, 6379, "Redis", "Redis Key-Value Store", ServiceCategory.Database);
        RegisterTcpService(tcpBuilder, 8080, "HTTP Alt", "HTTP Alternate (Proxy)", ServiceCategory.Web);
        RegisterTcpService(tcpBuilder, 8443, "HTTPS Alt", "HTTPS Alternate", ServiceCategory.Web);
        RegisterTcpService(tcpBuilder, 27017, "MongoDB", "MongoDB Database", ServiceCategory.Database);

        RegisterUdpService(udpBuilder, 53, "DNS", "Domain Name System", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 67, "DHCP", "Dynamic Host Configuration Protocol", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 68, "DHCP Client", "Dynamic Host Configuration Protocol Client", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 69, "TFTP", "Trivial File Transfer Protocol", ServiceCategory.FileTransfer);
        RegisterUdpService(udpBuilder, 123, "NTP", "Network Time Protocol", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 161, "SNMP", "Simple Network Management Protocol", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 500, "IKE", "Internet Key Exchange", ServiceCategory.Security);
        RegisterUdpService(udpBuilder, 514, "Syslog", "System Logging Protocol", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 1900, "SSDP", "Simple Service Discovery Protocol", ServiceCategory.System);
        RegisterUdpService(udpBuilder, 5353, "mDNS", "Multicast DNS", ServiceCategory.System);

        TcpServices = tcpBuilder.ToImmutable();
        UdpServices = udpBuilder.ToImmutable();
    }

    /// <summary>
    /// Looks up a well-known service by port number and protocol.
    /// </summary>
    /// <param name="port">The port number to look up.</param>
    /// <param name="protocol">The protocol to look up.</param>
    /// <returns>The <see cref="ServiceInfo"/> if found; otherwise, <c>null</c>.</returns>
    public static ServiceInfo? Lookup(int port, ProtocolType protocol)
    {
        var table = protocol == ProtocolType.TCP ? TcpServices : UdpServices;
        return table.TryGetValue(port, out var service) ? service : null;
    }

    /// <summary>
    /// Looks up a well-known service by port number for TCP.
    /// </summary>
    /// <param name="port">The port number to look up.</param>
    /// <returns>The <see cref="ServiceInfo"/> if found; otherwise, <c>null</c>.</returns>
    public static ServiceInfo? LookupTcp(int port)
    {
        return TcpServices.TryGetValue(port, out var service) ? service : null;
    }

    /// <summary>
    /// Looks up a well-known service by port number for UDP.
    /// </summary>
    /// <param name="port">The port number to look up.</param>
    /// <returns>The <see cref="ServiceInfo"/> if found; otherwise, <c>null</c>.</returns>
    public static ServiceInfo? LookupUdp(int port)
    {
        return UdpServices.TryGetValue(port, out var service) ? service : null;
    }

    /// <summary>
    /// Gets all registered TCP service definitions.
    /// </summary>
    /// <returns>An immutable dictionary mapping port numbers to service info.</returns>
    public static IReadOnlyDictionary<int, ServiceInfo> GetAllTcpServices()
    {
        return TcpServices;
    }

    /// <summary>
    /// Gets all registered UDP service definitions.
    /// </summary>
    /// <returns>An immutable dictionary mapping port numbers to service info.</returns>
    public static IReadOnlyDictionary<int, ServiceInfo> GetAllUdpServices()
    {
        return UdpServices;
    }

    /// <summary>
    /// Gets all services belonging to the specified category.
    /// </summary>
    /// <param name="category">The service category to filter by.</param>
    /// <returns>A read-only list of matching service definitions.</returns>
    public static IReadOnlyList<ServiceInfo> GetByCategory(ServiceCategory category)
    {
        return TcpServices.Values
            .Concat(UdpServices.Values)
            .Where(s => s.Category == category)
            .GroupBy(s => s.Port)
            .Select(g => g.First())
            .OrderBy(s => s.Port)
            .ToList();
    }

    /// <summary>
    /// Gets all unique service categories present in the lookup table.
    /// </summary>
    /// <returns>A read-only list of used <see cref="ServiceCategory"/> values.</returns>
    public static IReadOnlyList<ServiceCategory> GetCategories()
    {
        return TcpServices.Values
            .Select(s => s.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Registers a TCP service entry into the builder.
    /// </summary>
    private static void RegisterTcpService(
        ImmutableDictionary<int, ServiceInfo>.Builder builder,
        int port, string name, string description, ServiceCategory category)
    {
        builder[port] = new ServiceInfo
        {
            Port = port,
            Protocol = ProtocolType.TCP,
            ServiceName = name,
            Description = description,
            Category = category
        };
    }

    /// <summary>
    /// Registers a UDP service entry into the builder.
    /// </summary>
    private static void RegisterUdpService(
        ImmutableDictionary<int, ServiceInfo>.Builder builder,
        int port, string name, string description, ServiceCategory category)
    {
        builder[port] = new ServiceInfo
        {
            Port = port,
            Protocol = ProtocolType.UDP,
            ServiceName = name,
            Description = description,
            Category = category
        };
    }
}
