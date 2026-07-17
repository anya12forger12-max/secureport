namespace SecurePort.Core.Models;

/// <summary>
/// Contains metadata about a backup file.
/// </summary>
public sealed record BackupMetadata
{
    /// <summary>Unique identifier for this backup.</summary>
    public required Guid BackupId { get; init; }

    /// <summary>Display name for the backup.</summary>
    public required string Name { get; init; }

    /// <summary>When the backup was created.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>The application version that created this backup.</summary>
    public required string ApplicationVersion { get; init; }

    /// <summary>The total size of the backup in bytes.</summary>
    public required long SizeBytes { get; init; }

    /// <summary>SHA-256 checksum of the backup file.</summary>
    public required string Checksum { get; init; }

    /// <summary>The file path where the backup is stored.</summary>
    public required string FilePath { get; init; }

    /// <summary>The type of backup (full, incremental).</summary>
    public required BackupType Type { get; init; }

    /// <summary>The current status of the backup.</summary>
    public required BackupStatus Status { get; init; }

    /// <summary>What data categories are included in the backup.</summary>
    public required IReadOnlyList<string> IncludedCategories { get; init; }

    /// <summary>Optional description or notes about the backup.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Specifies the type of backup.
/// </summary>
public enum BackupType
{
    /// <summary>Complete backup of all data.</summary>
    Full,

    /// <summary>Backup of only changed data since last full backup.</summary>
    Incremental
}

/// <summary>
/// Represents the current state of a backup.
/// </summary>
public enum BackupStatus
{
    /// <summary>The backup is valid and available.</summary>
    Valid,

    /// <summary>The backup is being created.</summary>
    InProgress,

    /// <summary>The backup has been archived.</summary>
    Archived,

    /// <summary>The backup is corrupted or invalid.</summary>
    Corrupted,

    /// <summary>The backup is being restored.</summary>
    Restoring
}
