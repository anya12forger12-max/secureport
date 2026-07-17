using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates report metadata.
/// </summary>
public sealed class ReportValidator : IReportValidator
{
    private static readonly HashSet<ExportFormat> s_supportedFormats = new()
    {
        ExportFormat.JSON, ExportFormat.CSV, ExportFormat.TXT,
        ExportFormat.HTML, ExportFormat.Markdown
    };

    public ValidationResult Validate(ReportMetadata value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value.Title))
        {
            return ValidationResult.Invalid(
                "Report.Title",
                "ERR_REPORT_TITLE_EMPTY",
                "Report title cannot be empty.",
                "Provide a meaningful title for the report.");
        }

        if (value.Title.Length > 256)
        {
            return ValidationResult.Invalid(
                "Report.Title",
                "ERR_REPORT_TITLE_TOO_LONG",
                "Report title exceeds 256 characters.",
                "Use a shorter title.");
        }

        if (string.IsNullOrWhiteSpace(value.TargetHost))
        {
            return ValidationResult.Invalid(
                "Report.Target",
                "ERR_REPORT_TARGET_EMPTY",
                "Report target host cannot be empty.",
                "Specify a target host for the report.");
        }

        if (string.IsNullOrWhiteSpace(value.FilePath))
        {
            return ValidationResult.Invalid(
                "Report.FilePath",
                "ERR_REPORT_PATH_EMPTY",
                "Report file path cannot be empty.",
                "Provide a valid file path.");
        }

        if (!s_supportedFormats.Contains(value.Format))
        {
            return ValidationResult.Invalid(
                "Report.Format",
                "ERR_REPORT_FORMAT_UNSUPPORTED",
                $"Export format {value.Format} is not supported.",
                $"Use one of: {string.Join(", ", s_supportedFormats)}");
        }

        if (value.FileSizeBytes < 0)
        {
            return ValidationResult.Invalid(
                "Report.Size",
                "ERR_REPORT_SIZE_NEGATIVE",
                "Report file size cannot be negative.",
                "Ensure the file was properly written.");
        }

        if (string.IsNullOrWhiteSpace(value.Checksum))
        {
            return ValidationResult.Invalid(
                "Report.Checksum",
                "ERR_REPORT_CHECKSUM_EMPTY",
                "Report checksum cannot be empty.",
                "Regenerate the report to compute a new checksum.");
        }

        return ValidationResult.Valid("Report.Metadata");
    }
}
