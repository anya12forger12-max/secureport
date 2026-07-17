# Troubleshooting Guide

## Build Issues

### Avalonia NuGet Packages Fail to Restore

**Symptoms:** `dotnet restore` fails with errors about Avalonia packages not being found.

**Fixes:**
1. Verify you have the correct NuGet source configured:
   ```bash
   dotnet nuget list source
   ```
   Ensure `nuget.org` is listed. If not:
   ```bash
   dotnet nuget add source https://api.nuget.org/v3/index.json
   ```
2. Clear the NuGet cache and retry:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```
3. Verify your internet connection is active and not behind a proxy blocking NuGet.

### .NET 6 SDK Not Found

**Symptoms:** `dotnet` command is not recognized, or build errors reference unsupported target framework.

**Fixes:**
1. Install the [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0).
2. If you have multiple SDK versions installed, ensure the project uses .NET 6:
   ```bash
   dotnet --list-sdks
   ```
3. Create a `global.json` in the repository root to pin the SDK version:
   ```json
   {
     "sdk": {
       "version": "6.0.0",
       "rollForward": "latestFeature"
     }
   }
   ```

### Build Errors After Pulling Latest Code

**Fixes:**
1. Run a clean restore:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```
2. If issues persist, delete `obj` and `bin` directories and rebuild.

## Runtime Issues

### Application Fails to Start

**Symptoms:** Double-clicking the executable produces no window or an immediate crash.

**Fixes:**
1. Ensure the .NET 6 runtime is available (either via the SDK or a standalone installation).
2. On Linux, verify you have the required native libraries:
   ```bash
   # Debian/Ubuntu
   sudo apt install libfontconfig1 libx11-6 libxext6 libxfixes3 libxi6 libxrender1 libxtst6

   # Fedora/RHEL
   sudo dnf install fontconfig libX11 libXext libXfixes libXi libXrender libXtst
   ```
3. On macOS, ensure Gatekeeper hasn't quarantined the app. Right-click and select Open if prompted.
4. Run from the terminal to see error output:
   ```bash
   dotnet run --project src/SecurePort.UI
   ```

### Theme Doesn't Apply Correctly

**Fixes:**
- Switch to a different theme and back in Settings.
- If using `System` theme, ensure your OS has a dark/light mode preference set.
- Reset configuration to defaults if the theme setting is corrupted.

### Scan Hangs or Takes Too Long

**Possible causes:**
- High concurrency with a large port range against a slow target
- Target is unreachable and connections time out on every port

**Fixes:**
- Reduce `DefaultMaxConcurrent` in Settings (try 20-50).
- Increase `DefaultTimeout` only if targeting high-latency networks.
- Scan a smaller port range first (e.g., well-known ports 1-1024).
- Click Cancel to stop a stuck scan.

## Storage and Permissions

### Cannot Write Configuration or Data

**Symptoms:** Settings don't persist, or backup/export operations fail.

**Fixes:**
1. Verify the application data directory is writable:
   ```bash
   # Linux
   ls -la ~/.local/share/SecurePort/

   # Windows - check %LOCALAPPDATA%\SecurePort\
   ```
2. If running from a read-only location (e.g., a mounted USB drive), move the application to a writable location.
3. On Linux, ensure your user owns the data directory, not root.

### Data Directory Consuming Too Much Space

**Fixes:**
- Reduce `HistoryRetentionDays` and `Log.RetentionDays` in Settings.
- Enable `AutoDelete` to automatically purge old entries.
- Use the Privacy Dashboard to delete specific data categories.
- Run manual cleanup through **Settings > Storage > Clear History**.

### Encryption Issues After Enabling Encryption

**Possible causes:**
- Corrupted encrypted data
- System crypto library incompatibility

**Fixes:**
- Disable encryption in Settings. If data is corrupted, you may need to reset.
- On Linux, ensure `libssl` is installed and up to date.
- Back up your data directory before making changes.

## Performance Tips

1. **Reduce concurrency for large scans.** The default of 100 concurrent connections is aggressive for some networks. Lower it if you see many timeout results.
2. **Use well-known port ranges.** Scanning only ports 1-1024 is much faster than scanning all 65,535 ports.
3. **Increase timeout for remote targets.** The default 2-second timeout works for local networks; increase to 5-10 seconds for targets across the internet.
4. **Close other network-heavy applications** during large scans to reduce contention.
5. **Periodically clear old scan history** to keep the application responsive.

## Getting Help

If your issue isn't covered here:

1. **Check existing issues** at the project's GitHub Issues page.
2. **Search closed issues** - your problem may have been resolved in a recent update.
3. **File a new issue** with:
   - Your OS and version
   - .NET SDK version (`dotnet --version`)
   - Steps to reproduce the problem
   - Any error messages from the console output
   - Whether you're running from source or a published build
4. **For security vulnerabilities**, follow the security policy instead of opening a public issue.
