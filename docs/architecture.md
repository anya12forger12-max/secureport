# Architecture Guide

SecurePort follows Clean Architecture with strict inward-facing dependencies. Each layer depends only on the layer(s) inside it, never outward.

## Layer Overview

| Layer | Responsibility | Key Types |
|-------|---------------|-----------|
| **Core** | Scanning logic, domain models, port definitions | `ScanEngine`, `ScanProfile`, `PortScanner`, `ScanResult` |
| **Security** | Encryption, input validation, secure logging | `EncryptionProvider`, `SecurityValidator`, `SecureLogger` |
| **Storage** | File I/O, JSON persistence, backup/restore | `JsonFileStore`, `BackupManager`, `FileValidator` |
| **Results** | Results management, reporting, deduplication | `ResultsManager`, `ReportGenerator`, `ResultValidator` |
| **UI** | Views, ViewModels, user interaction | `MainWindow`, `MainViewModel`, converters, resources |

## Dependency Graph

```
SecurePort.UI
  ├── SecurePort.Results
  │     ├── SecurePort.Storage
  │     │     ├── SecurePort.Security
  │     │     │     └── SecurePort.Core
  │     │     └── SecurePort.Core
  │     └── SecurePort.Core
  └── SecurePort.Core (via Results)
```

All 15 projects and their dependencies:

| Project | Depends On |
|---------|-----------|
| `SecurePort.Core` | *(none)* |
| `SecurePort.Security` | Core |
| `SecurePort.Storage` | Core, Security |
| `SecurePort.Results` | Core, Security, Storage |
| `SecurePort.UI` | Core, Security, Storage, Results |
| `SecurePort.Core.Tests` | Core |
| `SecurePort.Security.Tests` | Core, Security |
| `SecurePort.Storage.Tests` | Core, Security, Storage |
| `SecurePort.Results.Tests` | Core, Security, Storage, Results |
| `SecurePort.UI.Tests` | Core, Security, Storage, Results, UI |

**Rule:** A project may only reference projects listed above it in the dependency chain. Outer layers must never be referenced by inner layers.

## Data Flow

### Scan Lifecycle

```
User Input → Profile Selection → ScanEngine.Execute()
  → PortScanner.ScanHost() → Concurrent results
  → ScanResult returned → ResultsManager.Store()
  → JsonFileStore.Persist() → Encrypted at rest
```

1. User specifies target, ports, and profile in the UI.
2. `MainViewModel` delegates to `ScanEngine` with a `ScanProfile`.
3. `ScanEngine` orchestrates `PortScanner` across target/port combinations.
4. Results stream back via thread-safe callbacks.
5. `ResultsManager` validates, deduplicates, and stores via `JsonFileStore`.
6. Backups are generated with SHA-256 checksums.

### Report Lifecycle

```
ResultsManager.GetResults() → ReportGenerator.Generate()
  → Format-specific renderer → File output (JSON/CSV/TXT/HTML)
```

Reports are generated entirely from locally stored data. No external services are called.

## Thread Safety

### Scanning

`PortScanner` uses `SemaphoreSlim` to limit concurrent connections:

```csharp
private readonly SemaphoreSlim _semaphore = new(maxConcurrency);

public async Task<PortResult> ScanPortAsync(string host, int port, CancellationToken ct)
{
    await _semaphore.WaitAsync(ct);
    try { /* connect and probe */ }
    finally { _semaphore.Release(); }
}
```

### Results Storage

`ResultsManager` protects its internal collection with `ConcurrentDictionary` and explicit locking for compound operations:

```csharp
private readonly ConcurrentDictionary<string, ScanResult> _results = new();
private readonly SemaphoreSlim _writeLock = new(1, 1);
```

Reads are lock-free via `ConcurrentDictionary`. Writes that require atomicity (e.g., merge + deduplicate) acquire `_writeLock`.

### UI Updates

All UI-bound state flows through `CommunityToolkit.Mvvm` `ObservableObject` properties, which marshal change notifications to the UI thread via `Dispatcher`.

## Offline-First Design

SecurePort operates without network access beyond the scanned targets:

- **No telemetry.** No analytics, crash reporting, or usage data is collected.
- **No cloud services.** All storage is local filesystem.
- **No external dependencies at runtime.** NuGet packages are build-time only.
- **No auto-updates.** Version checks are disabled.
- **No authentication.** The application has no user accounts or login screens.

All data remains on the user's machine. Encrypted storage and secure deletion ensure data does not persist beyond the user's intent.
