# Changelog

All notable changes to SecurePort will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-01

### Added

#### Core Scanning (Part 1A)
- TCP connect scanning engine with async socket operations
- Configurable connection timeout per scan
- Configurable maximum concurrent connections
- Custom port range scanning (start and end port)
- Scan profiles with persistent port lists and settings
- Parallel scan strategy with semaphore-based throttling
- Sequential scan strategy for low-bandwidth environments
- Real-time scan progress tracking and status updates
- Scan cancellation support

#### Scan Profiles (Part 1B)
- Create, edit, and delete scan profiles
- Per-profile port range and timeout configuration
- Per-profile concurrency limits
- Profile persistence across sessions
- Default profile selection

#### Results Management (Part 1A-1C)
- Persistent scan history with JSON storage
- Full result storage with host, port, status, and metadata
- Favorites marking for frequently referenced hosts
- Custom tags for organizing results across categories
- Free-text notes attached to individual results
- Search across all result fields
- Filter by port, status, host, tags, and favorites
- Sort by any result column
- Result merge across multiple scans
- Duplicate detection and removal
- Batch operations on selected results

#### Reporting and Export (Part 2A)
- Export to JSON format with full metadata
- Export to CSV format for spreadsheet compatibility
- Export to TXT format for plain text readability
- Export to HTML format with styled tables
- Customizable report templates
- Timestamped report generation
- Attributed reports with scan context

#### Security and Encryption (Part 2B)
- AES-256-GCM encryption for data at rest
- Encryption key derivation from user passphrase
- Encrypted scan history and results storage
- Path traversal prevention on all file operations
- Input sanitization and validation throughout
- Secure file deletion with overwrite passes
- Security validation layer for all storage operations

#### Backup and Restore (Part 2B-2C)
- Full data backup with JSON serialization
- Backup integrity verification using SHA-256 checksums
- Restore from backup with validation
- Backup metadata with timestamps and checksums
- Selective backup of scan history, profiles, and settings

#### Logging and Diagnostics (Part 3A)
- Application-wide structured logging
- Configurable log levels (Debug, Info, Warning, Error)
- Log file rotation with configurable retention
- Privacy-respecting log content (no sensitive data logged)
- Log viewer in application UI

#### Privacy Dashboard (Part 3B)
- Complete data inventory showing all stored information
- Per-category storage breakdown
- Data age and retention information
- One-click data purge for selected categories
- Privacy status indicators

#### UI Framework (Part 1A, 3A)
- Avalonia 11.0.10 cross-platform desktop application
- Windows, Linux, and macOS support
- Dark theme and light theme with system detection
- Responsive layout with resizable panels
- Keyboard navigation for all interactive elements
- Screen reader compatibility with ARIA-equivalent labels
- High contrast mode for visual accessibility
- Reduced motion mode for motion sensitivity
- Font size scaling with user preference
- Localization framework with default English support

#### Configuration (Part 3C)
- JSON-based application configuration
- Configurable scan defaults (timeout, concurrency, ports)
- Configurable storage settings (retention, auto-delete, encryption)
- Configurable logging settings (level, retention)
- Configurable UI settings (theme, language, font size, accessibility)
- Settings persistence across sessions
- Settings import and export

#### Architecture (Part 1A-3C)
- Clean Architecture with five-layer separation
- Core layer with no external dependencies
- CommunityToolkit.Mvvm for MVVM implementation
- Observable properties and commands for reactive UI
- Modular engine and manager pattern
- Comprehensive unit test suite across all layers

### Security

- AES-256-GCM encryption for all sensitive data at rest
- Path traversal prevention on all file I/O operations
- Injection prevention on all user inputs
- Secure deletion with multi-pass overwrite
- No telemetry, no cloud calls, no data collection
- SHA-256 integrity verification for backups
- Privacy-first design with local-only data storage
