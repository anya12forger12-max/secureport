# API Reference

SecurePort's internal architecture is organized around interfaces defined in `SecurePort.Core`. This reference covers the core interfaces, key models, engine abstractions, and how to extend the application with new components.

All interfaces live in the `SecurePort.Core.Interfaces` namespace. Models are in `SecurePort.Core.Models`. Enums are in `SecurePort.Core.Enums`.

---

## Core Interfaces

### IScanner

Executes network port scans and reports progress.

```csharp
public interface IScanner
{
    IObservable<ScanProgress> ProgressChanged { get; }
    bool IsScanning { get; }
    Task<ScanSession> StartScanAsync(ScanTarget target, CancellationToken ct);
    Task CancelScanAsync();
}
```

- `StartScanAsync` returns a `ScanSession` containing all results when complete.
- `ProgressChanged` emits `ScanProgress` instances with percentage, current port, and estimated time remaining.

---

### IResultsRepository

Storage and retrieval for `ResultEntry` records.

```csharp
public interface IResultsRepository
{
    Task SaveAsync(ResultEntry entry, CancellationToken ct);
    Task SaveBatchAsync(IReadOnlyList<ResultEntry> entries, CancellationToken ct);
    Task<ResultEntry?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ResultEntry>> GetByScanIdAsync(Guid scanId, CancellationToken ct);
    Task<IReadOnlyList<ResultEntry>> GetAllAsync(CancellationToken ct);
    Task UpdateAsync(ResultEntry entry, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<int> DeleteByScanIdAsync(Guid scanId, CancellationToken ct);
    Task<int> CountAsync(CancellationToken ct);
    Task<int> CountByScanIdAsync(Guid scanId, CancellationToken ct);
}
```

The current implementation is `InMemoryResultsRepository`. A persistent storage-backed implementation is planned.

---

### IStatisticsEngine

Computes aggregate statistics from scan sessions and results.

```csharp
public interface IStatisticsEngine
{
    ResultsStatistics ComputeStatistics(
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results);
}
```

Returns a `ResultsStatistics` record with counts, averages, frequency tables, and time-series data.

---

### IScanComparisonEngine

Compares two scan sessions and identifies differences.

```csharp
public interface IScanComparisonEngine
{
    ScanComparisonResult Compare(ScanSession previous, ScanSession current);
    IReadOnlyList<ScanComparisonResult> CompareMultiple(IReadOnlyList<ScanSession> sessions);
}
```

Returns `ScanComparisonResult` with per-port `PortDifference` entries classified by `DifferenceType` (NewlyOpen, NewlyClosed, ServiceChanged, etc.).

---

### IResultsFilterEngine

Applies combinable filter criteria to result collections.

```csharp
public interface IResultsFilterEngine
{
    IReadOnlyList<ResultEntry> ApplyFilters(IReadOnlyList<ResultEntry> results, ResultFilterCriteria criteria);
}
```

`ResultFilterCriteria` supports filtering by state, protocol, service name, port range, response time, date range, favorites, tags, target, and scan ID. Multiple criteria are combined with AND logic.

---

### IResultsSortEngine

Sorts results by configurable criteria.

```csharp
public interface IResultsSortEngine
{
    IReadOnlyList<ResultEntry> Sort(IReadOnlyList<ResultEntry> results, SortCriteria criteria);
}
```

`SortCriteria` specifies a `SortField` (Port, State, Service, Protocol, ResponseTime, Target, ScanDate, ScanDuration, Confidence, Favorites) and a `Descending` flag.

---

### IResultsSearchEngine

Full-text local search across result fields.

```csharp
public interface IResultsSearchEngine
{
    IReadOnlyList<ResultEntry> Search(IReadOnlyList<ResultEntry> results, string query);
    IReadOnlyList<TextRange> FindMatches(string text, string query);
}
```

Searches are case-insensitive with multi-keyword support. `FindMatches` returns ranges for UI highlighting.

---

### IReportPreparationEngine

Assembles report data from sessions and results without performing export.

```csharp
public interface IReportPreparationEngine
{
    ReportData PrepareReport(
        string title,
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results,
        IReadOnlyList<string>? appliedFilters = null);

    ReportData PrepareComparisonReport(
        string title,
        ScanSession previous,
        ScanSession current,
        IReadOnlyList<ResultEntry> previousResults,
        IReadOnlyList<ResultEntry> currentResults);
}
```

Returns a `ReportData` record containing title, sessions, results, statistics, chart data, and optional comparisons.

---

### IReportGenerator

Generates formatted report files from scan sessions.

```csharp
public interface IReportGenerator
{
    Task<string> GenerateReportAsync(ScanSession session, ExportOptions options, CancellationToken ct);
    Task<byte[]> GenerateReportBytesAsync(ScanSession session, ExportOptions options, CancellationToken ct);
    string GetFileExtension(ExportFormat format);
}
```

Implementations exist for each format: `JsonReportGenerator`, `CsvReportGenerator`, `HtmlReportGenerator`, `TxtReportGenerator`.

---

### IVisualizationEngine

Generates chart-ready data sets from scan results.

```csharp
public interface IVisualizationEngine
{
    ChartDataSet GeneratePortStateChart(IReadOnlyList<ResultEntry> results);
    ChartDataSet GenerateServiceDistributionChart(IReadOnlyList<ResultEntry> results);
    ChartDataSet GeneratePortRangeChart(IReadOnlyList<ResultEntry> results);
    ChartDataSet GenerateResponseTimeChart(IReadOnlyList<ResultEntry> results);
    ChartDataSet GenerateScanDurationTrendChart(IReadOnlyList<ScanSession> sessions);
    ChartDataSet GenerateServicesFrequencyChart(IReadOnlyList<ResultEntry> results);
    ChartDataSet GenerateHistoricalTimelineChart(IReadOnlyList<ScanSession> sessions);
}
```

Each method returns a `ChartDataSet` record ready for rendering.

---

### IStorageProvider\<T\>

Generic key-value storage with CRUD operations.

```csharp
public interface IStorageProvider<T>
{
    Task<T?> GetAsync(string key, CancellationToken ct);
    Task SetAsync(string key, T value, CancellationToken ct);
    Task<bool> RemoveAsync(string key, CancellationToken ct);
    Task<bool> ExistsAsync(string key, CancellationToken ct);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct);
    Task ClearAsync(CancellationToken ct);
}
```

Implementations: `JsonStorageProvider`, `EncryptedStorageProvider`.

---

### IConfigurationService

Read/write access to application configuration.

```csharp
public interface IConfigurationService
{
    Task<AppConfiguration> GetConfigurationAsync(CancellationToken ct);
    Task SaveConfigurationAsync(AppConfiguration configuration, CancellationToken ct);
    Task UpdateConfigurationAsync(Func<AppConfiguration, AppConfiguration> update, CancellationToken ct);
    Task<AppConfiguration> ResetToDefaultsAsync(CancellationToken ct);
    event EventHandler<AppConfiguration>? ConfigurationChanged;
}
```

Thread-safe via `SemaphoreSlim`. Configuration changes are broadcast through the `ConfigurationChanged` event.

---

### IBackupManager

Creates, restores, and manages backups with integrity verification.

```csharp
public interface IBackupManager
{
    Task<BackupMetadata> CreateBackupAsync(string name, string? description, CancellationToken ct);
    Task<bool> RestoreBackupAsync(Guid backupId, CancellationToken ct);
    Task<bool> DeleteBackupAsync(Guid backupId, CancellationToken ct);
    Task<bool> RenameBackupAsync(Guid backupId, string newName, CancellationToken ct);
    Task<string> ExportBackupAsync(Guid backupId, string destinationPath, CancellationToken ct);
    Task<BackupMetadata?> ImportBackupAsync(string sourcePath, CancellationToken ct);
    Task<IReadOnlyList<BackupMetadata>> GetAllBackupsAsync(CancellationToken ct);
    Task<BackupMetadata?> GetBackupByIdAsync(Guid backupId, CancellationToken ct);
    Task<IntegrityCheckResult> ValidateBackupAsync(Guid backupId, CancellationToken ct);
}
```

---

### IReportVerificationManager

Verifies data integrity using SHA-256 checksums.

```csharp
public interface IReportVerificationManager
{
    string ComputeChecksum(byte[] data);
    Task<string> ComputeFileChecksumAsync(string filePath, CancellationToken ct);
    Task<IntegrityCheckResult> VerifyFileAsync(string filePath, string expectedChecksum, CancellationToken ct);
    Task<IntegrityCheckResult> VerifyReportAsync(ReportMetadata report, CancellationToken ct);
    Task<IntegrityCheckResult> VerifyBackupAsync(BackupMetadata backup, CancellationToken ct);
}
```

---

### IReportManager

Full lifecycle management for generated reports.

```csharp
public interface IReportManager
{
    Task<ReportMetadata> GenerateAndStoreReportAsync(ReportData data, ExportFormat format, string? customTitle, CancellationToken ct);
    Task<IReadOnlyList<ReportMetadata>> GetAllReportsAsync(CancellationToken ct);
    Task<ReportMetadata?> GetReportByIdAsync(Guid reportId, CancellationToken ct);
    Task<bool> DeleteReportAsync(Guid reportId, CancellationToken ct);
    Task<bool> RenameReportAsync(Guid reportId, string newTitle, CancellationToken ct);
    Task<ReportMetadata?> DuplicateReportAsync(Guid reportId, string? newTitle, CancellationToken ct);
    Task<bool> ArchiveReportAsync(Guid reportId, CancellationToken ct);
    Task<bool> RestoreReportAsync(Guid reportId, CancellationToken ct);
    Task<IReadOnlyList<ReportMetadata>> SearchReportsAsync(ReportSearchCriteria criteria, CancellationToken ct);
    Task<bool> AddTagToReportAsync(Guid reportId, string tag, CancellationToken ct);
    Task<bool> RemoveTagFromResultAsync(Guid reportId, string tag, CancellationToken ct);
    Task<string> ReExportReportAsync(Guid reportId, string destinationPath, CancellationToken ct);
}
```

---

## Supporting Interfaces

### Data Management

| Interface | Purpose |
|-----------|---------|
| `IFavoritesManager` | Toggle, set, and query favorite status on result entries |
| `ITagManager` | Create, rename, delete tags; assign tags to results |
| `INotesManager` | Attach, retrieve, search text notes on results |
| `IScanHistoryRepository` | Persist and query scan session history |
| `IResultsCache` | In-memory cache for result entries |
| `IDataRetentionManager` | Enforce retention policies and automatic cleanup |

### Security and File Operations

| Interface | Purpose |
|-----------|---------|
| `ISecureFileOperations` | Atomic writes, safe reads, path validation, secure deletion |
| `INetworkResolver` | DNS resolution and reverse lookups |

### Monitoring and Diagnostics

| Interface | Purpose |
|-----------|---------|
| `IHealthMonitor` | Check health of application subsystems |
| `IPeriodicHealthMonitor` | Periodic health checks with change notifications |
| `IDiagnosticsService` | Collect memory, thread pool, and version diagnostics |
| `IStorageMonitor` | Storage usage, privacy status, and data deletion |
| `ILogManager` | Search, export, and manage application logs |

### Application Services

| Interface | Purpose |
|-----------|---------|
| `IThemeService` | Manage visual themes and accessibility states |
| `ILocalizationService` | Access localized UI strings |
| `IVersionService` | Version info, update checks, schema migration |
| `ISchemaMigrator` | Migrate configuration between schema versions |
| `IServiceKnowledgeBase` | Query the local service identification database |
| `IFailureRecovery` | Recover from data corruption and interrupted operations |
| `ILoggingProvider` | Low-level log writing abstraction |

---

## Key Models

### ResultEntry

A single result with extended metadata: target, port, protocol, state, service info, response time, banner, tags, notes, and favorite status.

### ScanTarget

Immutable scan specification: host, port range, protocol, scan type, timeout, concurrency, and creation timestamp.

### ScanSession

A complete scan session: target, results, status, timing, and error information.

### ScanResult

An individual port scan result: host, port, state, service name, banner, response time, and protocol.

### ScanProgress

Real-time progress: session ID, total/scanned ports, open ports, current port, elapsed time, ETA, and percentage.

### ReportData

Structured report content: title, timestamp, sessions, results, statistics, chart data, comparisons, notes, and applied filters.

### ResultsStatistics

Computed statistics: scan counts, port counts, response time averages, service frequencies, port frequencies, and scan frequency over time.

### ScanComparisonResult

Comparison between two sessions: per-port differences classified by type, counts of changes, and duration difference.

### ExportOptions

Export configuration: format, file path, and toggles for closed ports, timestamps, and scan metadata.

### AppConfiguration

Root configuration: `ScanSettings`, `UISettings`, `StorageSettings`, and `LogSettings`.

---

## Extending with New Engines

To add a new engine or service, follow these steps:

1. **Define the interface** in `SecurePort.Core.Interfaces`:

```csharp
public interface IMyNewEngine
{
    Task<Result> ProcessAsync(Input input, CancellationToken ct);
}
```

2. **Create the model** in `SecurePort.Core.Models` if needed:

```csharp
public sealed record Result { /* ... */ }
public sealed record Input { /* ... */ }
```

3. **Implement the engine** in the appropriate project (e.g., `SecurePort.Results.Engines` or `SecurePort.Scanner.Engine`):

```csharp
public sealed class MyNewEngine : IMyNewEngine
{
    public async Task<Result> ProcessAsync(Input input, CancellationToken ct)
    {
        // Implementation
    }
}
```

4. **Register it** in the dependency injection container alongside existing engines.

5. **Write tests** in `tests/SecurePort.Tests/` following the existing patterns.

The Clean Architecture dependency flow is: **UI -> Results -> Storage -> Security -> Core**. Core defines all interfaces and models. Outer layers implement them. Core must never depend on any outer layer.
