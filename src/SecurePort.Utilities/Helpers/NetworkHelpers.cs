using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SecurePort.Core.Constants;

namespace SecurePort.Utilities.Helpers;

/// <summary>
/// Provides static helper methods for network-related operations
/// including IP validation, hostname resolution, and port checks.
/// </summary>
public static class NetworkHelpers
{
    private static readonly Regex IpAddressRegex =
        new(@"^(\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled);

    private static readonly Regex HostnameRegex =
        new(@"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

    /// <summary>
    /// Determines whether the given string is a valid IPv4 address.
    /// Each octet is validated to be in the range 0-255.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid IPv4 address; otherwise, <c>false</c>.</returns>
    public static bool IsValidIPv4(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!IpAddressRegex.IsMatch(value))
            return false;

        return value.Split('.').All(octet =>
            int.TryParse(octet, out var value) && value >= 0 && value <= 255);
    }

    /// <summary>
    /// Determines whether the given string is a valid IPv6 address.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid IPv6 address; otherwise, <c>false</c>.</returns>
    public static bool IsValidIPv6(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return IPAddress.TryParse(value, out var address) && address.AddressFamily == AddressFamily.InterNetworkV6;
    }

    /// <summary>
    /// Determines whether the given string is a valid IP address (v4 or v6).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid IP address; otherwise, <c>false</c>.</returns>
    public static bool IsValidIpAddress(string value)
    {
        return IsValidIPv4(value) || IsValidIPv6(value);
    }

    /// <summary>
    /// Determines whether the given string is a valid hostname.
    /// Hostnames must start with a letter or digit, can contain hyphens,
    /// and each label must be 1-63 characters with a total length under 253.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid hostname; otherwise, <c>false</c>.</returns>
    public static bool IsValidHostname(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length > 253)
            return false;

        return HostnameRegex.IsMatch(value);
    }

    /// <summary>
    /// Determines whether the given string is a valid scan target
    /// (either an IP address or a hostname).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid scan target; otherwise, <c>false</c>.</returns>
    public static bool IsValidScanTarget(string value)
    {
        return IsValidIpAddress(value) || IsValidHostname(value);
    }

    /// <summary>
    /// Determines whether the given integer is a valid port number (1-65535).
    /// </summary>
    /// <param name="port">The port number to check.</param>
    /// <returns><c>true</c> if the port is in the valid range; otherwise, <c>false</c>.</returns>
    public static bool IsValidPort(int port)
    {
        return port >= AppConstants.MinPort && port <= AppConstants.MaxPort;
    }

    /// <summary>
    /// Parses a port range specification into a list of individual port numbers.
    /// Supports comma-separated values and dash-separated ranges.
    /// Examples: "80", "80-443", "22,80,443", "80-100,443,8080-8090".
    /// </summary>
    /// <param name="portRange">The port range string to parse.</param>
    /// <returns>An array of unique port numbers in ascending order.</returns>
    /// <exception cref="FormatException">Thrown when the port range format is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a port number is outside the valid range.</exception>
    public static int[] ParsePortRange(string portRange)
    {
        if (string.IsNullOrWhiteSpace(portRange))
            throw new FormatException("Port range must not be null or empty.");

        var ports = new HashSet<int>();
        var segments = portRange.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();

            if (trimmed.Contains('-'))
            {
                var parts = trimmed.Split('-', StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                    throw new FormatException($"Invalid port range segment: '{segment}'.");

                if (!int.TryParse(parts[0], out var start) || !IsValidPort(start))
                    throw new ArgumentOutOfRangeException(nameof(portRange), start, $"Port must be between {AppConstants.MinPort} and {AppConstants.MaxPort}.");

                if (!int.TryParse(parts[1], out var end) || !IsValidPort(end))
                    throw new ArgumentOutOfRangeException(nameof(portRange), end, $"Port must be between {AppConstants.MinPort} and {AppConstants.MaxPort}.");

                if (start > end)
                    throw new FormatException($"Start port {start} is greater than end port {end}.");

                for (var port = start; port <= end; port++)
                    ports.Add(port);
            }
            else
            {
                if (!int.TryParse(trimmed, out var port) || !IsValidPort(port))
                    throw new ArgumentOutOfRangeException(nameof(portRange), trimmed, "Invalid port number.");

                ports.Add(port);
            }
        }

        return ports.OrderBy(p => p).ToArray();
    }

    /// <summary>
    /// Attempts to parse a port range string without throwing exceptions.
    /// </summary>
    /// <param name="portRange">The port range string to parse.</param>
    /// <param name="ports">The parsed port numbers if successful; otherwise, an empty array.</param>
    /// <param name="error">An error message if parsing failed; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the port range was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParsePortRange(string portRange, out int[] ports, out string? error)
    {
        try
        {
            ports = ParsePortRange(portRange);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            ports = Array.Empty<int>();
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Asynchronously resolves a hostname to its IPv4 addresses.
    /// </summary>
    /// <param name="host">The hostname or IP address to resolve.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of resolved <see cref="IPAddress"/> instances.</returns>
    public static async Task<IReadOnlyList<IPAddress>> ResolveHostAsync(string host, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            return Array.Empty<IPAddress>();

        if (IPAddress.TryParse(host, out var directAddress))
            return new[] { directAddress };

        try
        {
            var results = await Dns.GetHostAddressesAsync(host, ct).ConfigureAwait(false);
            return results.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
        }
        catch
        {
            return Array.Empty<IPAddress>();
        }
    }

    /// <summary>
    /// Checks whether a given port is likely to be in use by testing a TCP connection.
    /// </summary>
    /// <param name="host">The target host.</param>
    /// <param name="port">The port number to check.</param>
    /// <param name="timeoutMs">The connection timeout in milliseconds.</param>
    /// <returns><c>true</c> if the port responded; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs = AppConstants.DefaultTimeoutMs)
    {
        if (string.IsNullOrWhiteSpace(host) || !IsValidPort(port))
            return false;

        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);

            var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

            if (completed == connectTask && client.Connected)
            {
                client.Close();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
