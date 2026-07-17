using System.Diagnostics;
using System.Runtime.InteropServices;
using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Health;

/// <summary>
/// Monitors the health of application subsystems.
/// </summary>
public sealed class HealthMonitor : IHealthMonitor
{
    private readonly IConfigurationService? _configService;
    private readonly IResultsRepository? _resultsRepository;
    private readonly ILogManager? _logManager;

    public HealthMonitor(
        IConfigurationService? configService = null,
        IResultsRepository? resultsRepository = null,
        ILogManager? logManager = null)
    {
        _configService = configService;
        _resultsRepository = resultsRepository;
        _logManager = logManager;
    }

    public async Task<HealthReport> CheckHealthAsync(CancellationToken ct = default)
    {
        var subsystems = new List<SubsystemHealth>();

        subsystems.Add(await CheckApplicationStateAsync(ct));
        subsystems.Add(CheckMemoryUsage());
        subsystems.Add(CheckThreadPoolHealth());
        subsystems.Add(await CheckStorageHealthAsync(ct));
        subsystems.Add(await CheckConfigurationHealthAsync(ct));
        subsystems.Add(await CheckHistoryHealthAsync(ct));
        subsystems.Add(await CheckLogHealthAsync(ct));

        var overallStatus = DetermineOverallStatus(subsystems);

        return new HealthReport
        {
            OverallStatus = overallStatus,
            Subsystems = subsystems,
            Summary = GenerateSummary(subsystems, overallStatus)
        };
    }

    public Task<SubsystemHealth> CheckSubsystemAsync(string subsystemName, CancellationToken ct = default)
    {
        return subsystemName.ToLowerInvariant() switch
        {
            "memory" => Task.FromResult(CheckMemoryUsage()),
            "threadpool" => Task.FromResult(CheckThreadPoolHealth()),
            "application" => CheckApplicationStateAsync(ct),
            "storage" => CheckStorageHealthAsync(ct),
            "configuration" => CheckConfigurationHealthAsync(ct),
            "history" => CheckHistoryHealthAsync(ct),
            "logs" => CheckLogHealthAsync(ct),
            _ => Task.FromResult(new SubsystemHealth
            {
                SubsystemName = subsystemName,
                Status = HealthLevel.Unknown,
                Message = $"Unknown subsystem: {subsystemName}"
            })
        };
    }

    public IReadOnlyList<string> GetRegisteredSubsystems() => new[]
    {
        "Application", "Memory", "ThreadPool", "Storage",
        "Configuration", "History", "Logs"
    };

    private static async Task<SubsystemHealth> CheckApplicationStateAsync(CancellationToken ct)
    {
        await Task.CompletedTask;
        var process = Process.GetCurrentProcess();
        return new SubsystemHealth
        {
            SubsystemName = "Application",
            Status = HealthLevel.Healthy,
            Message = $"Process running (PID: {process.Id})",
            Metadata = new Dictionary<string, string>
            {
                ["PID"] = process.Id.ToString(),
                ["StartTime"] = process.StartTime.ToUniversalTime().ToString("O"),
                ["Uptime"] = (DateTimeOffset.UtcNow - process.StartTime.ToUniversalTime()).ToString()
            }
        };
    }

    private static SubsystemHealth CheckMemoryUsage()
    {
        var gcMemory = GC.GetTotalMemory(forceFullCollection: false);
        var process = Process.GetCurrentProcess();
        var workingSet = process.WorkingSet64;

        var level = HealthLevel.Healthy;
        string message;
        if (gcMemory > AppConstants.CriticalMemoryThresholdBytes)
        {
            level = HealthLevel.Critical;
            message = $"High memory usage: {gcMemory / (1024 * 1024)}MB";
        }
        else if (gcMemory > AppConstants.WarningMemoryThresholdBytes)
        {
            level = HealthLevel.Warning;
            message = $"Elevated memory usage: {gcMemory / (1024 * 1024)}MB";
        }
        else
        {
            message = $"Normal memory usage: {gcMemory / (1024 * 1024)}MB";
        }

        return new SubsystemHealth
        {
            SubsystemName = "Memory",
            Status = level,
            Message = message,
            Metadata = new Dictionary<string, string>
            {
                ["GcTotalBytes"] = gcMemory.ToString(),
                ["WorkingSetBytes"] = workingSet.ToString(),
                ["Gen0Collections"] = GC.CollectionCount(0).ToString(),
                ["Gen1Collections"] = GC.CollectionCount(1).ToString(),
                ["Gen2Collections"] = GC.CollectionCount(2).ToString()
            }
        };
    }

    private static SubsystemHealth CheckThreadPoolHealth()
    {
        ThreadPool.GetAvailableThreads(out var workerAvail, out var portAvail);
        ThreadPool.GetMaxThreads(out var workerMax, out var portMax);
        ThreadPool.GetMinThreads(out var workerMin, out var portMin);

        var workerInUse = workerMax - workerAvail;
        var utilizationPercent = workerMax > 0 ? (workerInUse * 100.0 / workerMax) : 0;

        var level = HealthLevel.Healthy;
        string message;
        if (utilizationPercent > 90)
        {
            level = HealthLevel.Critical;
            message = $"Thread pool near exhaustion: {utilizationPercent:F0}% utilized";
        }
        else if (utilizationPercent > 70)
        {
            level = HealthLevel.Warning;
            message = $"Thread pool elevated usage: {utilizationPercent:F0}% utilized";
        }
        else
        {
            message = $"Thread pool healthy: {utilizationPercent:F0}% utilized";
        }

        return new SubsystemHealth
        {
            SubsystemName = "ThreadPool",
            Status = level,
            Message = message,
            Metadata = new Dictionary<string, string>
            {
                ["WorkerMin"] = workerMin.ToString(),
                ["WorkerMax"] = workerMax.ToString(),
                ["WorkerAvailable"] = workerAvail.ToString(),
                ["PortMin"] = portMin.ToString(),
                ["PortMax"] = portMax.ToString(),
                ["PortAvailable"] = portAvail.ToString()
            }
        };
    }

    private async Task<SubsystemHealth> CheckStorageHealthAsync(CancellationToken ct)
    {
        try
        {
            var config = _configService != null ? await _configService.GetConfigurationAsync(ct) : null;
            var retentionDays = config?.Storage?.HistoryRetentionDays ?? 0;

            var level = HealthLevel.Healthy;
            string message;

            if (retentionDays <= 0)
            {
                level = HealthLevel.Warning;
                message = "History retention not configured (entries may not be auto-cleaned).";
            }
            else
            {
                message = $"History retention configured: {retentionDays} days.";
            }

            return new SubsystemHealth
            {
                SubsystemName = "Storage",
                Status = level,
                Message = message,
                Metadata = new Dictionary<string, string>
                {
                    ["HistoryRetentionDays"] = retentionDays.ToString(),
                    ["EncryptionEnabled"] = (config?.Storage?.EncryptionEnabled ?? false).ToString(),
                    ["AutoDelete"] = (config?.Storage?.AutoDelete ?? false).ToString()
                }
            };
        }
        catch (Exception ex)
        {
            return new SubsystemHealth
            {
                SubsystemName = "Storage",
                Status = HealthLevel.Critical,
                Message = $"Storage check failed: {ex.Message}",
                RecoverySuggestion = "Verify storage configuration and permissions."
            };
        }
    }

    private async Task<SubsystemHealth> CheckConfigurationHealthAsync(CancellationToken ct)
    {
        if (_configService == null)
        {
            return new SubsystemHealth
            {
                SubsystemName = "Configuration",
                Status = HealthLevel.Warning,
                Message = "Configuration service not available."
            };
        }

        try
        {
            var config = await _configService.GetConfigurationAsync(ct);
            if (config == null)
            {
                return new SubsystemHealth
                {
                    SubsystemName = "Configuration",
                    Status = HealthLevel.Warning,
                    Message = "Configuration is null (using defaults).",
                    RecoverySuggestion = "Save configuration to persist settings."
                };
            }

            return new SubsystemHealth
            {
                SubsystemName = "Configuration",
                Status = HealthLevel.Healthy,
                Message = "Configuration loaded successfully."
            };
        }
        catch (Exception ex)
        {
            return new SubsystemHealth
            {
                SubsystemName = "Configuration",
                Status = HealthLevel.Critical,
                Message = $"Configuration load failed: {ex.Message}",
                RecoverySuggestion = "Restore configuration from backup."
            };
        }
    }

    private async Task<SubsystemHealth> CheckHistoryHealthAsync(CancellationToken ct)
    {
        if (_resultsRepository == null)
        {
            return new SubsystemHealth
            {
                SubsystemName = "History",
                Status = HealthLevel.Unknown,
                Message = "Results repository not available."
            };
        }

        try
        {
            var count = await _resultsRepository.CountAsync(ct);
            return new SubsystemHealth
            {
                SubsystemName = "History",
                Status = HealthLevel.Healthy,
                Message = $"History contains {count} entries.",
                Metadata = new Dictionary<string, string>
                {
                    ["EntryCount"] = count.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            return new SubsystemHealth
            {
                SubsystemName = "History",
                Status = HealthLevel.Critical,
                Message = $"History check failed: {ex.Message}",
                RecoverySuggestion = "Rebuild history index from storage."
            };
        }
    }

    private async Task<SubsystemHealth> CheckLogHealthAsync(CancellationToken ct)
    {
        if (_logManager == null)
        {
            return new SubsystemHealth
            {
                SubsystemName = "Logs",
                Status = HealthLevel.Unknown,
                Message = "Log manager not available."
            };
        }

        try
        {
            var size = await _logManager.GetLogSizeAsync(ct);
            return new SubsystemHealth
            {
                SubsystemName = "Logs",
                Status = HealthLevel.Healthy,
                Message = $"Log files total {size / 1024}KB.",
                Metadata = new Dictionary<string, string>
                {
                    ["LogSizeBytes"] = size.ToString()
                }
            };
        }
        catch (Exception ex)
        {
            return new SubsystemHealth
            {
                SubsystemName = "Logs",
                Status = HealthLevel.Warning,
                Message = $"Log health check failed: {ex.Message}"
            };
        }
    }

    private static HealthLevel DetermineOverallStatus(IReadOnlyList<SubsystemHealth> subsystems)
    {
        if (subsystems.Any(s => s.Status == HealthLevel.Critical))
            return HealthLevel.Critical;
        if (subsystems.Any(s => s.Status == HealthLevel.Warning))
            return HealthLevel.Warning;
        return HealthLevel.Healthy;
    }

    private static string GenerateSummary(IReadOnlyList<SubsystemHealth> subsystems, HealthLevel overall)
    {
        var healthy = subsystems.Count(s => s.Status == HealthLevel.Healthy);
        var warning = subsystems.Count(s => s.Status == HealthLevel.Warning);
        var critical = subsystems.Count(s => s.Status == HealthLevel.Critical);
        return $"Overall: {overall} — {healthy} healthy, {warning} warnings, {critical} critical out of {subsystems.Count} subsystems.";
    }
}
