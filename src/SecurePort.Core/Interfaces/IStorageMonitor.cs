using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides storage monitoring and privacy management capabilities.
/// </summary>
public interface IStorageMonitor
{
    /// <summary>Gets current storage usage information.</summary>
    Task<StorageUsageInfo> GetStorageUsageAsync(CancellationToken ct);

    /// <summary>Gets the privacy status.</summary>
    Task<PrivacyStatus> GetPrivacyStatusAsync(CancellationToken ct);

    /// <summary>Deletes all application data.</summary>
    Task<bool> DeleteAllDataAsync(CancellationToken ct);

    /// <summary>Clears all scan history.</summary>
    Task<int> ClearHistoryAsync(CancellationToken ct);

    /// <summary>Deletes all reports.</summary>
    Task<int> DeleteAllReportsAsync(CancellationToken ct);

    /// <summary>Deletes all backups.</summary>
    Task<int> DeleteAllBackupsAsync(CancellationToken ct);

    /// <summary>Resets privacy settings to defaults.</summary>
    Task ResetPrivacySettingsAsync(CancellationToken ct);

    /// <summary>Performs secure deletion of specified data.</summary>
    Task<bool> SecureDeleteAsync(string dataCategory, Guid? itemId, CancellationToken ct);
}
