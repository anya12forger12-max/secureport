using System.Text.Json;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Recovery;

/// <summary>
/// Provides failure recovery capabilities for application subsystems.
/// </summary>
public sealed class FailureRecovery : IFailureRecovery
{
    private readonly string _dataDirectory;
    private readonly ISecureFileOperations? _fileOperations;
    private readonly ILogManager? _logManager;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public FailureRecovery(
        string dataDirectory,
        ISecureFileOperations? fileOperations = null,
        ILogManager? logManager = null)
    {
        _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        _fileOperations = fileOperations;
        _logManager = logManager;
    }

    public async Task<RecoveryResult> RecoverConfigurationAsync(CancellationToken ct = default)
    {
        var configPath = Path.Combine(_dataDirectory, "config", "appsettings.json");
        var backupPath = configPath + ".backup";
        var corruptedPath = configPath + ".corrupted";

        try
        {
            if (!File.Exists(configPath))
            {
                return new RecoveryResult
                {
                    Success = true,
                    Component = "Configuration",
                    RecoveryAction = "DefaultConfiguration",
                    Details = "Configuration file missing. Default configuration will be used.",
                    RecoverySteps = new[] { "Application will use built-in defaults.", "Save settings to persist configuration." }
                };
            }

            var json = await File.ReadAllTextAsync(configPath, ct);
            if (string.IsNullOrWhiteSpace(json))
            {
                return await RestoreFromBackupAsync(configPath, backupPath, "Configuration", ct);
            }

            try
            {
                JsonSerializer.Deserialize<AppConfiguration>(json, s_jsonOptions);
                return new RecoveryResult
                {
                    Success = true,
                    Component = "Configuration",
                    RecoveryAction = "NoActionNeeded",
                    Details = "Configuration file is valid."
                };
            }
            catch (JsonException)
            {
                if (File.Exists(configPath))
                    File.Move(configPath, corruptedPath, overwrite: true);

                return await RestoreFromBackupAsync(configPath, backupPath, "Configuration", ct);
            }
        }
        catch (Exception ex)
        {
            return new RecoveryResult
            {
                Success = false,
                Component = "Configuration",
                RecoveryAction = "RecoveryFailed",
                Details = $"Recovery attempt failed: {ex.Message}",
                RecoverySteps = new[] { "Manually restore configuration from backup.", "Check file permissions." }
            };
        }
    }

    public async Task<RecoveryResult> RecoverHistoryAsync(CancellationToken ct = default)
    {
        var historyDir = Path.Combine(_dataDirectory, "history");
        try
        {
            if (!Directory.Exists(historyDir))
            {
                Directory.CreateDirectory(historyDir);
                return new RecoveryResult
                {
                    Success = true,
                    Component = "History",
                    RecoveryAction = "DirectoryCreated",
                    Details = "History directory was missing and has been recreated."
                };
            }

            var files = Directory.GetFiles(historyDir, "*.json");
            var corruptCount = 0;
            var validCount = 0;

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var json = await File.ReadAllTextAsync(file, ct);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        JsonSerializer.Deserialize<List<JsonElement>>(json);
                        validCount++;
                    }
                }
                catch
                {
                    var corruptedFile = file + ".corrupted";
                    File.Move(file, corruptedFile, overwrite: true);
                    corruptCount++;
                }
            }

            return new RecoveryResult
            {
                Success = true,
                Component = "History",
                RecoveryAction = "IntegrityCheck",
                Details = $"Checked {files.Length} files: {validCount} valid, {corruptCount} corrupted (moved to .corrupted).",
                DataLoss = corruptCount > 0,
                RecoverySteps = corruptCount > 0
                    ? new[] { $"Review {corruptCount} corrupted files with .corrupted extension.", "Manually extract recoverable data if needed." }
                    : null
            };
        }
        catch (Exception ex)
        {
            return new RecoveryResult
            {
                Success = false,
                Component = "History",
                RecoveryAction = "RecoveryFailed",
                Details = $"History recovery failed: {ex.Message}"
            };
        }
    }

    public Task<RecoveryResult> RecoverInterruptedReportAsync(CancellationToken ct = default)
    {
        var reportsDir = Path.Combine(_dataDirectory, "reports");
        try
        {
            if (!Directory.Exists(reportsDir))
                return Task.FromResult(new RecoveryResult
                {
                    Success = true,
                    Component = "Reports",
                    RecoveryAction = "NoActionNeeded",
                    Details = "Reports directory does not exist. No interrupted reports to recover."
                });

            var tempFiles = Directory.GetFiles(reportsDir, "*.tmp.*");
            var removedCount = 0;

            foreach (var file in tempFiles)
            {
                try { File.Delete(file); removedCount++; }
                catch { /* best effort */ }
            }

            return Task.FromResult(new RecoveryResult
            {
                Success = true,
                Component = "Reports",
                RecoveryAction = "CleanupTempFiles",
                Details = $"Cleaned up {removedCount} temporary files from interrupted report generation.",
                RecoverySteps = removedCount > 0
                    ? new[] { "Interrupted report generation has been cleaned up.", "Regenerate any incomplete reports." }
                    : null
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new RecoveryResult
            {
                Success = false,
                Component = "Reports",
                RecoveryAction = "RecoveryFailed",
                Details = $"Report recovery failed: {ex.Message}"
            });
        }
    }

    public async Task<RecoverySummary> RunFullRecoveryAsync(CancellationToken ct = default)
    {
        var results = new List<RecoveryResult>
        {
            await RecoverConfigurationAsync(ct),
            await RecoverHistoryAsync(ct),
            await RecoverInterruptedReportAsync(ct)
        };

        return new RecoverySummary
        {
            Results = results
        };
    }

    public Task<string> CreateRecoveryBackupAsync(CancellationToken ct = default)
    {
        var backupDir = Path.Combine(_dataDirectory, "recovery_backups");
        Directory.CreateDirectory(backupDir);

        var backupName = $"recovery_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}";
        var backupPath = Path.Combine(backupDir, backupName);
        Directory.CreateDirectory(backupPath);

        var sourceDir = Path.Combine(_dataDirectory, "config");
        if (Directory.Exists(sourceDir))
        {
            var destDir = Path.Combine(backupPath, "config");
            CopyDirectory(sourceDir, destDir);
        }

        return Task.FromResult(backupPath);
    }

    private async Task<RecoveryResult> RestoreFromBackupAsync(string configPath, string backupPath, string component, CancellationToken ct)
    {
        if (File.Exists(backupPath))
        {
            try
            {
                var backupJson = await File.ReadAllTextAsync(backupPath, ct);
                JsonSerializer.Deserialize<AppConfiguration>(backupJson, s_jsonOptions);
                File.Copy(backupPath, configPath, overwrite: true);
                return new RecoveryResult
                {
                    Success = true,
                    Component = component,
                    RecoveryAction = "RestoreFromBackup",
                    Details = "Configuration restored from backup.",
                    RecoverySteps = new[] { "Verify restored settings.", "Save to create a fresh backup." }
                };
            }
            catch
            {
                // Backup also corrupted
            }
        }

        return new RecoveryResult
        {
            Success = true,
            Component = component,
            RecoveryAction = "DefaultConfiguration",
            Details = "Corrupted configuration removed. Application will use defaults.",
            RecoverySteps = new[] { "Save settings to persist a new configuration." }
        };
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }
        foreach (var dir in Directory.GetDirectories(source))
        {
            CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
        }
    }
}
