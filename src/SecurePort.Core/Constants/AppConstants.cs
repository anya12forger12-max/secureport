namespace SecurePort.Core.Constants;

/// <summary>
/// Application-wide constants used throughout SecurePort.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// The display name of the application.
    /// </summary>
    public const string AppName = "SecurePort";

    /// <summary>
    /// The current application version.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The lowest valid port number.
    /// </summary>
    public const int MinPort = 1;

    /// <summary>
    /// The highest valid port number.
    /// </summary>
    public const int MaxPort = 65535;

    /// <summary>
    /// Default connection timeout in milliseconds.
    /// </summary>
    public const int DefaultTimeoutMs = 2000;

    /// <summary>
    /// Default maximum number of concurrent connections per scan.
    /// </summary>
    public const int DefaultMaxConcurrent = 100;

    /// <summary>
    /// Hard limit on the maximum number of concurrent connections allowed.
    /// </summary>
    public const int MaxConcurrentLimit = 1000;

    /// <summary>
    /// Default starting port for a scan range.
    /// </summary>
    public const int DefaultPortRangeStart = 1;

    /// <summary>
    /// Default ending port for a scan range.
    /// </summary>
    public const int DefaultPortRangeEnd = 1024;

    /// <summary>
    /// Common well-known ports used for service identification.
    /// </summary>
    public static readonly IReadOnlyList<int> WellKnownPorts = new[]
    {
        21, 22, 23, 25, 53, 80, 110, 143,
        443, 993, 995, 3306, 3389, 5432, 8080, 8443
    };

    /// <summary>
    /// Maximum number of scan history entries retained in storage.
    /// </summary>
    public const int MaxScanHistoryEntries = 10000;

    /// <summary>
    /// Maximum age of history retention in days (10 years).
    /// </summary>
    public const int MaxRetentionDays = 3650;

    /// <summary>
    /// Default cache capacity for result entries.
    /// </summary>
    public const int DefaultCacheCapacity = 10_000;

    /// <summary>
    /// Memory threshold in bytes for critical health level (500 MB).
    /// </summary>
    public const long CriticalMemoryThresholdBytes = 500L * 1024 * 1024;

    /// <summary>
    /// Memory threshold in bytes for warning health level (200 MB).
    /// </summary>
    public const long WarningMemoryThresholdBytes = 200L * 1024 * 1024;

    /// <summary>
    /// Default timeout for secure data operations in milliseconds.
    /// </summary>
    public const int SecureOperationTimeoutMs = 3000;

    /// <summary>
    /// Maximum file read buffer size in bytes for integrity verification.
    /// </summary>
    public const int IntegrityCheckBufferSize = 81_920;

    /// <summary>
    /// A brief description of the application.
    /// </summary>
    public const string ApplicationDescription = "Advanced Defensive Network Security Scanner";
}
