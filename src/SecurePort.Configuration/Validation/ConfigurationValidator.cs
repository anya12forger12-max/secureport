using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Configuration.Validation;

/// <summary>
/// Validates an <see cref="AppConfiguration"/> instance, ensuring all fields
/// are within acceptable ranges and required values are present.
/// </summary>
public sealed class ConfigurationValidator
{
    /// <summary>
    /// The minimum allowed font size in points.
    /// </summary>
    private const double MinFontSize = 8.0;

    /// <summary>
    /// The maximum allowed font size in points.
    /// </summary>
    private const double MaxFontSize = 72.0;

    /// <summary>
    /// The minimum allowed connection timeout in seconds.
    /// </summary>
    private const double MinTimeoutSeconds = 1.0;

    /// <summary>
    /// The maximum allowed connection timeout in seconds.
    /// </summary>
    private const double MaxTimeoutSeconds = 300.0;

    /// <summary>
    /// The minimum allowed maximum concurrent connections.
    /// </summary>
    private const int MinMaxConcurrent = 1;

    /// <summary>
    /// The maximum allowed maximum concurrent connections.
    /// </summary>
    private const int MaxMaxConcurrent = 10000;

    /// <summary>
    /// The minimum allowed port number.
    /// </summary>
    private const int MinPort = 1;

    /// <summary>
    /// The maximum allowed port number.
    /// </summary>
    private const int MaxPort = 65535;

    /// <summary>
    /// The minimum retention period in days.
    /// </summary>
    private const int MinRetentionDays = 1;

    /// <summary>
    /// The maximum retention period in days.
    /// </summary>
    private const int MaxRetentionDays = 3650;

    /// <summary>
    /// Validates the specified <see cref="AppConfiguration"/> and returns
    /// a list of all validation errors found.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A list of human-readable error messages. Empty if valid.</returns>
    public IReadOnlyList<string> Validate(AppConfiguration configuration)
    {
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        var errors = new List<string>();

        ValidateScanSettings(configuration.Scan, errors);
        ValidateUISettings(configuration.UI, errors);
        ValidateStorageSettings(configuration.Storage, errors);
        ValidateLogSettings(configuration.Log, errors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Returns true if the configuration passes all validation rules.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public bool IsValid(AppConfiguration configuration) =>
        Validate(configuration).Count == 0;

    /// <summary>
    /// Validates scan-related settings.
    /// </summary>
    private static void ValidateScanSettings(ScanSettings? scan, List<string> errors)
    {
        if (scan is null)
        {
            errors.Add("Scan settings are required.");
            return;
        }

        var timeoutSeconds = scan.DefaultTimeout.TotalSeconds;

        if (timeoutSeconds < MinTimeoutSeconds || timeoutSeconds > MaxTimeoutSeconds)
            errors.Add($"Scan.DefaultTimeout must be between {MinTimeoutSeconds}s and {MaxTimeoutSeconds}s. Actual: {timeoutSeconds}s.");

        if (scan.DefaultMaxConcurrent < MinMaxConcurrent || scan.DefaultMaxConcurrent > MaxMaxConcurrent)
            errors.Add($"Scan.DefaultMaxConcurrent must be between {MinMaxConcurrent} and {MaxMaxConcurrent}. Actual: {scan.DefaultMaxConcurrent}.");

        if (scan.DefaultPortRangeStart < MinPort || scan.DefaultPortRangeStart > MaxPort)
            errors.Add($"Scan.DefaultPortRangeStart must be between {MinPort} and {MaxPort}. Actual: {scan.DefaultPortRangeStart}.");

        if (scan.DefaultPortRangeEnd < MinPort || scan.DefaultPortRangeEnd > MaxPort)
            errors.Add($"Scan.DefaultPortRangeEnd must be between {MinPort} and {MaxPort}. Actual: {scan.DefaultPortRangeEnd}.");

        if (scan.DefaultPortRangeStart > scan.DefaultPortRangeEnd)
            errors.Add($"Scan.DefaultPortRangeStart ({scan.DefaultPortRangeStart}) must not exceed DefaultPortRangeEnd ({scan.DefaultPortRangeEnd}).");
    }

    /// <summary>
    /// Validates UI-related settings.
    /// </summary>
    private static void ValidateUISettings(UISettings? ui, List<string> errors)
    {
        if (ui is null)
        {
            errors.Add("UI settings are required.");
            return;
        }

        if (!Enum.IsDefined(typeof(ThemeMode), ui.Theme))
            errors.Add($"UI.Theme is not a valid ThemeMode value. Actual: '{ui.Theme}'.");

        if (!Enum.IsDefined(typeof(Language), ui.Language))
            errors.Add($"UI.Language is not a valid Language value. Actual: '{ui.Language}'.");

        if (ui.FontSize < MinFontSize || ui.FontSize > MaxFontSize)
            errors.Add($"UI.FontSize must be between {MinFontSize} and {MaxFontSize}. Actual: {ui.FontSize}.");
    }

    /// <summary>
    /// Validates storage-related settings.
    /// </summary>
    private static void ValidateStorageSettings(StorageSettings? storage, List<string> errors)
    {
        if (storage is null)
        {
            errors.Add("Storage settings are required.");
            return;
        }

        if (storage.HistoryRetentionDays < MinRetentionDays || storage.HistoryRetentionDays > MaxRetentionDays)
            errors.Add($"Storage.HistoryRetentionDays must be between {MinRetentionDays} and {MaxRetentionDays}. Actual: {storage.HistoryRetentionDays}.");
    }

    /// <summary>
    /// Validates log-related settings.
    /// </summary>
    private static void ValidateLogSettings(LogSettings? log, List<string> errors)
    {
        if (log is null)
        {
            errors.Add("Log settings are required.");
            return;
        }

        if (!Enum.IsDefined(typeof(LogLevel), log.Level))
            errors.Add($"Log.Level is not a valid LogLevel value. Actual: '{log.Level}'.");

        if (log.RetentionDays < MinRetentionDays || log.RetentionDays > MaxRetentionDays)
            errors.Add($"Log.RetentionDays must be between {MinRetentionDays} and {MaxRetentionDays}. Actual: {log.RetentionDays}.");
    }
}
