using System.Net;
using System.Text.RegularExpressions;
using SecurePort.Core.Constants;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates scan targets (hostnames and IP addresses).
/// Implements both the existing ITargetValidator and the new ITargetInputValidator.
/// </summary>
public sealed class TargetValidator : ITargetInputValidator, ITargetValidator
{
    private static readonly Regex s_ipAddressRegex = new(
        @"^(\d{1,3}\.){3}\d{1,3}$", RegexOptions.Compiled);

    private static readonly Regex s_hostnameRegex = new(
        @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> s_reservedRanges = new()
    {
        "127.", "0.", "169.254.", "224.", "240.", "255."
    };

    // ITargetInputValidator (generic IValidator<string>)
    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Invalid("Target.Input", "ERR_TARGET_EMPTY",
                "Target cannot be null or empty.", "Provide a valid hostname or IP address.");

        value = value.Trim();

        if (value.Length > 253)
            return ValidationResult.Invalid("Target.Input", "ERR_TARGET_TOO_LONG",
                "Target exceeds maximum length of 253 characters.", "Use a shorter hostname or IP address.");

        if (value.Contains(".."))
            return ValidationResult.Invalid("Target.Input", "ERR_TARGET_INVALID_FORMAT",
                "Target contains consecutive dots.", "Remove consecutive dots from the target.");

        if (s_ipAddressRegex.IsMatch(value))
            return ValidateIpAddress(value);

        if (s_hostnameRegex.IsMatch(value))
            return ValidationResult.Valid("Target.Input");

        return ValidationResult.Invalid("Target.Input", "ERR_TARGET_INVALID_FORMAT",
            $"'{value}' is not a valid hostname or IP address.", "Provide a valid IPv4 address or hostname.");
    }

    // ITargetValidator (existing interface)
    public IReadOnlyList<string> Validate(ScanTarget target)
    {
        var errors = new List<string>();

        var hostResult = ValidateHost(target.Host);
        if (hostResult != null) errors.Add(hostResult);

        var portResult = ValidatePortRange(target.PortStart, target.PortEnd);
        if (portResult != null) errors.Add(portResult);

        if (target.Timeout <= TimeSpan.Zero)
            errors.Add("Timeout must be greater than zero.");

        if (target.MaxConcurrentConnections <= 0)
            errors.Add("Max concurrent connections must be greater than zero.");

        return errors;
    }

    public bool IsValid(ScanTarget target) => Validate(target).Count == 0;

    public string? ValidateHost(string host)
    {
        var result = Validate(host);
        return result.IsValid ? null : result.ErrorMessage;
    }

    public string? ValidatePortRange(int portStart, int portEnd)
    {
        if (portStart < AppConstants.MinPort || portStart > AppConstants.MaxPort)
            return $"Start port {portStart} is outside valid range ({AppConstants.MinPort}-{AppConstants.MaxPort}).";
        if (portEnd < AppConstants.MinPort || portEnd > AppConstants.MaxPort)
            return $"End port {portEnd} is outside valid range ({AppConstants.MinPort}-{AppConstants.MaxPort}).";
        if (portStart > portEnd)
            return $"Start port ({portStart}) must be less than or equal to end port ({portEnd}).";
        return null;
    }

    private static ValidationResult ValidateIpAddress(string ip)
    {
        var parts = ip.Split('.');
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var octet) || octet < 0 || octet > 255)
                return ValidationResult.Invalid("Target.Input", "ERR_TARGET_IP_OCTET",
                    $"Invalid IP address octet: {part}", "Ensure all octets are between 0 and 255.");
        }

        if (s_reservedRanges.Any(r => ip.StartsWith(r, StringComparison.Ordinal)))
            return ValidationResult.Invalid("Target.Input", "ERR_TARGET_RESERVED_IP",
                $"IP address {ip} is in a reserved range.", "Use a non-reserved IP address.");

        return ValidationResult.Valid("Target.Input");
    }
}
