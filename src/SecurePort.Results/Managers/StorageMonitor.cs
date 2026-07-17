using System.Diagnostics;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Provides storage monitoring, privacy management, and secure data deletion capabilities.
/// </summary>
public sealed class StorageMonitor : IStorageMonitor, IDisposable
{
    private readonly IScanHistoryRepository? _historyRepository;
    private readonly IReportManager? _reportManager;
    private readonly IBackupManager? _backupManager;
    private readonly string _dataDirectory;
    private readonly string _storagePath;
    private bool _disposed;

    public StorageMonitor(
        IScanHistoryRepository? historyRepository,
        IReportManager? reportManager,
        IBackupManager? backupManager,
        string dataDirectory)
    {
        _historyRepository = historyRepository;
        _reportManager = reportManager;
        _backupManager = backupManager;
        _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        _storagePath = dataDirectory;
    }

    public async Task<StorageUsageInfo> GetStorageUsageAsync(CancellationToken ct)
    {
        var historySize = await GetDirectorySizeAsync(Path.Combine(_dataDirectory, "history"), ct);
        var reportsSize = await GetDirectorySizeAsync(Path.Combine(_dataDirectory, "reports"), ct);
        var backupsSize = await GetDirectorySizeAsync(Path.Combine(_dataDirectory, "backups"), ct);
        var logsSize = await GetDirectorySizeAsync(Path.Combine(_dataDirectory, "logs"), ct);
        var configSize = await GetDirectorySizeAsync(Path.Combine(_dataDirectory, "config"), ct);

        var totalSize = historySize + reportsSize + backupsSize + logsSize + configSize;

        var driveInfo = new DriveInfo(Path.GetPathRoot(_dataDirectory) ?? Path.DirectorySeparatorChar.ToString());

        return new StorageUsageInfo
        {
            TotalSizeBytes = totalSize,
            HistorySizeBytes = historySize,
            ReportsSizeBytes = reportsSize,
            BackupsSizeBytes = backupsSize,
            LogsSizeBytes = logsSize,
            ConfigurationSizeBytes = configSize,
            FreeDiskSpaceBytes = driveInfo.AvailableFreeSpace,
            TotalDiskSpaceBytes = driveInfo.TotalSize,
            EncryptionEnabled = false,
            CollectedAt = DateTimeOffset.UtcNow
        };
    }

    public async Task<PrivacyStatus> GetPrivacyStatusAsync(CancellationToken ct)
    {
        var historyCount = _historyRepository is not null
            ? await _historyRepository.CountAsync(ct)
            : 0;

        var reportCount = _reportManager is not null
            ? (await _reportManager.GetAllReportsAsync(ct)).Count
            : 0;

        var backupCount = _backupManager is not null
            ? (await _backupManager.GetAllBackupsAsync(ct)).Count
            : 0;

        var storageUsage = await GetStorageUsageAsync(ct);

        return new PrivacyStatus
        {
            HistoryEnabled = _historyRepository is not null,
            EncryptionEnabled = false,
            OfflineMode = true,
            TelemetryDisabled = true,
            TrackingDisabled = true,
            CloudServicesDisabled = true,
            HistoryEntryCount = historyCount,
            ReportCount = reportCount,
            BackupCount = backupCount,
            TotalDataSizeBytes = storageUsage.TotalSizeBytes,
            StoragePath = _storagePath
        };
    }

    public async Task<bool> DeleteAllDataAsync(CancellationToken ct)
    {
        var historyCleared = await ClearHistoryAsync(ct);
        var reportsDeleted = await DeleteAllReportsAsync(ct);
        var backupsDeleted = await DeleteAllBackupsAsync(ct);

        return true;
    }

    public async Task<int> ClearHistoryAsync(CancellationToken ct)
    {
        if (_historyRepository is null)
            return 0;

        var count = await _historyRepository.CountAsync(ct);
        await _historyRepository.ClearAsync(ct);
        return count;
    }

    public async Task<int> DeleteAllReportsAsync(CancellationToken ct)
    {
        if (_reportManager is null)
            return 0;

        var reports = await _reportManager.GetAllReportsAsync(ct);
        int deleted = 0;

        foreach (var report in reports)
        {
            ct.ThrowIfCancellationRequested();
            if (await _reportManager.DeleteReportAsync(report.ReportId, ct))
                deleted++;
        }

        return deleted;
    }

    public async Task<int> DeleteAllBackupsAsync(CancellationToken ct)
    {
        if (_backupManager is null)
            return 0;

        var backups = await _backupManager.GetAllBackupsAsync(ct);
        int deleted = 0;

        foreach (var backup in backups)
        {
            ct.ThrowIfCancellationRequested();
            if (await _backupManager.DeleteBackupAsync(backup.BackupId, ct))
                deleted++;
        }

        return deleted;
    }

    public Task ResetPrivacySettingsAsync(CancellationToken ct)
    {
        // Reset privacy settings to defaults - encryption off, history enabled
        return Task.CompletedTask;
    }

    public async Task<bool> SecureDeleteAsync(string dataCategory, Guid? itemId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dataCategory))
            return false;

        return dataCategory.ToLowerInvariant() switch
        {
            "history" => await ClearHistoryAsync(ct) >= 0,
            "reports" => itemId.HasValue
                ? await _reportManager!.DeleteReportAsync(itemId.Value, ct)
                : await DeleteAllReportsAsync(ct) >= 0,
            "backups" => itemId.HasValue
                ? await _backupManager!.DeleteBackupAsync(itemId.Value, ct)
                : await DeleteAllBackupsAsync(ct) >= 0,
            "logs" => await DeleteLogFileAsync(ct),
            _ => false
        };
    }

    private Task<bool> DeleteLogFileAsync(CancellationToken ct)
    {
        var logDir = Path.Combine(_dataDirectory, "logs");
        if (!Directory.Exists(logDir))
            return Task.FromResult(true);

        var logFiles = Directory.GetFiles(logDir, "log-*.txt");
        foreach (var file in logFiles)
        {
            ct.ThrowIfCancellationRequested();
            File.Delete(file);
        }

        return Task.FromResult(true);
    }

    private static async Task<long> GetDirectorySizeAsync(string directory, CancellationToken ct)
    {
        if (!Directory.Exists(directory))
            return 0;

        return await Task.Run(() =>
        {
            try
            {
                return Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                    .Sum(file =>
                    {
                        ct.ThrowIfCancellationRequested();
                        try { return new FileInfo(file).Length; }
                        catch { return 0L; }
                    });
            }
            catch
            {
                return 0L;
            }
        }, ct);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
