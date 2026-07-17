using System.Collections.Concurrent;
using SecurePort.Core.Constants;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Repositories;

/// <summary>
/// Thread-safe LRU-style cache for result entries with configurable capacity.
/// </summary>
public sealed class ResultsCache : IResultsCache
{
    private readonly ConcurrentDictionary<Guid, ResultEntry> _cache = new();
    private readonly int _maxCapacity;

    public int Count => _cache.Count;

    /// <summary>
    /// Initializes a new instance with the specified maximum capacity.
    /// </summary>
    /// <param name="maxCapacity">Maximum number of entries to cache. Default is 10000.</param>
    public ResultsCache(int maxCapacity = AppConstants.DefaultCacheCapacity)
    {
        _maxCapacity = maxCapacity > 0 ? maxCapacity : AppConstants.DefaultCacheCapacity;
    }

    public ResultEntry? GetOrAdd(Guid id, Func<ResultEntry> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (_cache.TryGetValue(id, out var existing))
            return existing;

        if (_cache.Count >= _maxCapacity)
        {
            EvictOldest();
        }

        var entry = factory();
        _cache[entry.Id] = entry;
        return entry;
    }

    public ResultEntry? Get(Guid id)
    {
        _cache.TryGetValue(id, out var entry);
        return entry;
    }

    public void Add(ResultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (_cache.Count >= _maxCapacity)
        {
            EvictOldest();
        }

        _cache[entry.Id] = entry;
    }

    public bool Update(ResultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!_cache.ContainsKey(entry.Id))
            return false;

        _cache[entry.Id] = entry;
        return true;
    }

    public bool Remove(Guid id)
    {
        return _cache.TryRemove(id, out _);
    }

    public int RemoveByScanId(Guid scanId)
    {
        var toRemove = _cache.Values
            .Where(e => e.ScanId == scanId)
            .Select(e => e.Id)
            .ToList();

        int count = 0;
        foreach (var id in toRemove)
        {
            if (_cache.TryRemove(id, out _))
                count++;
        }

        return count;
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public void Preload(IReadOnlyList<ResultEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        foreach (var entry in entries)
        {
            if (_cache.Count >= _maxCapacity)
                break;

            _cache[entry.Id] = entry;
        }
    }

    private void EvictOldest()
    {
        if (_cache.IsEmpty)
            return;

        var oldest = _cache.Values
            .OrderBy(e => e.ScanDate)
            .ThenBy(e => e.Id)
            .FirstOrDefault();

        if (oldest is not null)
        {
            _cache.TryRemove(oldest.Id, out _);
        }
    }
}
