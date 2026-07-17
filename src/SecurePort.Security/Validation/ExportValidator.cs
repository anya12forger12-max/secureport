using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates export destination paths.
/// </summary>
public sealed class ExportValidator : IExportValidator
{
    private static readonly HashSet<string> s_blockedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/etc", "/boot", "/sys", "/proc", "/dev",
        "C:\\Windows", "C:\\System32", "C:\\Program Files"
    };

    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Invalid(
                "Export.Path",
                "ERR_EXPORT_PATH_EMPTY",
                "Export destination cannot be empty.",
                "Provide a valid file path.");
        }

        value = value.Trim();

        if (value.Contains(".."))
        {
            return ValidationResult.Invalid(
                "Export.Path",
                "ERR_EXPORT_PATH_TRAVERSAL",
                "Export path contains path traversal sequences.",
                "Use an absolute path without '..' components.");
        }

        var directory = Path.GetDirectoryName(value);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch
            {
                return ValidationResult.Invalid(
                    "Export.Path",
                    "ERR_EXPORT_DIR_INACCESSIBLE",
                    $"Cannot create or access directory: {directory}",
                    "Choose a different export location.");
            }
        }

        foreach (var blocked in s_blockedPaths)
        {
            if (value.StartsWith(blocked, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Invalid(
                    "Export.Path",
                    "ERR_EXPORT_BLOCKED_PATH",
                    $"Export to '{blocked}' is not allowed for security reasons.",
                    "Choose a different export location.");
            }
        }

        var extension = Path.GetExtension(value);
        if (string.IsNullOrEmpty(extension))
        {
            return ValidationResult.Invalid(
                "Export.Path",
                "ERR_EXPORT_NO_EXTENSION",
                "Export path must have a file extension.",
                "Add a file extension (e.g., .json, .csv, .html).");
        }

        return ValidationResult.Valid("Export.Path");
    }
}
