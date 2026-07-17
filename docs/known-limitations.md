# Known Limitations

This document tracks known limitations in the current release of SecurePort (v1.0.0). Many of these are planned for future releases as outlined in the [Roadmap](roadmap.md).

## Scanning Limitations

### TCP Scanning Only

SecurePort currently performs TCP connect scanning only. UDP scanning, SYN scanning, and ICMP ping sweeps are not yet implemented despite being defined in the `ScanType` and `ProtocolType` enums.

**Workaround:** Use a dedicated tool like Nmap or Masscan for UDP or raw socket scanning.

**Planned:** UDP scanning is targeted for v1.1.0.

### Single-Target Scans

Each scan session operates against a single target host. You cannot scan multiple hosts in one session or scan a network range (CIDR notation).

**Workaround:** Run separate scans for each target and use scan comparison to review results side by side.

**Planned:** Multi-target and CIDR scanning are targeted for v1.2.0.

### No Scheduled or Recurring Scans

Scans must be started manually through the UI. There is no scheduler, cron integration, or API for automated scan execution.

**Workaround:** Use OS-level scheduling (Windows Task Scheduler, cron on Linux/macOS) to launch the application with predefined settings, though this requires manual setup.

**Planned:** Scheduled scans are targeted for v1.2.0.

## Data Limitations

### In-Memory Results

Scan results are held in memory during a session. While scan history and reports can be persisted to disk through the storage layer, the in-memory results repository does not survive application restarts within an active scan session. If the application crashes mid-scan, unsaved results are lost.

**Workaround:** Export or save results immediately after a scan completes.

### No PDF Export

Although the `ExportFormat` enum includes `PDF`, the PDF report generator has not been implemented. Available formats are JSON, CSV, TXT, and HTML.

**Workaround:** Export to HTML and use your browser's Print to PDF feature.

**Planned:** Native PDF export is targeted for v1.1.0.

## UI Limitations

### Desktop Only

SecurePort is a desktop GUI application. There is no web interface, no mobile app, and no command-line interface.

**Planned:** A CLI is planned for v2.0.0.

### Limited Localization

While the `Language` enum defines nine languages (English, Spanish, French, German, Japanese, Portuguese, Arabic, Hindi, Chinese), only English is fully localized in v1.0.0. Other language selections will display English text with placeholder resource keys.

**Planned:** Full localization for major languages is targeted for v1.2.0.

### No Plugin System

There is no mechanism to extend SecurePort with custom scanner strategies, report formats, or service detectors at runtime. All functionality is compiled into the source projects.

**Workaround:** Modify the source code directly following the existing engine and interface patterns.

**Planned:** A plugin system is targeted for v1.1.0.

## Security Limitations

### Encryption is Optional and Manual

AES-256-GCM encryption for data at rest is available but disabled by default. Enabling it requires a manual toggle in Settings. There is no key rotation, no master password prompt on startup, and no automatic encryption of legacy data.

### No Authentication

The application has no login screen or user authentication. Anyone with access to the machine has full access to SecurePort and its stored data.

## Platform Limitations

### Linux Native Dependencies

On Linux, SecurePort depends on system libraries for Avalonia rendering (`libX11`, `libfontconfig`, etc.). These are usually present on desktop installations but may be missing on minimal or server systems. The application will fail to start without them.

See the [Troubleshooting Guide](troubleshooting.md) for the full list of required packages.
