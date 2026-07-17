using System.Text;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages log searching, filtering, export, and retention.
/// Works with file-based log storage for searching and cleanup.
/// </summary>
public sealed class LogManager : ILogManager
{
    private readonly string _logDirectory;
    private readonly ILoggingProvider? _loggingProvider;

    public LogManager(string logDirectory, ILoggingProvider? loggingProvider = null)
    {
        _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
        _loggingProvider = loggingProvider;
    }

    public async Task<IReadOnlyList<LogEntryInfo>> SearchLogsAsync(
        string query,
        LogLevel? minLevel,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken ct)
    {
        var allEntries = await GetLogsInternalAsync(startDate, endDate, ct);

        if (!string.IsNullOrWhiteSpace(query))
        {
            allEntries = allEntries.Where(e =>
                e.Message.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Source.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (minLevel.HasValue)
        {
            allEntries = allEntries.Where(e => e.Level >= minLevel.Value).ToList();
        }

        return allEntries;
    }

    public Task<IReadOnlyList<LogEntryInfo>> GetLogsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken ct)
    {
        return GetLogsInternalAsync(startDate, endDate, ct);
    }

    public async Task<string> ExportLogsAsync(
        string filePath,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken ct)
    {
        var entries = await GetLogsInternalAsync(startDate, endDate, ct);

        var sb = new StringBuilder();
        sb.AppendLine("SecurePort Log Export");
        sb.AppendLine($"Exported: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"Entries: {entries.Count}");
        sb.AppendLine(new string('=', 80));
        sb.AppendLine();

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            sb.AppendLine($"[{entry.Timestamp:O}] [{entry.Level}] [{entry.Source}] {entry.Message}");
            if (!string.IsNullOrEmpty(entry.ExceptionDetails))
                sb.AppendLine($"  Exception: {entry.ExceptionDetails}");
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, ct);
        return filePath;
    }

    public Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken ct)
    {
        if (!Directory.Exists(_logDirectory))
            return Task.FromResult(0);

        int deleted = 0;
        var logFiles = Directory.GetFiles(_logDirectory, "log-*.txt");

        foreach (var file in logFiles)
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(fileName.Replace("log-", ""), "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var fileDate))
            {
                if (fileDate < cutoffDate.UtcDateTime.Date)
                {
                    File.Delete(file);
                    deleted++;
                }
            }
        }

        return Task.FromResult(deleted);
    }

    public Task<int> DeleteAllLogsAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_logDirectory))
            return Task.FromResult(0);

        var logFiles = Directory.GetFiles(_logDirectory, "log-*.txt");
        int deleted = 0;

        foreach (var file in logFiles)
        {
            ct.ThrowIfCancellationRequested();
            File.Delete(file);
            deleted++;
        }

        return Task.FromResult(deleted);
    }

    public Task<long> GetLogSizeAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_logDirectory))
            return Task.FromResult(0L);

        return Task.Run(() =>
        {
            try
            {
                return Directory.GetFiles(_logDirectory, "log-*.txt")
                    .Sum(f =>
                    {
                        ct.ThrowIfCancellationRequested();
                        try { return new FileInfo(f).Length; }
                        catch { return 0L; }
                    });
            }
            catch
            {
                return 0L;
            }
        }, ct);
    }

    private async Task<IReadOnlyList<LogEntryInfo>> GetLogsInternalAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken ct)
    {
        if (!Directory.Exists(_logDirectory))
            return Array.Empty<LogEntryInfo>();

        var entries = new List<LogEntryInfo>();
        var logFiles = Directory.GetFiles(_logDirectory, "log-*.txt")
            .OrderBy(f => f);

        foreach (var file in logFiles)
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileNameWithoutExtension(file);
            if (DateTime.TryParseExact(fileName.Replace("log-", ""), "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var fileDate))
            {
                if (startDate.HasValue && fileDate < startDate.Value.UtcDateTime.Date)
                    continue;
                if (endDate.HasValue && fileDate > endDate.Value.UtcDateTime.Date)
                    continue;
            }

            try
            {
                var lines = await File.ReadAllLinesAsync(file, ct);
                foreach (var line in lines)
                {
                    ct.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var entry = ParseLogLine(line);
                    if (entry is not null)
                    {
                        if (startDate.HasValue && entry.Timestamp < startDate.Value)
                            continue;
                        if (endDate.HasValue && entry.Timestamp > endDate.Value)
                            continue;

                        entries.Add(entry);
                    }
                }
            }
            catch
            {
                // Skip corrupted log files
            }
        }

        return entries.OrderByDescending(e => e.Timestamp).ToList();
    }

    private static LogEntryInfo? ParseLogLine(string line)
    {
        // Format: [Timestamp] [Level] [Source] Message
        if (!line.StartsWith('['))
            return null;

        var parts = line.Split(new[] { ']', '[' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
        if (parts.Length < 4)
            return null;

        if (!DateTimeOffset.TryParse(parts[0].Trim(), out var timestamp))
            return null;

        if (!Enum.TryParse<LogLevel>(parts[1].Trim(), true, out var level))
            return null;

        return new LogEntryInfo
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp,
            Level = level,
            Source = parts[2].Trim(),
            Message = string.Join(" ", parts.Skip(3)).Trim()
        };
    }
}
