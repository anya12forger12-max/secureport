using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// Contains all application settings for scan behavior, UI, storage, and logging.
/// </summary>
public sealed record AppConfiguration
{
    /// <summary>
    /// Gets the scan configuration settings.
    /// </summary>
    public required ScanSettings Scan { get; init; }

    /// <summary>
    /// Gets the user interface configuration settings.
    /// </summary>
    public required UISettings UI { get; init; }

    /// <summary>
    /// Gets the data storage configuration settings.
    /// </summary>
    public required StorageSettings Storage { get; init; }

    /// <summary>
    /// Gets the logging configuration settings.
    /// </summary>
    public required LogSettings Log { get; init; }
}

/// <summary>
/// Configuration values that control scan behavior and defaults.
/// </summary>
public sealed record ScanSettings
{
    /// <summary>
    /// The default connection timeout for new scans.
    /// </summary>
    public required TimeSpan DefaultTimeout { get; init; }

    /// <summary>
    /// The default maximum concurrent connections for new scans.
    /// </summary>
    public required int DefaultMaxConcurrent { get; init; }

    /// <summary>
    /// The default starting port for new scan ranges.
    /// </summary>
    public required int DefaultPortRangeStart { get; init; }

    /// <summary>
    /// The default ending port for new scan ranges.
    /// </summary>
    public required int DefaultPortRangeEnd { get; init; }
}

/// <summary>
/// Configuration values that control the user interface appearance and accessibility.
/// </summary>
public sealed record UISettings
{
    /// <summary>
    /// The active visual theme.
    /// </summary>
    public required ThemeMode Theme { get; init; }

    /// <summary>
    /// The active display language.
    /// </summary>
    public required Language Language { get; init; }

    /// <summary>
    /// The base font size for UI text.
    /// </summary>
    public required double FontSize { get; init; }

    /// <summary>
    /// Whether high-contrast mode is enabled for accessibility.
    /// </summary>
    public required bool HighContrast { get; init; }

    /// <summary>
    /// Whether reduced motion is enabled for accessibility.
    /// </summary>
    public required bool ReducedMotion { get; init; }

    /// <summary>
    /// Whether screen reader optimizations are enabled.
    /// </summary>
    public required bool ScreenReaderMode { get; init; }
}

/// <summary>
/// Configuration values that control data retention and encryption.
/// </summary>
public sealed record StorageSettings
{
    /// <summary>
    /// The number of days to retain scan history before automatic deletion.
    /// </summary>
    public required int HistoryRetentionDays { get; init; }

    /// <summary>
    /// Whether old history entries are automatically deleted after retention period.
    /// </summary>
    public required bool AutoDelete { get; init; }

    /// <summary>
    /// Whether local data encryption is enabled.
    /// </summary>
    public required bool EncryptionEnabled { get; init; }
}

/// <summary>
/// Configuration values that control application logging.
/// </summary>
public sealed record LogSettings
{
    /// <summary>
    /// The minimum log level to capture.
    /// </summary>
    public required LogLevel Level { get; init; }

    /// <summary>
    /// The number of days to retain log files.
    /// </summary>
    public required int RetentionDays { get; init; }
}
