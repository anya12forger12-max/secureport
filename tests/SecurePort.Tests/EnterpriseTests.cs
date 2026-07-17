using System.Text.Json;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;
using SecurePort.Results.Diagnostics;
using SecurePort.Results.Health;
using SecurePort.Results.Recovery;
using SecurePort.Results.Versioning;
using SecurePort.Security.Validation;
using SecurePort.Storage.Providers;

namespace SecurePort.Tests;

public class TargetValidatorTests
{
    private readonly TargetValidator _validator = new();

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("example.com")]
    [InlineData("sub.domain.example.com")]
    public void Validate_ValidTarget_ReturnsValid(string target)
    {
        var result = _validator.Validate(target);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyTarget_ReturnsInvalid(string? target)
    {
        var result = _validator.Validate(target!);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_TARGET_EMPTY", result.ErrorCode!);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("224.0.0.1")]
    public void Validate_ReservedIp_ReturnsInvalid(string ip)
    {
        var result = _validator.Validate(ip);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_TARGET_RESERVED", result.ErrorCode!);
    }

    [Fact]
    public void Validate_ConsecutiveDots_ReturnsInvalid()
    {
        var result = _validator.Validate("192..168.1.1");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_IpWithInvalidOctet_ReturnsInvalid()
    {
        var result = _validator.Validate("999.999.999.999");
        Assert.False(result.IsValid);
        Assert.Contains("ERR_TARGET_IP_OCTET", result.ErrorCode!);
    }

    // ITargetValidator interface methods
    [Fact]
    public void IsValid_ValidTarget_ReturnsTrue()
    {
        var target = new ScanTarget
        {
            Host = "192.168.1.1",
            PortStart = 1,
            PortEnd = 1024,
            Protocol = ProtocolType.TCP,
            ScanType = ScanType.ConnectScan,
            Timeout = TimeSpan.FromSeconds(3),
            MaxConcurrentConnections = 100,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Assert.True(_validator.IsValid(target));
    }

    [Fact]
    public void ValidateHost_InvalidHost_ReturnsErrorMessage()
    {
        var error = _validator.ValidateHost("");
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidateHost_ValidHost_ReturnsNull()
    {
        var error = _validator.ValidateHost("192.168.1.1");
        Assert.Null(error);
    }

    [Fact]
    public void ValidatePortRange_InvalidRange_ReturnsErrorMessage()
    {
        var error = _validator.ValidatePortRange(100, 50);
        Assert.NotNull(error);
    }

    [Fact]
    public void ValidatePortRange_ValidRange_ReturnsNull()
    {
        var error = _validator.ValidatePortRange(1, 1024);
        Assert.Null(error);
    }
}

public class PortValidatorTests
{
    private readonly PortValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(65535)]
    public void ValidatePort_ValidPort_ReturnsValid(int port)
    {
        var result = _validator.ValidatePort(port);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void ValidatePort_InvalidPort_ReturnsInvalid(int port)
    {
        var result = _validator.ValidatePort(port);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidatePortRange_ValidRange_ReturnsValid()
    {
        var result = _validator.ValidatePortRange(1, 1024);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidatePortRange_StartGreaterThanEnd_ReturnsInvalid()
    {
        var result = _validator.ValidatePortRange(1024, 1);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_PORT_RANGE_ORDER", result.ErrorCode!);
    }

    [Fact]
    public void Validate_PortString_ReturnsValid()
    {
        var result = _validator.Validate("80");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PortRangeString_ReturnsValid()
    {
        var result = _validator.Validate("1-1024");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_InvalidFormat_ReturnsInvalid()
    {
        var result = _validator.Validate("abc");
        Assert.False(result.IsValid);
    }
}

public class ProfileValidatorTests
{
    private readonly ProfileValidator _validator = new();

    [Theory]
    [InlineData("fast-scan")]
    [InlineData("full_audit")]
    [InlineData("scan.v2")]
    public void Validate_ValidProfile_ReturnsValid(string profile)
    {
        var result = _validator.Validate(profile);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyProfile_ReturnsInvalid(string? profile)
    {
        var result = _validator.Validate(profile!);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("admin")]
    [InlineData("root")]
    public void Validate_ReservedName_ReturnsInvalid(string name)
    {
        var result = _validator.Validate(name);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_PROFILE_RESERVED", result.ErrorCode!);
    }

    [Fact]
    public void Validate_InvalidChars_ReturnsInvalid()
    {
        var result = _validator.Validate("profile with spaces");
        Assert.False(result.IsValid);
    }
}

public class ReportValidatorTests
{
    private readonly ReportValidator _validator = new();

    [Fact]
    public void Validate_ValidReport_ReturnsValid()
    {
        var report = TestDataFactory.CreateReportMetadata();
        var result = _validator.Validate(report);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyTitle_ReturnsInvalid()
    {
        var report = TestDataFactory.CreateReportMetadata(title: "");
        var result = _validator.Validate(report);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_REPORT_TITLE", result.ErrorCode!);
    }

    [Fact]
    public void Validate_EmptyTarget_ReturnsInvalid()
    {
        var report = TestDataFactory.CreateReportMetadata() with { TargetHost = "" };
        var result = _validator.Validate(report);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }
}

public class BackupValidatorTests
{
    private readonly BackupValidator _validator = new();

    [Fact]
    public void Validate_ValidBackup_ReturnsValid()
    {
        var backup = new BackupMetadata
        {
            BackupId = Guid.NewGuid(),
            Name = "Test Backup",
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = "1.0.0",
            SizeBytes = 1024,
            Checksum = "abc123",
            FilePath = "/tmp/backup.zip",
            Type = BackupType.Full,
            Status = BackupStatus.Valid,
            IncludedCategories = new[] { "history", "reports" }
        };
        var result = _validator.Validate(backup);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsInvalid()
    {
        var backup = new BackupMetadata
        {
            BackupId = Guid.NewGuid(),
            Name = "",
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = "1.0.0",
            SizeBytes = 1024,
            Checksum = "abc",
            FilePath = "/tmp/backup.zip",
            Type = BackupType.Full,
            Status = BackupStatus.Valid,
            IncludedCategories = new[] { "history" }
        };
        var result = _validator.Validate(backup);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_CorruptedStatus_ReturnsInvalid()
    {
        var backup = new BackupMetadata
        {
            BackupId = Guid.NewGuid(),
            Name = "Test",
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = "1.0.0",
            SizeBytes = 1024,
            Checksum = "abc",
            FilePath = "/tmp/backup.zip",
            Type = BackupType.Full,
            Status = BackupStatus.Corrupted,
            IncludedCategories = new[] { "history" }
        };
        var result = _validator.Validate(backup);
        Assert.False(result.IsValid);
        Assert.Contains("ERR_BACKUP_CORRUPTED", result.ErrorCode!);
    }
}

public class ImportValidatorTests
{
    private readonly ImportValidator _validator = new();

    [Fact]
    public void Validate_EmptyPath_ReturnsInvalid()
    {
        var result = _validator.Validate("");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NonExistentFile_ReturnsInvalid()
    {
        var result = _validator.Validate("/tmp/nonexistent_file_12345.json");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ExistingTempFile_ReturnsValid()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_import_{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(tempFile, "{\"test\": true}");
            var result = _validator.Validate(tempFile);
            Assert.True(result.IsValid);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}

public class ExportValidatorTests
{
    private readonly ExportValidator _validator = new();

    [Theory]
    [InlineData("/tmp/report.json")]
    [InlineData("/tmp/export.csv")]
    public void ValidPaths_ReturnValid(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var result = _validator.Validate(path);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyPath_ReturnsInvalid()
    {
        var result = _validator.Validate("");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_PathTraversal_ReturnsInvalid()
    {
        var result = _validator.Validate("/tmp/../../etc/passwd");
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("/boot/grub")]
    public void Validate_BlockedPath_ReturnsInvalid(string path)
    {
        var result = _validator.Validate(path);
        Assert.False(result.IsValid);
    }
}

public class ConfigurationValidatorTests
{
    private readonly ConfigurationValidator _validator = new();

    [Fact]
    public void ValidateConfiguration_ValidConfig_ReturnsValid()
    {
        var config = new AppConfiguration
        {
            Scan = new ScanSettings
            {
                DefaultTimeout = TimeSpan.FromSeconds(3),
                DefaultMaxConcurrent = 100,
                DefaultPortRangeStart = 1,
                DefaultPortRangeEnd = 1024
            },
            UI = new UISettings
            {
                Theme = ThemeMode.Dark,
                Language = Language.English,
                FontSize = 14,
                HighContrast = false,
                ReducedMotion = false,
                ScreenReaderMode = false
            },
            Storage = new StorageSettings
            {
                HistoryRetentionDays = 90,
                AutoDelete = true,
                EncryptionEnabled = false
            },
            Log = new LogSettings
            {
                Level = LogLevel.Info,
                RetentionDays = 30
            }
        };
        var result = _validator.ValidateConfiguration(config);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateConfiguration_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.ValidateConfiguration(null!));
    }

    [Fact]
    public void ValidateConfigurationFile_NonExistent_ReturnsInvalid()
    {
        var result = _validator.ValidateConfigurationFile("/tmp/nonexistent.json");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateConfigurationFile_EmptyPath_ReturnsInvalid()
    {
        var result = _validator.ValidateConfigurationFile("");
        Assert.False(result.IsValid);
    }
}

public class VersionInfoTests
{
    [Fact]
    public void SemVer_NoPreRelease_ReturnsCoreVersion()
    {
        var version = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
        Assert.Equal("1.2.3", version.SemVer);
    }

    [Fact]
    public void SemVer_WithPreRelease_IncludesTag()
    {
        var version = new VersionInfo { Major = 1, Minor = 0, Patch = 0, PreReleaseTag = "alpha.1" };
        Assert.Equal("1.0.0-alpha.1", version.SemVer);
    }

    [Fact]
    public void IsNewerThan_ReturnsCorrectly()
    {
        var v1 = new VersionInfo { Major = 1, Minor = 0, Patch = 0 };
        var v2 = new VersionInfo { Major = 1, Minor = 1, Patch = 0 };
        Assert.True(v2.IsNewerThan(v1));
        Assert.False(v1.IsNewerThan(v2));
    }

    [Fact]
    public void GetUpdateType_ReturnsCorrectType()
    {
        var v1 = new VersionInfo { Major = 1, Minor = 0, Patch = 0 };
        var major = new VersionInfo { Major = 2, Minor = 0, Patch = 0 };
        var minor = new VersionInfo { Major = 1, Minor = 1, Patch = 0 };
        var patch = new VersionInfo { Major = 1, Minor = 0, Patch = 1 };

        Assert.Equal(UpdateType.Major, major.GetUpdateType(v1));
        Assert.Equal(UpdateType.Minor, minor.GetUpdateType(v1));
        Assert.Equal(UpdateType.Patch, patch.GetUpdateType(v1));
        Assert.Equal(UpdateType.None, v1.GetUpdateType(v1));
    }
}

public class HealthMonitorTests : IDisposable
{
    private readonly string _dataDir;
    private readonly HealthMonitor _monitor;

    public HealthMonitorTests()
    {
        _dataDir = Path.Combine(Path.GetTempPath(), $"secureport_test_health_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dataDir);
        _monitor = new HealthMonitor();
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDir))
            Directory.Delete(_dataDir, true);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsOverallStatus()
    {
        var report = await _monitor.CheckHealthAsync(CancellationToken.None);
        Assert.NotNull(report);
        Assert.True(report.Subsystems.Count > 0);
        Assert.Contains(report.Subsystems, s => s.SubsystemName == "Memory");
        Assert.Contains(report.Subsystems, s => s.SubsystemName == "ThreadPool");
    }

    [Fact]
    public async Task CheckSubsystemAsync_UnknownSubsystem_ReturnsUnknown()
    {
        var health = await _monitor.CheckSubsystemAsync("nonexistent", CancellationToken.None);
        Assert.Equal(HealthLevel.Unknown, health.Status);
    }

    [Fact]
    public void GetRegisteredSubsystems_ReturnsNonEmpty()
    {
        var subsystems = _monitor.GetRegisteredSubsystems();
        Assert.True(subsystems.Count > 0);
    }
}

public class DiagnosticsServiceTests
{
    private readonly DiagnosticsService _service = new();

    [Fact]
    public void GetVersionInfo_ReturnsValidVersion()
    {
        var version = _service.GetVersionInfo();
        Assert.True(version.Major >= 0);
        Assert.True(version.Minor >= 0);
        Assert.True(version.Patch >= 0);
    }

    [Fact]
    public void GetMemoryUsage_ReturnsNonNegativeValues()
    {
        var usage = _service.GetMemoryUsage();
        Assert.True(usage.WorkingSetBytes > 0);
        Assert.True(usage.GcTotalMemoryBytes >= 0);
    }

    [Fact]
    public void GetThreadPoolInfo_ReturnsValidInfo()
    {
        var info = _service.GetThreadPoolInfo();
        Assert.True(info.MaxWorkerThreads > 0);
        Assert.True(info.AvailableWorkerThreads >= 0);
    }

    [Fact]
    public async Task CollectDiagnosticsAsync_ReturnsCompleteSnapshot()
    {
        var diagnostics = await _service.CollectDiagnosticsAsync(CancellationToken.None);
        Assert.NotNull(diagnostics.ApplicationVersion);
        Assert.NotNull(diagnostics.OperatingSystem);
        Assert.NotNull(diagnostics.RuntimeVersion);
        Assert.NotNull(diagnostics.Architecture);
        Assert.True(diagnostics.GcTotalMemoryBytes >= 0);
    }
}

public class VersionServiceTests
{
    private readonly VersionService _service = new();

    [Fact]
    public void GetCurrentVersion_ReturnsValidVersion()
    {
        var version = _service.GetCurrentVersion();
        Assert.True(version.Major >= 1);
    }

    [Fact]
    public void GetKnownSchemaVersions_ContainsCurrentVersion()
    {
        var versions = _service.GetKnownSchemaVersions();
        Assert.Contains(_service.GetCurrentSchemaVersion(), versions);
    }

    [Fact]
    public void NeedsMigration_LowerVersion_ReturnsTrue()
    {
        Assert.True(_service.NeedsMigration(0));
    }

    [Fact]
    public void NeedsMigration_CurrentVersion_ReturnsFalse()
    {
        Assert.False(_service.NeedsMigration(_service.GetCurrentSchemaVersion()));
    }
}

public class SecureFileOperationsTests : IDisposable
{
    private readonly string _testDir;
    private readonly SecureFileOperations _ops = new();

    public SecureFileOperationsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"secureport_test_sfo_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public async Task SafeWriteAllTextAsync_WritesAndReadsBack()
    {
        var path = Path.Combine(_testDir, "test.txt");
        await _ops.SafeWriteAllTextAsync(path, "hello world", CancellationToken.None);
        var content = await _ops.SafeReadAllTextAsync(path, CancellationToken.None);
        Assert.Equal("hello world", content);
    }

    [Fact]
    public async Task SafeReadAllTextAsync_NonExistentFile_ReturnsNull()
    {
        var content = await _ops.SafeReadAllTextAsync("/tmp/nonexistent_12345.txt", CancellationToken.None);
        Assert.Null(content);
    }

    [Fact]
    public async Task SafeDeleteAsync_RemovesFile()
    {
        var path = Path.Combine(_testDir, "to_delete.txt");
        await File.WriteAllTextAsync(path, "data");
        await _ops.SecureDeleteAsync(path, CancellationToken.None);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void IsPathSafe_NormalPath_ReturnsTrue()
    {
        Assert.True(_ops.IsPathSafe("/tmp/test.json"));
    }

    [Fact]
    public void IsPathSafe_PathTraversal_ReturnsFalse()
    {
        Assert.False(_ops.IsPathSafe("/tmp/../../etc/passwd"));
    }

    [Fact]
    public void IsPathSafe_BlockedPath_ReturnsFalse()
    {
        Assert.False(_ops.IsPathSafe("/etc/passwd"));
    }

    [Fact]
    public void GetTempFilePath_ReturnsUniquePaths()
    {
        var path1 = _ops.GetTempFilePath("/tmp/test.json");
        var path2 = _ops.GetTempFilePath("/tmp/test.json");
        Assert.NotEqual(path1, path2);
    }
}

public class FailureRecoveryTests : IDisposable
{
    private readonly string _dataDir;
    private readonly FailureRecovery _recovery;

    public FailureRecoveryTests()
    {
        _dataDir = Path.Combine(Path.GetTempPath(), $"secureport_test_recovery_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dataDir);
        _recovery = new FailureRecovery(_dataDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDir))
            Directory.Delete(_dataDir, true);
    }

    [Fact]
    public async Task RecoverConfigurationAsync_NoConfig_ReturnsSuccess()
    {
        var result = await _recovery.RecoverConfigurationAsync(CancellationToken.None);
        Assert.True(result.Success);
        Assert.Equal("Configuration", result.Component);
    }

    [Fact]
    public async Task RecoverHistoryAsync_NoHistoryDir_CreatesAndSucceeds()
    {
        var result = await _recovery.RecoverHistoryAsync(CancellationToken.None);
        Assert.True(result.Success);
        Assert.True(Directory.Exists(Path.Combine(_dataDir, "history")));
    }

    [Fact]
    public async Task RecoverInterruptedReportAsync_NoReportsDir_ReturnsSuccess()
    {
        var result = await _recovery.RecoverInterruptedReportAsync(CancellationToken.None);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task RunFullRecoveryAsync_ReturnsSummaryWithResults()
    {
        var summary = await _recovery.RunFullRecoveryAsync(CancellationToken.None);
        Assert.True(summary.Results.Count >= 3);
    }

    [Fact]
    public async Task CreateRecoveryBackupAsync_CreatesBackupDirectory()
    {
        var backupPath = await _recovery.CreateRecoveryBackupAsync(CancellationToken.None);
        Assert.True(Directory.Exists(backupPath));
    }
}
