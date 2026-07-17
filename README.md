<div align="center">

# SecurePort

**Cross-platform TCP port scanner with privacy-first design and offline operation.**

[![.NET 6](https://img.shields.io/badge/.NET-6.0-purple)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.0.10-blue)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)]()

SecurePort is a defensive cybersecurity desktop application for TCP port scanning, results management, and reporting. All features operate locally with zero cloud dependencies, zero telemetry, and zero data collection.

</div>

---

## Features

### Core Scanning
- TCP connect scanning with configurable concurrency
- Scan profiles with custom port lists and timeouts
- Parallel and sequential scan strategies
- Real-time progress and status updates

### Results Management
- Persistent scan history with full result storage
- Favorites, tags, and notes for organizing results
- Search, filter, and sort across all scan data
- Merge and deduplicate results across scans

### Reporting
- Export to JSON, CSV, TXT, and HTML formats
- Customizable report templates
- Timestamped and attributed report metadata
- Backup and restore with SHA-256 integrity verification

### Security & Privacy
- AES-256-GCM encryption for sensitive data at rest
- Path traversal and injection prevention throughout
- Secure file deletion with overwrite passes
- Privacy dashboard showing all stored data
- No network calls except the requested scans

### UI/UX
- Cross-platform desktop application (Windows, Linux, macOS)
- Dark and light theme support
- Responsive layout with resizable panels
- Accessibility-first design (keyboard navigation, screen readers, high contrast)

### Developer
- Clean Architecture with clear layer separation
- CommunityToolkit.Mvvm for reactive UI binding
- Comprehensive unit test suite
- Modular engine and manager pattern

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                      UI Layer                        │
│          Views · ViewModels · Converters             │
├─────────────────────────────────────────────────────┤
│                  Results Layer                       │
│        ResultsManager · Validators · DTOs            │
├─────────────────────────────────────────────────────┤
│                  Storage Layer                       │
│     JsonFileStore · BackupManager · Validators       │
├─────────────────────────────────────────────────────┤
│                  Security Layer                      │
│  EncryptionProvider · SecurityValidator · Logger      │
├─────────────────────────────────────────────────────┤
│                    Core Layer                        │
│  ScanEngine · ScanProfile · PortScanner · Models     │
└─────────────────────────────────────────────────────┘
```

Dependencies flow inward: UI → Results → Storage → Security → Core. Core has no dependency on any outer layer.

---

## Quick Start

**Prerequisites:** [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

```bash
git clone https://github.com/yourorg/SecurePort.git
cd SecurePort
dotnet restore
dotnet build
```

Run the application:

```bash
dotnet run --project SecurePort.UI
```

Run the test suite:

```bash
dotnet test
```

---

## Project Structure

```
SecurePort/
├── src/
│   ├── SecurePort.Core/              # Core scanning logic and models
│   ├── SecurePort.Security/          # Encryption, validation, logging
│   ├── SecurePort.Storage/           # File I/O, backups, persistence
│   ├── SecurePort.Results/           # Results management and reporting
│   ├── SecurePort.UI/                # Avalonia views and view models
│   ├── SecurePort.UI.Tests/          # UI layer tests
│   ├── SecurePort.Results.Tests/     # Results layer tests
│   ├── SecurePort.Storage.Tests/     # Storage layer tests
│   ├── SecurePort.Security.Tests/    # Security layer tests
│   └── SecurePort.Core.Tests/        # Core layer tests
├── docs/
│   ├── architecture.md
│   ├── developer-guide.md
│   ├── user-guide.md
│   ├── security-guide.md
│   └── accessibility-guide.md
├── SecurePort.sln
└── README.md
```

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome. Please read the [Developer Guide](docs/developer-guide.md) before submitting a pull request.

## Security

To report vulnerabilities, please see the [Security Policy](docs/security-guide.md). Do not open public issues for security reports.
