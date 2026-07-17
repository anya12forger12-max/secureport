namespace SecurePort.Core.Models;

/// <summary>
/// Contains storage usage information for monitoring.
/// </summary>
public sealed record StorageUsageInfo
{
    /// <summary>Total size of all data in bytes.</summary>
    public required long TotalSizeBytes { get; init; }

    /// <summary>Size of history data in bytes.</summary>
    public required long HistorySizeBytes { get; init; }

    /// <summary>Size of report data in bytes.</summary>
    public required long ReportsSizeBytes { get; init; }

    /// <summary>Size of backup data in bytes.</summary>
    public required long BackupsSizeBytes { get; init; }

    /// <summary>Size of log data in bytes.</summary>
    public required long LogsSizeBytes { get; init; }

    /// <summary>Size of configuration data in bytes.</summary>
    public required long ConfigurationSizeBytes { get; init; }

    /// <summary>Free disk space in bytes.</summary>
    public required long FreeDiskSpaceBytes { get; init; }

    /// <summary>Total disk space in bytes.</summary>
    public required long TotalDiskSpaceBytes { get; init; }

    /// <summary>Whether encryption is enabled for local storage.</summary>
    public required bool EncryptionEnabled { get; init; }

    /// <summary>When this information was collected.</summary>
    public required DateTimeOffset CollectedAt { get; init; }
}

/// <summary>
/// Represents the privacy status of the application.
/// </summary>
public sealed record PrivacyStatus
{
    /// <summary>Whether history recording is enabled.</summary>
    public required bool HistoryEnabled { get; init; }

    /// <summary>Whether local data encryption is enabled.</summary>
    public required bool EncryptionEnabled { get; init; }

    /// <summary>Whether the application operates in offline mode.</summary>
    public required bool OfflineMode { get; init; }

    /// <summary>Whether telemetry is disabled.</summary>
    public required bool TelemetryDisabled { get; init; }

    /// <summary>Whether tracking is disabled.</summary>
    public required bool TrackingDisabled { get; init; }

    /// <summary>Whether cloud services are disabled.</summary>
    public required bool CloudServicesDisabled { get; init; }

    /// <summary>Number of stored history entries.</summary>
    public required int HistoryEntryCount { get; init; }

    /// <summary>Number of stored reports.</summary>
    public required int ReportCount { get; init; }

    /// <summary>Number of stored backups.</summary>
    public required int BackupCount { get; init; }

    /// <summary>Total data stored in bytes.</summary>
    public required long TotalDataSizeBytes { get; init; }

    /// <summary>The local storage path.</summary>
    public required string StoragePath { get; init; }
}

/// <summary>
/// Represents a log entry with full metadata.
/// </summary>
public sealed record LogEntryInfo
{
    /// <summary>Unique identifier for this log entry.</summary>
    public required Guid Id { get; init; }

    /// <summary>The UTC timestamp.</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>The severity level.</summary>
    public required Enums.LogLevel Level { get; init; }

    /// <summary>The source component.</summary>
    public required string Source { get; init; }

    /// <summary>The log message.</summary>
    public required string Message { get; init; }

    /// <summary>Optional exception details.</summary>
    public string? ExceptionDetails { get; init; }
}

/// <summary>
/// Represents data retention configuration.
/// </summary>
public sealed record DataRetentionConfig
{
    /// <summary>Maximum number of history entries to retain.</summary>
    public required int MaxHistoryEntries { get; init; }

    /// <summary>Whether automatic cleanup is enabled.</summary>
    public required bool AutoCleanupEnabled { get; init; }

    /// <summary>Number of days to retain data.</summary>
    public required int RetentionDays { get; init; }

    /// <summary>Number of days after which data is archived instead of deleted.</summary>
    public required int ArchiveThresholdDays { get; init; }

    /// <summary>Maximum total storage size in bytes (0 = unlimited).</summary>
    public required long MaxStorageBytes { get; init; }
}
