using System.Text.RegularExpressions;
using SecurePort.Core.Constants;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Utilities.Extensions;

/// <summary>
/// Provides extension methods for string validation and formatting
/// used throughout the SecurePort application.
/// </summary>
public static class StringExtensions
{
    private static readonly Regex IpAddressPattern =
        new(@"^(\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled);

    private static readonly Regex HostnamePattern =
        new(@"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

    private static readonly Regex PortRangePattern =
        new(@"^(\d+)(?:\s*[-,]\s*(\d+))?$", RegexOptions.Compiled);

    /// <summary>
    /// Determines whether the string is a valid IPv4 address.
    /// Each octet must be between 0 and 255.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns><c>true</c> if the string is a valid IPv4 address; otherwise, <c>false</c>.</returns>
    public static bool IsIpAddress(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!IpAddressPattern.IsMatch(value))
            return false;

        var octets = value.Split('.');
        return octets.All(o => int.TryParse(o, out var v) && v >= 0 && v <= 255);
    }

    /// <summary>
    /// Determines whether the string is a valid hostname (domain name).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns><c>true</c> if the string is a valid hostname; otherwise, <c>false</c>.</returns>
    public static bool IsHostname(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length > 253)
            return false;

        return HostnamePattern.IsMatch(value);
    }

    /// <summary>
    /// Determines whether the string is a valid scan target (IP address or hostname).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns><c>true</c> if the string is a valid scan target; otherwise, <c>false</c>.</returns>
    public static bool IsValidScanTarget(this string value)
    {
        return value.IsIpAddress() || value.IsHostname();
    }

    /// <summary>
    /// Parses a port range string (e.g. "80", "80-443", "80,443,8080") into
    /// a list of individual port numbers.
    /// </summary>
    /// <param name="value">The port range string to parse.</param>
    /// <returns>An array of port numbers extracted from the range string.</returns>
    /// <exception cref="FormatException">Thrown when the string contains invalid port values.</exception>
    public static int[] ParsePortRange(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException("Port range string must not be null or empty.");

        var ports = new List<int>();
        var segments = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segment in segments)
        {
            var match = PortRangePattern.Match(segment.Trim());
            if (!match.Success)
                throw new FormatException($"Invalid port range segment: '{segment}'.");

            var start = int.Parse(match.Groups[1].Value);
            var end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : start;

            if (start < 1 || start > AppConstants.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(value), start, "Port number must be between 1 and 65535.");
            if (end < 1 || end > AppConstants.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(value), end, "Port number must be between 1 and 65535.");
            if (start > end)
                throw new FormatException($"Invalid port range: start port {start} is greater than end port {end}.");

            for (var port = start; port <= end; port++)
                ports.Add(port);
        }

        return ports.Distinct().OrderBy(p => p).ToArray();
    }

    /// <summary>
    /// Validates whether a string represents a valid port number (1-65535).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <returns><c>true</c> if the string represents a valid port; otherwise, <c>false</c>.</returns>
    public static bool IsValidPort(this string value)
    {
        return int.TryParse(value, out var port) && port >= AppConstants.MinPort && port <= AppConstants.MaxPort;
    }

    /// <summary>
    /// Truncates the string to the specified maximum length and appends an ellipsis
    /// if truncation occurs.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum number of characters (including ellipsis).</param>
    /// <returns>The truncated string.</returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return maxLength <= 3
            ? value[..maxLength]
            : value[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Converts a <see cref="PortState"/> enum value to a human-readable display string.
    /// </summary>
    /// <param name="state">The port state to format.</param>
    /// <returns>The display string for the port state.</returns>
    public static string ToDisplayString(this PortState state)
    {
        return state switch
        {
            PortState.Open => "Open",
            PortState.Closed => "Closed",
            PortState.Filtered => "Filtered",
            PortState.Timeout => "Timed Out",
            PortState.Unknown => "Unknown",
            _ => state.ToString()
        };
    }

    /// <summary>
    /// Converts a <see cref="ScanStatus"/> enum value to a human-readable display string.
    /// </summary>
    /// <param name="status">The scan status to format.</param>
    /// <returns>The display string for the scan status.</returns>
    public static string ToDisplayString(this ScanStatus status)
    {
        return status switch
        {
            ScanStatus.Pending => "Pending",
            ScanStatus.Running => "Running",
            ScanStatus.Paused => "Paused",
            ScanStatus.Completed => "Completed",
            ScanStatus.Failed => "Failed",
            ScanStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    /// <summary>
    /// Converts a <see cref="ProtocolType"/> to a user-friendly name.
    /// </summary>
    /// <param name="protocol">The protocol type.</param>
    /// <returns>The display name (e.g. "TCP", "UDP").</returns>
    public static string ToDisplayString(this ProtocolType protocol)
    {
        return protocol switch
        {
            ProtocolType.TCP => "TCP",
            ProtocolType.UDP => "UDP",
            _ => protocol.ToString()
        };
    }
}
