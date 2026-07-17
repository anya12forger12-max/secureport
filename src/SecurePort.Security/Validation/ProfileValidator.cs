using System.Text.RegularExpressions;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates scan profile names.
/// </summary>
public sealed class ProfileValidator : IProfileValidator
{
    private static readonly Regex s_validProfileRegex = new(
        @"^[a-zA-Z0-9_\-\.]{1,64}$", RegexOptions.Compiled);

    private static readonly HashSet<string> s_reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "default", "system", "internal", "admin", "root", "null", "none"
    };

    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Invalid(
                "Profile.Input",
                "ERR_PROFILE_EMPTY",
                "Profile name cannot be null or empty.",
                "Provide a valid profile name.");
        }

        value = value.Trim();

        if (value.Length > 64)
        {
            return ValidationResult.Invalid(
                "Profile.Input",
                "ERR_PROFILE_TOO_LONG",
                "Profile name exceeds maximum length of 64 characters.",
                "Use a shorter profile name.");
        }

        if (!s_validProfileRegex.IsMatch(value))
        {
            return ValidationResult.Invalid(
                "Profile.Input",
                "ERR_PROFILE_INVALID_CHARS",
                "Profile name contains invalid characters.",
                "Use only alphanumeric characters, hyphens, underscores, and dots.");
        }

        if (s_reservedNames.Contains(value))
        {
            return ValidationResult.Invalid(
                "Profile.Input",
                "ERR_PROFILE_RESERVED",
                $"'{value}' is a reserved profile name.",
                "Choose a different name.");
        }

        return ValidationResult.Valid("Profile.Input");
    }
}
