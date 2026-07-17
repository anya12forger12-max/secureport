using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages log searching, filtering, export, and retention.
/// </summary>
public interface ILogManager
{
    /// <summary>Searches logs by text query.</summary>
    Task<IReadOnlyList<LogEntryInfo>> SearchLogsAsync(string query, Enums.LogLevel? minLevel, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ct);

    /// <summary>Gets all log entries within a date range.</summary>
    Task<IReadOnlyList<LogEntryInfo>> GetLogsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ct);

    /// <summary>Exports logs to a file.</summary>
    Task<string> ExportLogsAsync(string filePath, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken ct);

    /// <summary>Deletes logs older than the specified date.</summary>
    Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffDate, CancellationToken ct);

    /// <summary>Deletes all logs.</summary>
    Task<int> DeleteAllLogsAsync(CancellationToken ct);

    /// <summary>Gets the total size of log files in bytes.</summary>
    Task<long> GetLogSizeAsync(CancellationToken ct);
}
