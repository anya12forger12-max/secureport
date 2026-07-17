# Roadmap

SecurePort's development plan across upcoming releases. Timelines are approximate and may shift based on community feedback and priorities.

## v1.0.0 (Current Release)

The initial stable release providing core scanning and results management.

- TCP connect scanning with configurable concurrency and timeouts
- Scan profiles with custom port ranges
- Real-time progress updates during scans
- Scan history with full result storage
- Search, filter, and sort across all results
- Favorites, tags, and notes for organizing results
- Export to JSON, CSV, TXT, and HTML formats
- Backup and restore with SHA-256 integrity verification
- Scan comparison to detect changes between scans
- Statistics engine with charts and visualizations
- Service identification from a built-in knowledge base
- AES-256-GCM encryption for data at rest
- Privacy dashboard showing all stored data
- Dark, light, high-contrast, and system themes
- Accessibility-first UI (keyboard navigation, screen readers, high contrast)
- Cross-platform support (Windows, Linux, macOS)
- Clean Architecture with 15 source projects
- Comprehensive unit test suite

## v1.1.0 (Next Release)

**Target:** Expanding scanning capabilities and extensibility.

- **UDP scanning** - Send datagrams and analyze responses for common UDP services
- **Plugin system** - Load custom scanner strategies, report generators, and service detectors from external assemblies
- **Additional report formats** - PDF export, Markdown export
- **Custom scan profiles** - Save and manage named scan configurations
- **Improved service detection** - Banner grabbing with configurable probes
- **Filter presets** - Save and recall common filter configurations

## v1.2.0

**Target:** Advanced scanning features and automation.

- **Multi-target scanning** - Scan multiple hosts in a single session
- **Scheduled scans** - Configure recurring scans with time-based triggers
- **Custom service database** - Import and extend the service knowledge base from external JSON files
- **Scan templates** - Pre-configured scan profiles for common scenarios (web server, database, full port sweep)
- **Network range scanning** - Scan CIDR notations (e.g., 192.168.1.0/24)
- **Comparison reports** - Side-by-side HTML comparison reports with diff highlighting
- **Data retention policies** - Per-category retention rules with preview before cleanup
- **Localization** - French, Spanish, German, Japanese translations

## v2.0.0 (Vision)

**Target:** Comprehensive network assessment platform.

- **Network topology mapping** - Visual network map showing discovered hosts and services
- **Credential scanning with authentication** - Scan for default credentials on common services (SSH, FTP, databases)
- **Distributed scanning** - Coordinate scans across multiple machines
- **CLI interface** - Full command-line version for scripting and CI/CD integration
- **Custom alerting** - Notifications when scan results change in specified ways
- **Import/export compatibility** - Read and write Nmap XML, Masscan JSON output formats
- **API mode** - REST API for integrating with other security tools
- **Plugin marketplace** - Community-contributed extensions

## Contributing to the Roadmap

Have ideas for future development? The roadmap is shaped by community needs:

1. **Open an issue** on GitHub with the `enhancement` label describing your feature request.
2. **Explain the use case** - what problem does it solve, who benefits from it.
3. **Discuss with maintainers** - we may reprioritize based on community interest.
4. **Contribute directly** - many roadmap features can be implemented as community contributions.

We prioritize features that align with SecurePort's core principles: privacy-first, offline-capable, accessibility-focused, and defensively oriented.
