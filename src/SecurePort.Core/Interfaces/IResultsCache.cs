using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides an in-memory cache for result entries to optimize repeated access.
/// </summary>
public interface IResultsCache
{
    /// <summary>Gets or adds a result entry to the cache.</summary>
    ResultEntry? GetOrAdd(Guid id, Func<ResultEntry> factory);

    /// <summary>Gets a result entry from the cache.</summary>
    ResultEntry? Get(Guid id);

    /// <summary>Adds a result entry to the cache.</summary>
    void Add(ResultEntry entry);

    /// <summary>Updates an existing result entry in the cache.</summary>
    bool Update(ResultEntry entry);

    /// <summary>Removes a result entry from the cache.</summary>
    bool Remove(Guid id);

    /// <summary>Removes all cached entries for a scan session.</summary>
    int RemoveByScanId(Guid scanId);

    /// <summary>Clears the entire cache.</summary>
    void Clear();

    /// <summary>Gets the current number of entries in the cache.</summary>
    int Count { get; }

    /// <summary>Preloads a batch of entries into the cache.</summary>
    void Preload(IReadOnlyList<ResultEntry> entries);
}
