using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;
using SecurePort.Results.Managers;
using SecurePort.Results.Repositories;

namespace SecurePort.Tests;

public class FavoritesManagerTests
{
    private readonly InMemoryResultsRepository _repo = new();
    private readonly FavoritesManager _manager;

    public FavoritesManagerTests()
    {
        _manager = new FavoritesManager(_repo);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_TogglesState()
    {
        var entry = TestDataFactory.CreateResult(isFavorite: false);
        await _repo.SaveAsync(entry, CancellationToken.None);

        await _manager.ToggleFavoriteAsync(entry.Id, CancellationToken.None);
        Assert.True(await _manager.IsFavoriteAsync(entry.Id, CancellationToken.None));

        await _manager.ToggleFavoriteAsync(entry.Id, CancellationToken.None);
        Assert.False(await _manager.IsFavoriteAsync(entry.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SetFavoriteAsync_SetsExplicitState()
    {
        var entry = TestDataFactory.CreateResult(isFavorite: false);
        await _repo.SaveAsync(entry, CancellationToken.None);

        await _manager.SetFavoriteAsync(entry.Id, true, CancellationToken.None);
        Assert.True(await _manager.IsFavoriteAsync(entry.Id, CancellationToken.None));

        await _manager.SetFavoriteAsync(entry.Id, true, CancellationToken.None);
        Assert.True(await _manager.IsFavoriteAsync(entry.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetFavoritesAsync_ReturnsOnlyFavorited()
    {
        var entry1 = TestDataFactory.CreateResult();
        var entry2 = TestDataFactory.CreateResult();
        await _repo.SaveAsync(entry1, CancellationToken.None);
        await _repo.SaveAsync(entry2, CancellationToken.None);

        await _manager.SetFavoriteAsync(entry1.Id, true, CancellationToken.None);

        var favorites = await _manager.GetFavoritesAsync(CancellationToken.None);
        Assert.Single(favorites);
        Assert.Equal(entry1.Id, favorites[0].Id);
    }

    [Fact]
    public async Task IsFavoriteAsync_UnknownId_ReturnsFalse()
    {
        Assert.False(await _manager.IsFavoriteAsync(Guid.NewGuid(), CancellationToken.None));
    }
}

public class TagManagerTests
{
    private readonly TagManager _manager = new();

    [Fact]
    public async Task CreateTagAsync_AddsTag()
    {
        var tag = await _manager.CreateTagAsync("important", CancellationToken.None);
        Assert.Equal("important", tag);
        var tags = await _manager.GetAllTagsAsync(CancellationToken.None);
        Assert.Contains("important", tags);
    }

    [Fact]
    public async Task CreateTagAsync_DuplicateIdempotent()
    {
        await _manager.CreateTagAsync("important", CancellationToken.None);
        await _manager.CreateTagAsync("important", CancellationToken.None);
        var tags = await _manager.GetAllTagsAsync(CancellationToken.None);
        Assert.Single(tags);
    }

    [Fact]
    public async Task CreateTagAsync_WhitespaceThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _manager.CreateTagAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task RenameTagAsync_RenamesEverywhere()
    {
        var resultId = Guid.NewGuid();
        await _manager.CreateTagAsync("old", CancellationToken.None);
        await _manager.AddTagToResultAsync(resultId, "old", CancellationToken.None);

        var renamed = await _manager.RenameTagAsync("old", "new", CancellationToken.None);
        Assert.True(renamed);

        var tags = await _manager.GetTagsForResultAsync(resultId, CancellationToken.None);
        Assert.Contains("new", tags);
        Assert.DoesNotContain("old", tags);
    }

    [Fact]
    public async Task RenameTagAsync_OldNotExist_ReturnsFalse()
    {
        Assert.False(await _manager.RenameTagAsync("nonexistent", "new", CancellationToken.None));
    }

    [Fact]
    public async Task RenameTagAsync_NewAlreadyExists_ReturnsFalse()
    {
        await _manager.CreateTagAsync("old", CancellationToken.None);
        await _manager.CreateTagAsync("new", CancellationToken.None);
        Assert.False(await _manager.RenameTagAsync("old", "new", CancellationToken.None));
    }

    [Fact]
    public async Task DeleteTagAsync_RemovesTagEverywhere()
    {
        var resultId = Guid.NewGuid();
        await _manager.CreateTagAsync("todelete", CancellationToken.None);
        await _manager.AddTagToResultAsync(resultId, "todelete", CancellationToken.None);

        Assert.True(await _manager.DeleteTagAsync("todelete", CancellationToken.None));
        var tags = await _manager.GetAllTagsAsync(CancellationToken.None);
        Assert.DoesNotContain("todelete", tags);
    }

    [Fact]
    public async Task DeleteTagAsync_NotFound_ReturnsFalse()
    {
        Assert.False(await _manager.DeleteTagAsync("nonexistent", CancellationToken.None));
    }

    [Fact]
    public async Task AddTagToResultAsync_CreatesTagIfNotExists()
    {
        var resultId = Guid.NewGuid();
        await _manager.AddTagToResultAsync(resultId, "newtag", CancellationToken.None);
        var tags = await _manager.GetAllTagsAsync(CancellationToken.None);
        Assert.Contains("newtag", tags);
    }

    [Fact]
    public async Task AddTagToResultAsync_PreventsDuplicates()
    {
        var resultId = Guid.NewGuid();
        await _manager.AddTagToResultAsync(resultId, "tag", CancellationToken.None);
        await _manager.AddTagToResultAsync(resultId, "tag", CancellationToken.None);
        var tags = await _manager.GetTagsForResultAsync(resultId, CancellationToken.None);
        Assert.Single(tags);
    }

    [Fact]
    public async Task GetResultIdsByTagAsync_ReturnsCorrectIds()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await _manager.AddTagToResultAsync(id1, "shared", CancellationToken.None);
        await _manager.AddTagToResultAsync(id2, "shared", CancellationToken.None);

        var ids = await _manager.GetResultIdsByTagAsync("shared", CancellationToken.None);
        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public async Task GetResultIdsByTagAsync_Whitespace_ReturnsEmpty()
    {
        Assert.Empty(await _manager.GetResultIdsByTagAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsSortedDistinct()
    {
        await _manager.CreateTagAsync("zebra", CancellationToken.None);
        await _manager.CreateTagAsync("apple", CancellationToken.None);
        await _manager.CreateTagAsync("apple", CancellationToken.None);
        var tags = await _manager.GetAllTagsAsync(CancellationToken.None);
        Assert.Equal(new[] { "apple", "zebra" }, tags);
    }
}

public class NotesManagerTests
{
    private readonly InMemoryResultsRepository _repo = new();
    private readonly NotesManager _manager;

    public NotesManagerTests()
    {
        _manager = new NotesManager(_repo);
    }

    [Fact]
    public async Task SaveNotesAsync_NullNotes_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.SaveNotesAsync(Guid.NewGuid(), null!, CancellationToken.None));
    }

    [Fact]
    public async Task SaveNotesAsync_StoresAndSyncsToRepo()
    {
        var entry = TestDataFactory.CreateResult(notes: null);
        await _repo.SaveAsync(entry, CancellationToken.None);

        await _manager.SaveNotesAsync(entry.Id, "Test notes", CancellationToken.None);
        var notes = await _manager.GetNotesAsync(entry.Id, CancellationToken.None);
        Assert.Equal("Test notes", notes);

        var repoEntry = await _repo.GetByIdAsync(entry.Id, CancellationToken.None);
        Assert.Equal("Test notes", repoEntry!.Notes);
    }

    [Fact]
    public async Task ClearNotesAsync_RemovesNotes()
    {
        var entry = TestDataFactory.CreateResult();
        await _repo.SaveAsync(entry, CancellationToken.None);
        await _manager.SaveNotesAsync(entry.Id, "Some notes", CancellationToken.None);

        await _manager.ClearNotesAsync(entry.Id, CancellationToken.None);
        var notes = await _manager.GetNotesAsync(entry.Id, CancellationToken.None);
        Assert.Null(notes);
    }

    [Fact]
    public async Task GetNotesAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _manager.GetNotesAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task SearchNotesAsync_CaseInsensitiveSubstring()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 80) with { Id = id1 }, CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 443) with { Id = id2 }, CancellationToken.None);
        await _manager.SaveNotesAsync(id1, "Open port HTTP", CancellationToken.None);
        await _manager.SaveNotesAsync(id2, "Secure HTTPS", CancellationToken.None);

        var found = await _manager.SearchNotesAsync("http", CancellationToken.None);
        Assert.Contains(id1, found);
    }

    [Fact]
    public async Task SearchNotesAsync_Whitespace_ReturnsEmpty()
    {
        Assert.Empty(await _manager.SearchNotesAsync("   ", CancellationToken.None));
    }
}

public class ReportVerificationManagerTests
{
    private readonly ReportVerificationManager _manager = new();

    [Fact]
    public void ComputeChecksum_NullData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _manager.ComputeChecksum(null!));
    }

    [Fact]
    public void ComputeChecksum_Deterministic()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("test data");
        var hash1 = _manager.ComputeChecksum(data);
        var hash2 = _manager.ComputeChecksum(data);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeChecksum_Returns64CharHex()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("hello");
        var hash = _manager.ComputeChecksum(data);
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public async Task ComputeFileChecksumAsync_NullPath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _manager.ComputeFileChecksumAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ComputeFileChecksumAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _manager.ComputeFileChecksumAsync("/tmp/nonexistent_file.txt", CancellationToken.None));
    }

    [Fact]
    public async Task VerifyFileAsync_MatchingChecksum_ReturnsValid()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, System.Text.Encoding.UTF8.GetBytes("test"));
            var checksum = await _manager.ComputeFileChecksumAsync(tempFile, CancellationToken.None);
            var result = await _manager.VerifyFileAsync(tempFile, checksum, CancellationToken.None);
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task VerifyFileAsync_MismatchedChecksum_ReturnsInvalid()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, System.Text.Encoding.UTF8.GetBytes("test"));
            var result = await _manager.VerifyFileAsync(tempFile, "wrongchecksum", CancellationToken.None);
            Assert.False(result.IsValid);
            Assert.Contains("mismatch", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task VerifyFileAsync_NonExistentFile_ReturnsInvalid()
    {
        var result = await _manager.VerifyFileAsync("/tmp/nonexistent_file.txt", "abc", CancellationToken.None);
        Assert.False(result.IsValid);
        Assert.Contains("not found", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyReportAsync_DelegatesToVerifyFile()
    {
        var report = new ReportMetadata
        {
            ReportId = Guid.NewGuid(),
            Title = "Test",
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = "1.0",
            OperatingSystem = "Linux",
            Format = ExportFormat.JSON,
            FileSizeBytes = 0,
            Checksum = "abc",
            ScanId = Guid.NewGuid(),
            TargetHost = "localhost",
            FilePath = "/tmp/nonexistent.json",
            IsArchived = false,
            Tags = new List<string>()
        };
        var result = await _manager.VerifyReportAsync(report, CancellationToken.None);
        Assert.False(result.IsValid);
    }
}

public class LogManagerTests : IDisposable
{
    private readonly string _logDir;
    private readonly LogManager _manager;

    public LogManagerTests()
    {
        _logDir = Path.Combine(Path.GetTempPath(), $"secureport_test_logs_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_logDir);
        _manager = new LogManager(_logDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_logDir))
            Directory.Delete(_logDir, true);
    }

    private void WriteLogFile(string fileName, params string[] lines)
    {
        File.WriteAllLines(Path.Combine(_logDir, fileName), lines);
    }

    [Fact]
    public async Task GetLogsAsync_ReturnsParsedEntries()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var content = "[2024-01-15T10:00:00+00:00] [Info] [Scanner] Starting scan\n[2024-01-15T10:00:01+00:00] [Error] [Scanner] Connection failed";
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), content);
        var logs = await _manager.GetLogsAsync(null, null, CancellationToken.None);
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public async Task SearchLogsAsync_CaseInsensitiveMatch()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var content = "[2024-01-15T10:00:00+00:00] [Info] [Scanner] Starting scan";
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), content);
        var found = await _manager.SearchLogsAsync("scanner", null, null, null, CancellationToken.None);
        Assert.Single(found);
    }

    [Fact]
    public async Task SearchLogsAsync_MinLevelFilter()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var content = "[2024-01-15T10:00:00+00:00] [Info] [Scanner] Info message\n[2024-01-15T10:00:01+00:00] [Error] [Scanner] Error message\n[2024-01-15T10:00:02+00:00] [Debug] [Scanner] Debug message";
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), content);
        var found = await _manager.SearchLogsAsync(null, LogLevel.Warning, null, null, CancellationToken.None);
        Assert.Single(found);
        Assert.Equal("Error message", found[0].Message);
    }

    [Fact]
    public async Task ExportLogsAsync_WritesFile()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        var content = "[2024-01-15T10:00:00+00:00] [Info] [Scanner] Test message";
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), content);
        var exportPath = Path.GetTempFileName();
        try
        {
            var result = await _manager.ExportLogsAsync(exportPath, null, null, CancellationToken.None);
            Assert.True(File.Exists(exportPath));
            var exportedContent = await File.ReadAllTextAsync(exportPath);
            Assert.Contains("Test message", exportedContent);
        }
        finally
        {
            File.Delete(exportPath);
        }
    }

    [Fact]
    public async Task DeleteAllLogsAsync_RemovesAllLogFiles()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), "line1\nline2");
        File.WriteAllText(Path.Combine(_logDir, "log-2024-01-01.txt"), "old log");

        var deleted = await _manager.DeleteAllLogsAsync(CancellationToken.None);
        Assert.Equal(2, deleted);
        Assert.Empty(Directory.GetFiles(_logDir, "log-*.txt"));
    }

    [Fact]
    public async Task GetLogSizeAsync_ReturnsTotalBytes()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        File.WriteAllText(Path.Combine(_logDir, $"log-{date}.txt"), "some log content here");
        var size = await _manager.GetLogSizeAsync(CancellationToken.None);
        Assert.True(size > 0);
    }

    [Fact]
    public async Task DeleteOlderThanAsync_RemovesOldFiles()
    {
        File.WriteAllText(Path.Combine(_logDir, "log-2020-01-01.txt"), "very old");
        File.WriteAllText(Path.Combine(_logDir, $"log-{DateTimeOffset.UtcNow:yyyy-MM-dd}.txt"), "current");

        var deleted = await _manager.DeleteOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None);
        Assert.Equal(1, deleted);
        Assert.Single(Directory.GetFiles(_logDir, "log-*.txt"));
    }
}

public class StorageMonitorTests : IDisposable
{
    private readonly string _dataDir;
    private readonly StorageMonitor _monitor;

    public StorageMonitorTests()
    {
        _dataDir = Path.Combine(Path.GetTempPath(), $"secureport_test_storage_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_dataDir);
        _monitor = new StorageMonitor(null, null, null, _dataDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDir))
            Directory.Delete(_dataDir, true);
    }

    [Fact]
    public void Constructor_NullDataDir_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new StorageMonitor(null, null, null, null!));
    }

    [Fact]
    public async Task GetStorageUsageAsync_ReturnsNonNegativeSizes()
    {
        var usage = await _monitor.GetStorageUsageAsync(CancellationToken.None);
        Assert.True(usage.TotalSizeBytes >= 0);
        Assert.True(usage.HistorySizeBytes >= 0);
        Assert.True(usage.ReportsSizeBytes >= 0);
        Assert.True(usage.BackupsSizeBytes >= 0);
    }

    [Fact]
    public async Task GetPrivacyStatusAsync_ReturnsValidStatus()
    {
        var status = await _monitor.GetPrivacyStatusAsync(CancellationToken.None);
        Assert.True(status.OfflineMode);
        Assert.False(status.TelemetryDisabled == false);
    }

    [Fact]
    public async Task SecureDeleteAsync_HistoryCategory_ReturnsTrue()
    {
        var result = await _monitor.SecureDeleteAsync("history", null, CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task SecureDeleteAsync_LogsCategory_DeletesLogFiles()
    {
        var logDir = Path.Combine(_dataDir, "logs");
        Directory.CreateDirectory(logDir);
        File.WriteAllText(Path.Combine(logDir, "log-2024-01-01.txt"), "test log");

        var result = await _monitor.SecureDeleteAsync("logs", null, CancellationToken.None);
        Assert.True(result);
        Assert.Empty(Directory.GetFiles(logDir, "log-*.txt"));
    }

    [Fact]
    public async Task SecureDeleteAsync_UnknownCategory_ReturnsFalse()
    {
        var result = await _monitor.SecureDeleteAsync("unknown", null, CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task ClearHistoryAsync_NoRepository_ReturnsZero()
    {
        var count = await _monitor.ClearHistoryAsync(CancellationToken.None);
        Assert.Equal(0, count);
    }
}

public class DataRetentionManagerTests : IDisposable
{
    private readonly string _configDir;
    private readonly DataRetentionManager _manager;

    public DataRetentionManagerTests()
    {
        _configDir = Path.Combine(Path.GetTempPath(), $"secureport_test_retention_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_configDir);
        _manager = new DataRetentionManager(_configDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, true);
    }

    [Fact]
    public async Task GetConfigurationAsync_Defaults_WhenNoFileExists()
    {
        var config = await _manager.GetConfigurationAsync(CancellationToken.None);
        Assert.Equal(10000, config.MaxHistoryEntries);
        Assert.True(config.AutoCleanupEnabled);
        Assert.Equal(90, config.RetentionDays);
    }

    [Fact]
    public async Task SaveConfigurationAsync_PersistsConfig()
    {
        var config = new DataRetentionConfig
        {
            MaxHistoryEntries = 500,
            AutoCleanupEnabled = false,
            RetentionDays = 30,
            ArchiveThresholdDays = 60,
            MaxStorageBytes = 1024 * 1024 * 100
        };
        await _manager.SaveConfigurationAsync(config, CancellationToken.None);
        var loaded = await _manager.GetConfigurationAsync(CancellationToken.None);
        Assert.Equal(500, loaded.MaxHistoryEntries);
        Assert.False(loaded.AutoCleanupEnabled);
        Assert.Equal(30, loaded.RetentionDays);
    }

    [Fact]
    public async Task PreviewCleanupAsync_ReturnsPreviewResult()
    {
        var result = await _manager.PreviewCleanupAsync(CancellationToken.None);
        Assert.True(result.IsPreview);
        Assert.True(result.ExecutedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task ExecuteCleanupAsync_WhenDisabled_ReturnsNoop()
    {
        var config = new DataRetentionConfig
        {
            MaxHistoryEntries = 10000,
            AutoCleanupEnabled = false,
            RetentionDays = 90,
            ArchiveThresholdDays = 180,
            MaxStorageBytes = 0
        };
        await _manager.SaveConfigurationAsync(config, CancellationToken.None);
        var result = await _manager.ExecuteCleanupAsync(CancellationToken.None);
        Assert.Equal(0, result.HistoryEntriesRemoved);
        Assert.Equal(0, result.ReportsRemoved);
    }
}
