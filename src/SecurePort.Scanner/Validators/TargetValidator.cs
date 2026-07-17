using System.Text.RegularExpressions;
using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Scanner.Validators;

/// <summary>
/// Validates scan target inputs including hostnames, port ranges, timeouts,
/// and concurrency limits. Supports domain names, IPv4, and IPv6 address formats
/// via regular expression matching.
/// </summary>
public sealed class TargetValidator : ITargetValidator
{
    /// <summary>
    /// Maximum allowed timeout in seconds to prevent excessively long per-port waits.
    /// </summary>
    private const int MaxTimeoutSeconds = 300;

    /// <summary>
    /// Maximum allowed concurrent connections per scan.
    /// </summary>
    private const int MaxConcurrentConnections = AppConstants.MaxConcurrentLimit;

    /// <summary>
    /// Regex pattern matching valid hostnames (e.g., example.com, sub.domain.co.uk),
    /// IPv4 addresses (e.g., 192.168.1.1), and IPv6 addresses (e.g., ::1, 2001:db8::1).
    /// </summary>
    private static readonly Regex HostnameRegex = new(
        @"^(" +
        @"([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z]{2,}" +
        @"|" +
        @"(\d{1,3}\.){3}\d{1,3}" +
        @"|" +
        @"[a-fA-F0-9:]+(:[a-fA-F0-9:]+)*" +
        @")$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(ScanTarget target)
    {
        if (target is null)
            return new[] { "Scan target must not be null." };

        var errors = new List<string>();

        var hostError = ValidateHost(target.Host);
        if (hostError is not null)
            errors.Add(hostError);

        var portError = ValidatePortRange(target.PortStart, target.PortEnd);
        if (portError is not null)
            errors.Add(portError);

        var timeoutError = ValidateTimeout(target.Timeout);
        if (timeoutError is not null)
            errors.Add(timeoutError);

        var concurrentError = ValidateConcurrentConnections(target.MaxConcurrentConnections);
        if (concurrentError is not null)
            errors.Add(concurrentError);

        return errors;
    }

    /// <inheritdoc />
    public bool IsValid(ScanTarget target) => Validate(target).Count == 0;

    /// <inheritdoc />
    public string? ValidateHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "Host must not be null or empty.";

        if (host.Length > 253)
            return "Host must not exceed 253 characters.";

        if (!HostnameRegex.IsMatch(host))
            return $"Host '{host}' is not a valid hostname, IPv4, or IPv6 address.";

        return null;
    }

    /// <inheritdoc />
    public string? ValidatePortRange(int portStart, int portEnd)
    {
        if (portStart < AppConstants.MinPort || portStart > AppConstants.MaxPort)
            return $"Port start must be between {AppConstants.MinPort} and {AppConstants.MaxPort}, got {portStart}.";

        if (portEnd < AppConstants.MinPort || portEnd > AppConstants.MaxPort)
            return $"Port end must be between {AppConstants.MinPort} and {AppConstants.MaxPort}, got {portEnd}.";

        if (portStart > portEnd)
            return $"Port start ({portStart}) must not exceed port end ({portEnd}).";

        return null;
    }

    /// <summary>
    /// Validates that the timeout is positive and does not exceed the maximum allowed value.
    /// </summary>
    /// <param name="timeout">The timeout value to validate.</param>
    /// <returns>An error message if invalid, or null if valid.</returns>
    private static string? ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            return "Timeout must be greater than zero.";

        if (timeout.TotalSeconds > MaxTimeoutSeconds)
            return $"Timeout must not exceed {MaxTimeoutSeconds} seconds, got {timeout.TotalSeconds:F1}s.";

        return null;
    }

    /// <summary>
    /// Validates that the concurrent connection count is within acceptable bounds.
    /// </summary>
    /// <param name="maxConcurrent">The maximum concurrent connections value to validate.</param>
    /// <returns>An error message if invalid, or null if valid.</returns>
    private static string? ValidateConcurrentConnections(int maxConcurrent)
    {
        if (maxConcurrent <= 0)
            return "Maximum concurrent connections must be greater than zero.";

        if (maxConcurrent > MaxConcurrentConnections)
            return $"Maximum concurrent connections must not exceed {MaxConcurrentConnections}, got {maxConcurrent}.";

        return null;
    }
}
