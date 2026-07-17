using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Validates scan target inputs before they are submitted to the scanner.
/// </summary>
public interface ITargetValidator
{
    /// <summary>
    /// Validates the specified scan target and returns any validation errors.
    /// </summary>
    /// <param name="target">The scan target to validate.</param>
    /// <returns>A collection of validation error messages, or an empty collection if valid.</returns>
    IReadOnlyList<string> Validate(ScanTarget target);

    /// <summary>
    /// Determines whether the specified scan target is valid.
    /// </summary>
    /// <param name="target">The scan target to validate.</param>
    /// <returns>True if the target passes all validation rules; otherwise, false.</returns>
    bool IsValid(ScanTarget target);

    /// <summary>
    /// Validates the host field of a scan target.
    /// </summary>
    /// <param name="host">The hostname or IP address to validate.</param>
    /// <returns>A validation error message, or null if valid.</returns>
    string? ValidateHost(string host);

    /// <summary>
    /// Validates a port range for a scan target.
    /// </summary>
    /// <param name="portStart">The starting port number.</param>
    /// <param name="portEnd">The ending port number.</param>
    /// <returns>A validation error message, or null if valid.</returns>
    string? ValidatePortRange(int portStart, int portEnd);
}
