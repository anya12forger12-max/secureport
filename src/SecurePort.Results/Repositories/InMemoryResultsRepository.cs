using System.Collections.Concurrent;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Repositories;

/// <summary>
/// Thread-safe in-memory results repository with dictionary-based indexing.
/// </summary>
public sealed class InMemoryResultsRepository : IResultsRepository
{
    private readonly ConcurrentDictionary<Guid, ResultEntry> _entries = new();
    private readonly ConcurrentDictionary<Guid, List<Guid>> _scanIndex = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public Task SaveAsync(ResultEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ct.ThrowIfCancellationRequested();

        _entries[entry.Id] = entry;
        _scanIndex.AddOrUpdate(
            entry.ScanId,
            _ => new List<Guid> { entry.Id },
            (_, list) =>
            {
                lock (list)
                {
                    if (!list.Contains(entry.Id))
                        list.Add(entry.Id);
                }
                return list;
            });

        return Task.CompletedTask;
    }

    public async Task SaveBatchAsync(IReadOnlyList<ResultEntry> entries, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entries);

        await _lock.WaitAsync(ct);
        try
        {
            foreach (var entry in entries)
            {
                ct.ThrowIfCancellationRequested();
                _entries[entry.Id] = entry;
                _scanIndex.AddOrUpdate(
                    entry.ScanId,
                    _ => new List<Guid> { entry.Id },
                    (_, list) =>
                    {
                        lock (list)
                        {
                            if (!list.Contains(entry.Id))
                                list.Add(entry.Id);
                        }
                        return list;
                    });
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<ResultEntry?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _entries.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<ResultEntry>> GetByScanIdAsync(Guid scanId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_scanIndex.TryGetValue(scanId, out var ids))
            return Task.FromResult<IReadOnlyList<ResultEntry>>(Array.Empty<ResultEntry>());

        List<Guid> snapshot;
        lock (ids)
        {
            snapshot = new List<Guid>(ids);
        }

        var results = snapshot
            .Where(id => _entries.TryGetValue(id, out _))
            .Select(id => _entries[id])
            .OrderBy(r => r.Port)
            .ToList();

        return Task.FromResult<IReadOnlyList<ResultEntry>>(results);
    }

    public Task<IReadOnlyList<ResultEntry>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var all = _entries.Values.OrderBy(r => r.ScanDate).ThenBy(r => r.Port).ToList();
        return Task.FromResult<IReadOnlyList<ResultEntry>>(all);
    }

    public Task UpdateAsync(ResultEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ct.ThrowIfCancellationRequested();

        _entries[entry.Id] = entry;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_entries.TryRemove(id, out var entry))
            return Task.FromResult(false);

        if (_scanIndex.TryGetValue(entry.ScanId, out var ids))
        {
            lock (ids)
            {
                ids.Remove(id);
            }
        }

        return Task.FromResult(true);
    }

    public async Task<int> DeleteByScanIdAsync(Guid scanId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (!_scanIndex.TryRemove(scanId, out var ids))
                return 0;

            int count;
            List<Guid> snapshot;
            lock (ids)
            {
                snapshot = new List<Guid>(ids);
                count = snapshot.Count;
            }

            foreach (var id in snapshot)
            {
                _entries.TryRemove(id, out _);
            }

            return count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<int> CountAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_entries.Count);
    }

    public Task<int> CountByScanIdAsync(Guid scanId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_scanIndex.TryGetValue(scanId, out var ids))
            return Task.FromResult(0);

        lock (ids)
        {
            return Task.FromResult(ids.Count);
        }
    }
}
