# User Guide

## Installation

### Pre-built Binaries

Download the latest release for your platform from the Releases page. Extract and run.

### Building from Source

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0).

```bash
git clone https://github.com/yourorg/SecurePort.git
cd SecurePort
dotnet restore && dotnet build
dotnet run --project src/SecurePort.UI
```

## Scanning

### Setting a Target

Enter an IP address or hostname in the **Target** field.

- Single IP: `192.168.1.1`
- Hostname: `example.com`
- Localhost: `127.0.0.1`

### Specifying Ports

Enter port ranges in the **Ports** field:

- Single port: `80`
- Comma-separated: `22,80,443`
- Range: `1-1024`
- Mixed: `22,80,443,8000-9000`

### Scan Profiles

Scan profiles define port lists, timeouts, and concurrency. Select a profile from the dropdown or create a custom one.

| Profile | Ports | Concurrency | Timeout |
|---------|-------|-------------|---------|
| Quick | Common 100 | 50 | 2s |
| Standard | Well-known 1000 | 100 | 3s |
| Full | 1–65535 | 200 | 1s |
| Custom | User-defined | User-defined | User-defined |

### Running a Scan

Click **Start Scan** or press `Ctrl+Enter`. Progress is displayed in real time. Click **Stop** to cancel an in-progress scan.

## Results

### Viewing Results

Completed scans appear in the **Results** panel. Select a scan to view its details, including:

- Target and scan parameters
- Open ports with service names
- Response times
- Scan duration

### Dashboard

The dashboard provides an overview of:

- Total scans performed
- Open ports discovered
- Most common services
- Recent activity

## Organizing Results

### Favorites

Star any scan result to mark it as a favorite. Filter the results list to show only favorites.

### Tags

Add tags to results for custom categorization:

- Tag names are freeform text
- Multiple tags per result
- Filter by tag in the results list

### Notes

Add freeform text notes to any scan result. Notes are stored locally and included in exports.

## Search, Filter, and Sort

### Search

Use the search bar to find results by:

- Target address
- Port number
- Service name
- Tag name
- Note content

### Filter

Apply filters to narrow the results list:

- By status (open, closed, filtered)
- By port range
- By date range
- By tag
- By favorites only

### Sort

Sort results by:

- Date (newest/oldest first)
- Target (A–Z / Z–A)
- Open port count
- Scan duration

## Exporting Reports

Select one or more scan results and click **Export**. Choose a format:

| Format | Use Case |
|--------|----------|
| **JSON** | Programmatic consumption, data exchange |
| **CSV** | Spreadsheet analysis, data import |
| **TXT** | Plain text, terminal-friendly |
| **HTML** | Formatted report for sharing, printing |

Reports include scan metadata, timestamps, and all result data.

## Backup and Restore

### Creating a Backup

Go to **Settings → Backup** and click **Create Backup**. This saves all scan data, settings, and results to a single encrypted file with a SHA-256 checksum.

### Restoring a Backup

Go to **Settings → Backup** and click **Restore**. Select a backup file. The application verifies integrity before restoring.

**Note:** Restoring a backup replaces all current data.

## Privacy Dashboard

The Privacy Dashboard (accessible from the sidebar) shows:

- All data currently stored by the application
- Storage location on disk
- Encryption status
- Options to clear individual data categories or all data

SecurePort does not make any network connections except for the port scans you explicitly request. There is no telemetry, analytics, or cloud synchronization.

## Settings

### General

- **Default scan profile** — which profile to use on startup
- **Auto-save results** — save results automatically after each scan
- **Confirm on exit** — prompt before closing with unsaved changes

### Appearance

- **Theme** — Dark, Light, or System
- **Accent color** — choose a custom accent
- **Font size** — scale the UI font (see Accessibility below)

### Advanced

- **Data directory** — where scan data is stored (default: application directory)
- **Backup schedule** — optional automatic backup reminders
- **Log level** — verbosity of internal logs (Debug, Info, Warning, Error)

## Accessibility

SecurePort is designed to be accessible to all users:

- **Full keyboard navigation** — every feature is reachable without a mouse
- **Screen reader support** — ARIA labels and semantic markup throughout
- **High contrast mode** — increases contrast ratios for visibility
- **Reduced motion** — disables animations for motion-sensitive users
- **Font scaling** — adjust text size from 75% to 200%

See the [Accessibility Guide](accessibility-guide.md) for details.
