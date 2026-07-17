using System.Text.Json;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages data retention policies and automatic cleanup of old data.
/// </summary>
public sealed class DataRetentionManager : IDataRetentionManager
{
    private readonly IScanHistoryRepository? _historyRepository;
    private readonly IReportManager? _reportManager;
    private readonly ILogManager? _logManager;
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public DataRetentionManager(
        string configDirectory,
        IScanHistoryRepository? historyRepository = null,
        IReportManager? reportManager = null,
        ILogManager? logManager = null)
    {
        _historyRepository = historyRepository;
        _reportManager = reportManager;
        _logManager = logManager;

        Directory.CreateDirectory(configDirectory);
        _configFilePath = Path.Combine(configDirectory, "data_retention.json");
    }

    public async Task<DataRetentionConfig> GetConfigurationAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath, ct);
                var config = JsonSerializer.Deserialize<DataRetentionConfig>(json, s_jsonOptions);
                if (config is not null)
                    return config;
            }

            return CreateDefault();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveConfigurationAsync(DataRetentionConfig config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(config);

        await _lock.WaitAsync(ct);
        try
        {
            var json = JsonSerializer.Serialize(config, s_jsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<RetentionCleanupResult> ExecuteCleanupAsync(CancellationToken ct)
    {
        var config = await GetConfigurationAsync(ct);
        return await PerformCleanupAsync(config, isPreview: false, ct);
    }

    public async Task<RetentionCleanupResult> PreviewCleanupAsync(CancellationToken ct)
    {
        var config = await GetConfigurationAsync(ct);
        return await PerformCleanupAsync(config, isPreview: true, ct);
    }

    private async Task<RetentionCleanupResult> PerformCleanupAsync(
        DataRetentionConfig config,
        bool isPreview,
        CancellationToken ct)
    {
        int historyRemoved = 0;
        int reportsRemoved = 0;
        int logsRemoved = 0;
        long bytesFreed = 0;

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-config.RetentionDays);

        if (_historyRepository is not null && config.AutoCleanupEnabled)
        {
            if (isPreview)
            {
                var count = await _historyRepository.CountAsync(ct);
                historyRemoved = count; // Preview estimates all would be removed
            }
            else
            {
                historyRemoved = await _historyRepository.PurgeOlderThanAsync(cutoffDate, ct);
            }
        }

        if (_logManager is not null && config.AutoCleanupEnabled)
        {
            if (!isPreview)
            {
                logsRemoved = await _logManager.DeleteOlderThanAsync(cutoffDate, ct);
            }
        }

        return new RetentionCleanupResult
        {
            HistoryEntriesRemoved = historyRemoved,
            ReportsRemoved = reportsRemoved,
            LogsRemoved = logsRemoved,
            BytesFreed = bytesFreed,
            ExecutedAt = DateTimeOffset.UtcNow,
            IsPreview = isPreview
        };
    }

    private static DataRetentionConfig CreateDefault() => new()
    {
        MaxHistoryEntries = 10000,
        AutoCleanupEnabled = true,
        RetentionDays = 90,
        ArchiveThresholdDays = 180,
        MaxStorageBytes = 0
    };
}
