using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates port numbers and port ranges.
/// </summary>
public sealed class PortValidator : IPortValidator
{
    public ValidationResult ValidatePort(int port)
    {
        if (port < AppConstants.MinPort || port > AppConstants.MaxPort)
        {
            return ValidationResult.Invalid(
                "Port.Input",
                "ERR_PORT_RANGE",
                $"Port {port} is outside the valid range ({AppConstants.MinPort}-{AppConstants.MaxPort}).",
                $"Provide a port number between {AppConstants.MinPort} and {AppConstants.MaxPort}.");
        }

        return ValidationResult.Valid("Port.Input");
    }

    public ValidationResult ValidatePortRange(int start, int end)
    {
        var startResult = ValidatePort(start);
        if (!startResult.IsValid) return startResult;

        var endResult = ValidatePort(end);
        if (!endResult.IsValid) return endResult;

        if (start > end)
        {
            return ValidationResult.Invalid(
                "Port.Range",
                "ERR_PORT_RANGE_ORDER",
                $"Start port ({start}) must be less than or equal to end port ({end}).",
                "Swap the start and end ports, or adjust the range.");
        }

        return ValidationResult.Valid("Port.Range");
    }

    public ValidationResult Validate(string value)
    {
        if (int.TryParse(value, out var port))
        {
            return ValidatePort(port);
        }

        if (value.Contains('-'))
        {
            var parts = value.Split('-', 2);
            if (int.TryParse(parts[0].Trim(), out var start) && int.TryParse(parts[1].Trim(), out var end))
            {
                return ValidatePortRange(start, end);
            }
        }

        return ValidationResult.Invalid(
            "Port.Input",
            "ERR_PORT_FORMAT",
            $"'{value}' is not a valid port number or range.",
            "Provide a single port number (e.g., 80) or a range (e.g., 1-1024).");
    }
}
