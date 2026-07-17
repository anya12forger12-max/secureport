# Installation Guide

This guide covers building and running SecurePort from source on all supported platforms.

## Prerequisites

### .NET 6 SDK

SecurePort requires the [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (6.0 or later patch releases). Verify your installation:

```bash
dotnet --version
```

Any version matching `6.0.x` will work. The .NET 6 runtime is bundled with the SDK.

### Operating System Requirements

| Platform | Minimum Version | Architecture |
|----------|----------------|--------------|
| Windows  | Windows 10 (1809+) | x64, ARM64 |
| Linux    | Kernel 4.18+ (glibc 2.18+) | x64, ARM64 |
| macOS    | macOS 10.15 (Catalina) | x64, ARM64 (Apple Silicon) |

All platforms require a desktop environment for the GUI. SecurePort uses Avalonia for cross-platform rendering, so no additional UI framework dependencies are needed.

### Optional

- **Git** - for cloning the repository
- **An IDE** - Visual Studio 2022, Rider, or VS Code with the C# Dev Kit extension

## Building from Source

### 1. Clone the Repository

```bash
git clone https://github.com/yourorg/SecurePort.git
cd SecurePort
```

### 2. Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages including Avalonia, CommunityToolkit.Mvvm, and testing libraries.

### 3. Build the Solution

```bash
dotnet build
```

This compiles all 15 source projects and the test project. By default it builds in `Debug` configuration.

For an optimized build:

```bash
dotnet build -c Release
```

### 4. Run Tests (Optional)

```bash
dotnet test
```

Runs the full unit test suite covering the Core, Scanner, Configuration, Results, Storage, and other layers.

## Running the Application

### From the Command Line

```bash
dotnet run --project src/SecurePort.UI
```

The main window will open after a brief startup. No configuration is required for first use.

### From a Built Binary

After building in Release mode, the output is in:

```
src/SecurePort.UI/bin/Release/net6.0/
```

Run the platform-specific executable directly:

- **Windows:** `SecurePort.UI.exe`
- **Linux/macOS:** `SecurePort.UI`

### Published Self-Contained Build

To create a standalone executable with no .NET runtime dependency:

```bash
dotnet publish src/SecurePort.UI -c Release -r win-x64 --self-contained
dotnet publish src/SecurePort.UI -c Release -r linux-x64 --self-contained
dotnet publish src/SecurePort.UI -c Release -r osx-x64 --self-contained
```

Output appears in `src/SecurePort.UI/bin/Release/net6.0/<runtime>/publish/`.

## First-Time Setup

1. **Launch SecurePort** using one of the methods above.
2. **Theme selection** - The application starts in Dark theme. Change it in Settings if needed.
3. **Scan your first target** - Enter a hostname or IP address in the scan field, select a port range, and click Start.
4. **Review results** - Open, closed, and filtered ports appear in real time as the scan progresses.
5. **Export results** - Use the Reports view to export to JSON, CSV, TXT, or HTML.

No account creation, login, or network registration is required. All data stays on your machine.

## Uninstallation

SecurePort stores data in the platform-specific application data directory. To fully uninstall:

### Remove Application Files

Delete the directory where you cloned or built the repository.

### Remove Application Data

| Platform | Data Location |
|----------|--------------|
| Windows  | `%LOCALAPPDATA%\SecurePort\` |
| Linux    | `~/.local/share/SecurePort/` |
| macOS    | `~/Library/Application Support/SecurePort/` |

Delete the `SecurePort` folder in the appropriate location to remove all scan history, configuration, reports, backups, and logs.

### Remove .NET SDK (If No Longer Needed)

Follow the [.NET uninstall instructions](https://learn.microsoft.com/dotnet/core/install/remove-runtime-sdk-operators) for your platform.
