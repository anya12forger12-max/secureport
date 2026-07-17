# Configuration Guide

SecurePort is configured via a JSON file that persists across sessions. This guide explains all available settings, their defaults, and how to manage your configuration.

## Configuration File Location

The configuration file is stored in the platform-specific application data directory:

| Platform | Path |
|----------|------|
| Windows  | `%LOCALAPPDATA%\SecurePort\app-configuration.json` |
| Linux    | `~/.local/share/SecurePort/app-configuration.json` |
| macOS    | `~/Library/Application Support/SecurePort/app-configuration.json` |

If the file does not exist, SecurePort creates it with default values on first launch.

## Configuration Format

The file is a JSON document with four top-level sections:

```json
{
  "Scan": {
    "DefaultTimeout": "00:00:05",
    "DefaultMaxConcurrent": 100,
    "DefaultPortRangeStart": 1,
    "DefaultPortRangeEnd": 1024
  },
  "UI": {
    "Theme": "Dark",
    "Language": "English",
    "FontSize": 14.0,
    "HighContrast": false,
    "ReducedMotion": false,
    "ScreenReaderMode": false
  },
  "Storage": {
    "HistoryRetentionDays": 90,
    "AutoDelete": true,
    "EncryptionEnabled": false
  },
  "Log": {
    "Level": "Info",
    "RetentionDays": 30
  }
}
```

## Settings Reference

### Scan

Controls scan behavior and default parameters.

| Setting | Type | Default | Range | Description |
|---------|------|---------|-------|-------------|
| `DefaultTimeout` | TimeSpan | `00:00:05` (5s) | 1s - 300s | Connection timeout per port during scanning |
| `DefaultMaxConcurrent` | int | `100` | 1 - 10,000 | Maximum concurrent connections per scan |
| `DefaultPortRangeStart` | int | `1` | 1 - 65535 | Starting port for new scans |
| `DefaultPortRangeEnd` | int | `1024` | 1 - 65535 | Ending port for new scans |

The port range start must not exceed the port range end. The hard concurrency limit for the scanner engine is 1,000 concurrent connections regardless of this setting.

### UI

Controls visual appearance and accessibility features.

| Setting | Type | Default | Values | Description |
|---------|------|---------|--------|-------------|
| `Theme` | enum | `Dark` | `Dark`, `Light`, `HighContrast`, `System` | Visual theme applied to the interface |
| `Language` | enum | `English` | `English`, `Spanish`, `French`, `German`, `Japanese`, `Portuguese`, `Arabic`, `Hindi`, `Chinese` | UI display language |
| `FontSize` | double | `14.0` | 8.0 - 72.0 | Base font size in points |
| `HighContrast` | bool | `false` | | Enables high-contrast mode for accessibility |
| `ReducedMotion` | bool | `false` | | Disables UI animations |
| `ScreenReaderMode` | bool | `false` | | Optimizes layout and ARIA attributes for screen readers |

The `System` theme automatically matches your OS dark/light mode preference.

### Storage

Controls data retention and encryption.

| Setting | Type | Default | Range | Description |
|---------|------|---------|-------|-------------|
| `HistoryRetentionDays` | int | `90` | 1 - 3650 | Days to keep scan history before automatic deletion |
| `AutoDelete` | bool | `true` | | Automatically purge history entries older than retention period |
| `EncryptionEnabled` | bool | `false` | | Encrypt local data at rest using AES-256-GCM |

When encryption is enabled, scan results and configuration data are encrypted with AES-256-GCM using a password-derived key (PBKDF2 with 100,000 iterations of SHA-256).

### Log

Controls application logging.

| Setting | Type | Default | Values | Description |
|---------|------|---------|--------|-------------|
| `Level` | enum | `Info` | `Trace`, `Debug`, `Info`, `Warning`, `Error`, `Critical` | Minimum severity level to record |
| `RetentionDays` | int | `30` | 1 - 3650 | Days to keep log files |

Log level descriptions:

- **Trace** - Highly detailed tracing information for diagnostics
- **Debug** - Diagnostic information useful during development
- **Info** - General operational messages about application flow
- **Warning** - Potential issues that do not prevent normal operation
- **Error** - Errors that prevent a specific operation from completing
- **Critical** - Critical failures that may cause application termination

## Configuration Validation

SecurePort validates your configuration on load and after every save. If any value is out of range or malformed, the application reports specific error messages and either uses the default value for that section or reverts to full defaults.

Validation rules:

- Timeout must be between 1 and 300 seconds
- Concurrent connections must be between 1 and 10,000
- Port numbers must be between 1 and 65,535
- Port range start must not exceed port range end
- Font size must be between 8.0 and 72.0
- Theme must be a valid `ThemeMode` value
- Language must be a valid `Language` value
- Log level must be a valid `LogLevel` value
- Retention days must be between 1 and 3,650

## Modifying Configuration

### Through the UI

Navigate to **Settings** in the application to change any configuration value. Changes are saved and applied immediately.

### Through the API

Use the `IConfigurationService` interface in code:

```csharp
// Get current config
var config = await configService.GetConfigurationAsync(cancellationToken);

// Update a specific section
await configService.UpdateConfigurationAsync(
    current => current with
    {
        Scan = current.Scan with { DefaultTimeout = TimeSpan.FromSeconds(10) }
    },
    cancellationToken);

// Reset to defaults
var defaults = await configService.ResetToDefaultsAsync(cancellationToken);
```

### Manual Editing

Close SecurePort, edit the JSON file directly, then relaunch. Ensure valid JSON syntax.

## Backing Up Configuration

Your configuration file is included when you create a backup through the Backup Manager. You can also back it up manually by copying the `app-configuration.json` file to a safe location.

To restore a configuration backup, replace the file in the data directory and restart SecurePort.
