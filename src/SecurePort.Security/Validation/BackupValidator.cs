using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates backup metadata.
/// </summary>
public sealed class BackupValidator : IBackupValidator
{
    public ValidationResult Validate(BackupMetadata value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            return ValidationResult.Invalid(
                "Backup.Name",
                "ERR_BACKUP_NAME_EMPTY",
                "Backup name cannot be empty.",
                "Provide a meaningful name for the backup.");
        }

        if (value.Name.Length > 256)
        {
            return ValidationResult.Invalid(
                "Backup.Name",
                "ERR_BACKUP_NAME_TOO_LONG",
                "Backup name exceeds 256 characters.",
                "Use a shorter name.");
        }

        if (string.IsNullOrWhiteSpace(value.FilePath))
        {
            return ValidationResult.Invalid(
                "Backup.FilePath",
                "ERR_BACKUP_PATH_EMPTY",
                "Backup file path cannot be empty.",
                "Provide a valid file path.");
        }

        if (value.SizeBytes < 0)
        {
            return ValidationResult.Invalid(
                "Backup.Size",
                "ERR_BACKUP_SIZE_NEGATIVE",
                "Backup size cannot be negative.",
                "Ensure the backup was properly created.");
        }

        if (value.Status == BackupStatus.Corrupted)
        {
            return ValidationResult.Invalid(
                "Backup.Integrity",
                "ERR_BACKUP_CORRUPTED",
                "Backup is marked as corrupted.",
                "Recreate the backup from a valid source.");
        }

        if (string.IsNullOrWhiteSpace(value.Checksum))
        {
            return ValidationResult.Invalid(
                "Backup.Checksum",
                "ERR_BACKUP_CHECKSUM_EMPTY",
                "Backup checksum cannot be empty.",
                "Recreate the backup to generate a new checksum.");
        }

        if (value.IncludedCategories == null || value.IncludedCategories.Count == 0)
        {
            return ValidationResult.Invalid(
                "Backup.Categories",
                "ERR_BACKUP_NO_CATEGORIES",
                "Backup must include at least one category.",
                "Specify which data categories to include.");
        }

        return ValidationResult.Valid("Backup.Metadata");
    }
}
