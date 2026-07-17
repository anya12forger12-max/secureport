using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages data retention policies and automatic cleanup.
/// </summary>
public interface IDataRetentionManager
{
    /// <summary>Gets the current data retention configuration.</summary>
    Task<DataRetentionConfig> GetConfigurationAsync(CancellationToken ct);

    /// <summary>Saves the data retention configuration.</summary>
    Task SaveConfigurationAsync(DataRetentionConfig config, CancellationToken ct);

    /// <summary>Executes automatic cleanup based on current retention policies.</summary>
    Task<RetentionCleanupResult> ExecuteCleanupAsync(CancellationToken ct);

    /// <summary>Gets the number of entries that would be removed by cleanup.</summary>
    Task<RetentionCleanupResult> PreviewCleanupAsync(CancellationToken ct);
}

/// <summary>
/// Contains the results of a data retention cleanup operation.
/// </summary>
public sealed record RetentionCleanupResult
{
    /// <summary>Number of history entries that were or would be removed.</summary>
    public required int HistoryEntriesRemoved { get; init; }

    /// <summary>Number of reports that were or would be removed.</summary>
    public required int ReportsRemoved { get; init; }

    /// <summary>Number of logs that were or would be removed.</summary>
    public required int LogsRemoved { get; init; }

    /// <summary>Total bytes freed.</summary>
    public required long BytesFreed { get; init; }

    /// <summary>When the cleanup was or would be performed.</summary>
    public required DateTimeOffset ExecutedAt { get; init; }

    /// <summary>Whether this was a preview (dry run) or actual cleanup.</summary>
    public required bool IsPreview { get; init; }
}
