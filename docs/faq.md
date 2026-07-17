# Frequently Asked Questions

## What is SecurePort?

SecurePort is a cross-platform TCP port scanner for defensive cybersecurity. It scans ports on network targets, manages scan results, generates reports in multiple formats, and provides encryption and privacy features. It is a desktop application built with .NET 6 and Avalonia 11.

## Is it safe to use?

Yes. SecurePort is designed for authorized defensive use. All scanning uses standard TCP connect operations and does not exploit vulnerabilities. The application runs entirely on your machine with no telemetry, no cloud calls, and no data collection. You control what you scan.

**Important:** Only scan systems you own or have explicit authorization to test. Unauthorized port scanning may violate laws or policies in your jurisdiction.

## Does it send data anywhere?

No. SecurePort makes zero network requests except the port connections you explicitly initiate during a scan. There is no telemetry, no analytics, no crash reporting, and no remote services of any kind. All data remains on your local machine.

## What ports can I scan?

You can scan any TCP port from 1 to 65,535. The default range is 1-1024 (well-known ports). You can configure custom ranges in Settings or specify a range for individual scans.

## Can I scan my own network?

Yes. Enter your own IP address, hostname, or any other target you are authorized to test. The scan performs standard TCP connections to determine whether ports are open, closed, or filtered.

## What platforms are supported?

SecurePort runs on:

- **Windows** 10 (1809+) or later
- **Linux** with kernel 4.18+ (Ubuntu 20.04+, Fedora 32+, Debian 10+, etc.)
- **macOS** 10.15 (Catalina) or later, on both Intel and Apple Silicon

Requires a desktop environment. A .NET 6 runtime must be available (bundled with the SDK, or as a standalone install).

## How do I contribute?

1. Fork the repository.
2. Create a feature branch.
3. Make your changes following the existing code style and Clean Architecture conventions.
4. Ensure all tests pass with `dotnet test`.
5. Submit a pull request against the main branch.

Read the [Developer Guide](developer-guide.md) for full contribution guidelines.

## What about UDP scanning?

UDP scanning is not yet implemented. The current release (v1.0.0) focuses exclusively on TCP connect scanning. UDP support is planned for v1.1.0. See the [Roadmap](roadmap.md) for details.

## Can I run it without GUI?

Not in v1.0.0. SecurePort currently ships as a desktop GUI application only. A command-line interface is on the roadmap but not yet available.

## How do I customize reports?

Reports are generated through the Reports view. You can:

- Choose between JSON, CSV, TXT, and HTML formats.
- Toggle inclusion of closed ports, timestamps, and scan metadata.
- Add custom titles and tags to saved reports.
- Re-export previously generated reports to a different format.

The report system uses the `IReportGenerator` interface, and new format generators can be added by implementing it.
