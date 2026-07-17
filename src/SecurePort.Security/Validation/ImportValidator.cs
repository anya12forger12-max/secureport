using System.Text.RegularExpressions;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates import files.
/// </summary>
public sealed class ImportValidator : IImportValidator
{
    private static readonly HashSet<string> s_validExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".json", ".csv", ".xml", ".zip", ".txt"
    };

    private static readonly long s_maxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Invalid(
                "Import.Path",
                "ERR_IMPORT_PATH_EMPTY",
                "Import file path cannot be empty.",
                "Provide a valid file path.");
        }

        if (!File.Exists(value))
        {
            return ValidationResult.Invalid(
                "Import.Path",
                "ERR_IMPORT_FILE_NOT_FOUND",
                $"File not found: {value}",
                "Verify the file exists and the path is correct.");
        }

        return ValidateImportFile(value, Path.GetExtension(value));
    }

    public ValidationResult ValidateImportFile(string filePath, string expectedFormat)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ValidationResult.Invalid(
                "Import.File",
                "ERR_IMPORT_PATH_EMPTY",
                "Import file path cannot be empty.",
                "Provide a valid file path.");
        }

        if (!File.Exists(filePath))
        {
            return ValidationResult.Invalid(
                "Import.File",
                "ERR_IMPORT_NOT_FOUND",
                $"File not found: {filePath}",
                "Verify the file exists.");
        }

        var extension = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(extension) || !s_validExtensions.Contains(extension))
        {
            return ValidationResult.Invalid(
                "Import.Format",
                "ERR_IMPORT_UNSUPPORTED_FORMAT",
                $"File extension '{extension}' is not supported for import.",
                $"Supported formats: {string.Join(", ", s_validExtensions)}");
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > s_maxFileSizeBytes)
            {
                return ValidationResult.Invalid(
                    "Import.Size",
                    "ERR_IMPORT_TOO_LARGE",
                    $"File size ({fileInfo.Length} bytes) exceeds the maximum import size ({s_maxFileSizeBytes} bytes).",
                    "Use a smaller file or split the data.");
            }

            if (fileInfo.Length == 0)
            {
                return ValidationResult.Invalid(
                    "Import.Size",
                    "ERR_IMPORT_EMPTY",
                    "Import file is empty.",
                    "Provide a file with data to import.");
            }
        }
        catch (Exception ex)
        {
            return ValidationResult.Invalid(
                "Import.File",
                "ERR_IMPORT_ACCESS",
                $"Cannot access file: {ex.Message}",
                "Check file permissions and try again.");
        }

        return ValidationResult.Valid("Import.File");
    }
}
