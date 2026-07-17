using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages backup creation, restoration, and lifecycle.
/// </summary>
public interface IBackupManager
{
    /// <summary>Creates a full backup of all application data.</summary>
    Task<BackupMetadata> CreateBackupAsync(string name, string? description, CancellationToken ct);

    /// <summary>Restores application data from a backup.</summary>
    Task<bool> RestoreBackupAsync(Guid backupId, CancellationToken ct);

    /// <summary>Deletes a backup.</summary>
    Task<bool> DeleteBackupAsync(Guid backupId, CancellationToken ct);

    /// <summary>Renames a backup.</summary>
    Task<bool> RenameBackupAsync(Guid backupId, string newName, CancellationToken ct);

    /// <summary>Exports a backup to a specified file path.</summary>
    Task<string> ExportBackupAsync(Guid backupId, string destinationPath, CancellationToken ct);

    /// <summary>Imports a backup from a file.</summary>
    Task<BackupMetadata?> ImportBackupAsync(string sourcePath, CancellationToken ct);

    /// <summary>Gets all stored backups.</summary>
    Task<IReadOnlyList<BackupMetadata>> GetAllBackupsAsync(CancellationToken ct);

    /// <summary>Gets backup metadata by ID.</summary>
    Task<BackupMetadata?> GetBackupByIdAsync(Guid backupId, CancellationToken ct);

    /// <summary>Validates the integrity of a backup.</summary>
    Task<IntegrityCheckResult> ValidateBackupAsync(Guid backupId, CancellationToken ct);
}
