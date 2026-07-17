using System.Text.Json;
using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Security.Validation;

/// <summary>
/// Validates application configuration.
/// </summary>
public sealed class ConfigurationValidator : IConfigurationValidator
{
    public ValidationResult ValidateConfiguration(AppConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.Scan == null)
            return ValidationResult.Invalid("Configuration.Scan", "ERR_CONFIG_SCAN_MISSING",
                "Scan configuration section is missing.", "Add a 'Scan' section to the configuration.");

        if (config.Storage == null)
            return ValidationResult.Invalid("Configuration.Storage", "ERR_CONFIG_STORAGE_MISSING",
                "Storage configuration section is missing.", "Add a 'Storage' section to the configuration.");

        var scanResult = ValidateScanSettings(config.Scan);
        if (!scanResult.IsValid) return scanResult;

        var storageResult = ValidateStorageSettings(config.Storage);
        if (!storageResult.IsValid) return storageResult;

        if (config.UI != null)
        {
            var uiResult = ValidateUISettings(config.UI);
            if (!uiResult.IsValid) return uiResult;
        }

        return ValidationResult.Valid("Configuration");
    }

    public ValidationResult ValidateConfigurationFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_PATH_EMPTY",
                "Configuration file path cannot be empty.", "Provide a valid path.");
        }

        if (!File.Exists(filePath))
        {
            return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_FILE_NOT_FOUND",
                $"Configuration file not found: {filePath}",
                "Create a default configuration file.");
        }

        try
        {
            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_FILE_EMPTY",
                    "Configuration file is empty.", "Restore from backup or recreate.");
            }

            var config = JsonSerializer.Deserialize<AppConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_DESERIALIZE_FAILED",
                    "Failed to deserialize configuration file.", "Check JSON format and restore from backup.");
            }

            return ValidateConfiguration(config);
        }
        catch (JsonException ex)
        {
            return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_JSON_INVALID",
                $"Invalid JSON in configuration file: {ex.Message}",
                "Fix JSON syntax errors or restore from backup.");
        }
        catch (Exception ex)
        {
            return ValidationResult.Invalid("Configuration.File", "ERR_CONFIG_READ_FAILED",
                $"Failed to read configuration file: {ex.Message}",
                "Check file permissions.");
        }
    }

    private static ValidationResult ValidateScanSettings(ScanSettings scan)
    {
        if (scan.DefaultTimeout.TotalMilliseconds <= 0)
        {
            return ValidationResult.Invalid("Configuration.Scan.Timeout", "ERR_CONFIG_TIMEOUT_INVALID",
                "Default timeout must be greater than zero.",
                "Set a positive timeout value (e.g., 3000ms).");
        }

        if (scan.DefaultTimeout.TotalMinutes > 5)
        {
            return ValidationResult.Invalid("Configuration.Scan.Timeout", "ERR_CONFIG_TIMEOUT_TOO_HIGH",
                "Default timeout exceeds 5 minutes.",
                "Reduce the timeout to a reasonable value.");
        }

        if (scan.DefaultMaxConcurrent <= 0)
        {
            return ValidationResult.Invalid("Configuration.Scan.Connections", "ERR_CONFIG_CONNECTIONS_INVALID",
                "Max concurrent connections must be greater than zero.",
                "Set a positive connection limit.");
        }

        if (scan.DefaultMaxConcurrent > AppConstants.MaxConcurrentLimit)
        {
            return ValidationResult.Invalid("Configuration.Scan.Connections", "ERR_CONFIG_CONNECTIONS_TOO_HIGH",
                $"Max concurrent connections exceeds {AppConstants.MaxConcurrentLimit}.",
                "Reduce the connection limit to prevent resource exhaustion.");
        }

        if (scan.DefaultPortRangeStart < AppConstants.MinPort || scan.DefaultPortRangeStart > AppConstants.MaxPort)
        {
            return ValidationResult.Invalid("Configuration.Scan.PortRange", "ERR_CONFIG_PORT_START_INVALID",
                $"Default port range start ({scan.DefaultPortRangeStart}) is outside valid range.",
                $"Use a port between {AppConstants.MinPort} and {AppConstants.MaxPort}.");
        }

        if (scan.DefaultPortRangeEnd < AppConstants.MinPort || scan.DefaultPortRangeEnd > AppConstants.MaxPort)
        {
            return ValidationResult.Invalid("Configuration.Scan.PortRange", "ERR_CONFIG_PORT_END_INVALID",
                $"Default port range end ({scan.DefaultPortRangeEnd}) is outside valid range.",
                $"Use a port between {AppConstants.MinPort} and {AppConstants.MaxPort}.");
        }

        return ValidationResult.Valid("Configuration.Scan");
    }

    private static ValidationResult ValidateStorageSettings(StorageSettings storage)
    {
        if (storage.HistoryRetentionDays <= 0)
        {
            return ValidationResult.Invalid("Configuration.Storage.Retention", "ERR_CONFIG_RETENTION_INVALID",
                "History retention days must be greater than zero.",
                "Set a positive retention period.");
        }

        if (storage.HistoryRetentionDays > AppConstants.MaxRetentionDays)
        {
            return ValidationResult.Invalid("Configuration.Storage.Retention", "ERR_CONFIG_RETENTION_TOO_HIGH",
                "History retention exceeds 10 years.",
                "Use a more reasonable retention period.");
        }

        return ValidationResult.Valid("Configuration.Storage");
    }

    private static ValidationResult ValidateUISettings(UISettings ui)
    {
        if (ui.FontSize <= 0)
        {
            return ValidationResult.Invalid("Configuration.UI.Font", "ERR_CONFIG_FONT_INVALID",
                "Font size must be greater than zero.",
                "Set a positive font size value.");
        }

        if (ui.FontSize > 72)
        {
            return ValidationResult.Invalid("Configuration.UI.Font", "ERR_CONFIG_FONT_TOO_LARGE",
                "Font size exceeds maximum of 72.",
                "Use a smaller font size.");
        }

        return ValidationResult.Valid("Configuration.UI");
    }
}
