using System.Net;
using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Scanner.Validators;

/// <summary>
/// Validates scan configuration fields beyond basic input validation. Performs security checks
/// such as rejecting loopback scans with warnings, ensuring target IPs are not multicast,
/// and enforcing port range size limits.
/// </summary>
public sealed class ScanConfigurationValidator
{
    private readonly ILoggingProvider _logger;
    private readonly ITargetValidator _targetValidator;

    /// <summary>
    /// Maximum number of ports allowed in a single scan to prevent resource exhaustion.
    /// </summary>
    private const int MaxPortRangeSize = 100_000;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanConfigurationValidator"/> class.
    /// </summary>
    /// <param name="targetValidator">The base target validator for standard validation.</param>
    /// <param name="logger">The logging provider for diagnostic output and warnings.</param>
    public ScanConfigurationValidator(ITargetValidator targetValidator, ILoggingProvider logger)
    {
        _targetValidator = targetValidator ?? throw new ArgumentNullException(nameof(targetValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Performs comprehensive validation of a scan target, including all base validation rules
    /// from <see cref="ITargetValidator"/> plus security-oriented configuration checks.
    /// </summary>
    /// <param name="target">The scan target to validate.</param>
    /// <returns>A collection of validation error messages, or an empty collection if valid.</returns>
    public IReadOnlyList<string> Validate(ScanTarget target)
    {
        if (target is null)
            return new[] { "Scan target must not be null." };

        var errors = new List<string>();

        var baseErrors = _targetValidator.Validate(target);
        errors.AddRange(baseErrors);

        var loopbackError = ValidateLoopback(target.Host);
        if (loopbackError is not null)
            errors.Add(loopbackError);

        var multicastError = ValidateMulticast(target.Host);
        if (multicastError is not null)
            errors.Add(multicastError);

        var portRangeError = ValidatePortRangeSize(target.PortStart, target.PortEnd);
        if (portRangeError is not null)
            errors.Add(portRangeError);

        return errors;
    }

    /// <summary>
    /// Determines whether the scan target is valid according to all validation rules.
    /// </summary>
    /// <param name="target">The scan target to validate.</param>
    /// <returns>True if the target passes all validation rules; otherwise, false.</returns>
    public bool IsValid(ScanTarget target) => Validate(target).Count == 0;

    /// <summary>
    /// Checks whether the target host resolves to a loopback address and emits a warning log.
    /// Loopback scans are permitted but flagged as potentially unintended.
    /// </summary>
    /// <param name="host">The hostname or IP address to check.</param>
    /// <returns>A warning message if the host is a loopback address, or null otherwise.</returns>
    private string? ValidateLoopback(string host)
    {
        if (!IPAddress.TryParse(host, out var address))
            return null;

        if (!IPAddress.IsLoopback(address))
            return null;

        _logger.Warning("Scan target '{Host}' is a loopback address. Ensure this is intentional.", host);
        return $"Target '{host}' is a loopback address. Scanning loopback interfaces may not represent external network posture.";
    }

    /// <summary>
    /// Checks whether the target host resolves to a multicast address, which is invalid for scanning.
    /// </summary>
    /// <param name="host">The hostname or IP address to check.</param>
    /// <returns>An error message if the host is a multicast address, or null otherwise.</returns>
    private static string? ValidateMulticast(string host)
    {
        if (!IPAddress.TryParse(host, out var address))
            return null;

        if (!IsMulticastAddress(address))
            return null;

        return $"Target '{host}' is a multicast address and cannot be scanned.";
    }

    /// <summary>
    /// Determines whether the specified IP address falls within the multicast range.
    /// </summary>
    /// <param name="address">The IP address to check.</param>
    /// <returns>True if the address is multicast; otherwise, false.</returns>
    private static bool IsMulticastAddress(IPAddress address)
    {
        var bytes = address.GetAddressBytes();

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && bytes.Length == 4)
        {
            return bytes[0] >= 224 && bytes[0] <= 239;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && bytes.Length == 16)
        {
            return bytes[0] == 0xFF;
        }

        return false;
    }

    /// <summary>
    /// Validates that the port range does not exceed the maximum allowed scan size.
    /// </summary>
    /// <param name="portStart">The starting port number.</param>
    /// <param name="portEnd">The ending port number.</param>
    /// <returns>An error message if the range exceeds the limit, or null if valid.</returns>
    private static string? ValidatePortRangeSize(int portStart, int portEnd)
    {
        var rangeSize = portEnd - portStart + 1;

        if (rangeSize > MaxPortRangeSize)
            return $"Port range of {rangeSize} exceeds maximum allowed size of {MaxPortRangeSize} ports.";

        return null;
    }
}
