using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.Results.Repositories;

namespace SecurePort.Tests;

public class InMemoryResultsRepositoryTests
{
    private readonly InMemoryResultsRepository _repo = new();

    [Fact]
    public async Task SaveAsync_NullEntry_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.SaveAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task SaveAsync_AddsEntry()
    {
        var entry = TestDataFactory.CreateResult(port: 80);
        await _repo.SaveAsync(entry, CancellationToken.None);
        var count = await _repo.CountAsync(CancellationToken.None);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SaveAsync_DuplicateId_UpdatesEntry()
    {
        var id = Guid.NewGuid();
        var entry1 = TestDataFactory.CreateResult(port: 80) with { Id = id };
        var entry2 = TestDataFactory.CreateResult(port: 443) with { Id = id };
        await _repo.SaveAsync(entry1, CancellationToken.None);
        await _repo.SaveAsync(entry2, CancellationToken.None);
        var count = await _repo.CountAsync(CancellationToken.None);
        Assert.Equal(1, count);
        var result = await _repo.GetByIdAsync(id, CancellationToken.None);
        Assert.Equal(443, result!.Port);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByScanIdAsync_ReturnsCorrectEntries()
    {
        var scanId = Guid.NewGuid();
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 80, scanId: scanId), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 443, scanId: scanId), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 22), CancellationToken.None);

        var entries = await _repo.GetByScanIdAsync(scanId, CancellationToken.None);
        Assert.Equal(2, entries.Count);
    }

    [Fact]
    public async Task GetByScanIdAsync_UnknownScanId_ReturnsEmpty()
    {
        var entries = await _repo.GetByScanIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByScanDateThenPort()
    {
        var now = DateTimeOffset.UtcNow;
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 443, scanDate: now), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 22, scanDate: now.AddDays(-1)), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 80, scanDate: now.AddDays(-1)), CancellationToken.None);

        var all = await _repo.GetAllAsync(CancellationToken.None);
        Assert.Equal(3, all.Count);
        Assert.Equal(22, all[0].Port);
        Assert.Equal(80, all[1].Port);
        Assert.Equal(443, all[2].Port);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        var entry = TestDataFactory.CreateResult(port: 80);
        await _repo.SaveAsync(entry, CancellationToken.None);
        var deleted = await _repo.DeleteAsync(entry.Id, CancellationToken.None);
        Assert.True(deleted);
        Assert.Equal(0, await _repo.CountAsync(CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var deleted = await _repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteByScanIdAsync_RemovesAllEntriesForScan()
    {
        var scanId = Guid.NewGuid();
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 80, scanId: scanId), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 443, scanId: scanId), CancellationToken.None);
        await _repo.SaveAsync(TestDataFactory.CreateResult(port: 22), CancellationToken.None);

        var deleted = await _repo.DeleteByScanIdAsync(scanId, CancellationToken.None);
        Assert.Equal(2, deleted);
        Assert.Equal(1, await _repo.CountAsync(CancellationToken.None));
    }

    [Fact]
    public async Task DeleteByScanIdAsync_UnknownScanId_ReturnsZero()
    {
        var deleted = await _repo.DeleteByScanIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.Equal(0, deleted);
    }

    [Fact]
    public async Task SaveBatchAsync_AddsMultipleEntries()
    {
        var entries = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 443),
            TestDataFactory.CreateResult(port: 22)
        };
        await _repo.SaveBatchAsync(entries, CancellationToken.None);
        Assert.Equal(3, await _repo.CountAsync(CancellationToken.None));
    }
}

public class ResultsCacheTests
{
    [Fact]
    public void Constructor_ZeroCapacity_DefaultsTo10000()
    {
        var cache = new ResultsCache(0);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Constructor_NegativeCapacity_DefaultsTo10000()
    {
        var cache = new ResultsCache(-1);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var cache = new ResultsCache(100);
        cache.Add(TestDataFactory.CreateResult());
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Add_AtCapacity_EvictsOldest()
    {
        var cache = new ResultsCache(2);
        var old = TestDataFactory.CreateResult(port: 80, scanDate: DateTimeOffset.UtcNow.AddDays(-10));
        var mid = TestDataFactory.CreateResult(port: 443, scanDate: DateTimeOffset.UtcNow.AddDays(-5));
        var newest = TestDataFactory.CreateResult(port: 22, scanDate: DateTimeOffset.UtcNow);

        cache.Add(old);
        cache.Add(mid);
        Assert.Equal(2, cache.Count);

        cache.Add(newest);
        Assert.Equal(2, cache.Count);
        Assert.Null(cache.Get(old.Id));
        Assert.NotNull(cache.Get(mid.Id));
        Assert.NotNull(cache.Get(newest.Id));
    }

    [Fact]
    public void GetOrAdd_CacheMiss_CallsFactory()
    {
        var cache = new ResultsCache(100);
        var entry = TestDataFactory.CreateResult();
        var result = cache.GetOrAdd(entry.Id, () => entry);
        Assert.NotNull(result);
        Assert.Equal(entry.Id, result.Id);
    }

    [Fact]
    public void GetOrAdd_CacheHit_DoesNotCallFactory()
    {
        var cache = new ResultsCache(100);
        var entry = TestDataFactory.CreateResult();
        cache.Add(entry);

        var factoryCalled = false;
        var result = cache.GetOrAdd(entry.Id, () => { factoryCalled = true; return TestDataFactory.CreateResult(); });
        Assert.NotNull(result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void Update_ExistingEntry_ReturnsTrue()
    {
        var cache = new ResultsCache(100);
        var entry = TestDataFactory.CreateResult(port: 80);
        cache.Add(entry);
        var updated = cache.Update(entry with { Port = 443 });
        Assert.True(updated);
        Assert.Equal(443, cache.Get(entry.Id)!.Port);
    }

    [Fact]
    public void Update_NonExistentEntry_ReturnsFalse()
    {
        var cache = new ResultsCache(100);
        Assert.False(cache.Update(TestDataFactory.CreateResult()));
    }

    [Fact]
    public void Remove_ExistingEntry_ReturnsTrue()
    {
        var cache = new ResultsCache(100);
        var entry = TestDataFactory.CreateResult();
        cache.Add(entry);
        Assert.True(cache.Remove(entry.Id));
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void RemoveByScanId_RemovesMatchingEntries()
    {
        var cache = new ResultsCache(100);
        var scanId = Guid.NewGuid();
        cache.Add(TestDataFactory.CreateResult(scanId: scanId));
        cache.Add(TestDataFactory.CreateResult(scanId: scanId));
        cache.Add(TestDataFactory.CreateResult(scanId: Guid.NewGuid()));

        var removed = cache.RemoveByScanId(scanId);
        Assert.Equal(2, removed);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Clear_ResetsCache()
    {
        var cache = new ResultsCache(100);
        cache.Add(TestDataFactory.CreateResult());
        cache.Add(TestDataFactory.CreateResult());
        cache.Clear();
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Preload_StopsAtCapacity()
    {
        var cache = new ResultsCache(2);
        var entries = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(),
            TestDataFactory.CreateResult(),
            TestDataFactory.CreateResult()
        };
        cache.Preload(entries);
        Assert.Equal(2, cache.Count);
    }
}
