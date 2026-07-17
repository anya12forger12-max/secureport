using System.Collections.Concurrent;
using SecurePort.Core.Interfaces;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages user-defined tags for scan result entries.
/// </summary>
public sealed class TagManager : ITagManager, IDisposable
{
    private readonly ConcurrentBag<string> _allTags = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<string>> _resultTags = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<Guid>> _tagResults = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    public Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var tags = _allTags.Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(tags);
    }

    public async Task<string> CreateTagAsync(string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be null or empty.", nameof(tagName));

        ct.ThrowIfCancellationRequested();

        var normalized = tagName.Trim();

        await _lock.WaitAsync(ct);
        try
        {
            if (!_allTags.Any(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            {
                _allTags.Add(normalized);
            }
        }
        finally
        {
            _lock.Release();
        }

        return normalized;
    }

    public async Task<bool> RenameTagAsync(string oldName, string newName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            return false;

        ct.ThrowIfCancellationRequested();

        await _lock.WaitAsync(ct);
        try
        {
            var normalized = oldName.Trim();
            var newNormalized = newName.Trim();

            if (_allTags.Any(t => t.Equals(newNormalized, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (!_allTags.Any(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                return false;

            var tags = _allTags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var tagList = new ConcurrentBag<string>();
            foreach (var t in tags)
            {
                tagList.Add(t.Equals(normalized, StringComparison.OrdinalIgnoreCase) ? newNormalized : t);
            }

            while (!_allTags.IsEmpty)
                _allTags.TryTake(out _);
            foreach (var t in tagList)
                _allTags.Add(t);

            if (_tagResults.TryRemove(normalized, out var resultIds))
            {
                _tagResults[newNormalized] = resultIds;
            }

            foreach (var kvp in _resultTags)
            {
                var updated = new ConcurrentBag<string>();
                foreach (var t in kvp.Value)
                {
                    updated.Add(t.Equals(normalized, StringComparison.OrdinalIgnoreCase) ? newNormalized : t);
                }
                _resultTags[kvp.Key] = updated;
            }
        }
        finally
        {
            _lock.Release();
        }

        return true;
    }

    public async Task<bool> DeleteTagAsync(string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return false;

        ct.ThrowIfCancellationRequested();

        await _lock.WaitAsync(ct);
        try
        {
            var normalized = tagName.Trim();
            var tags = _allTags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var found = tags.FirstOrDefault(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (found is null)
                return false;

            var tagList = new ConcurrentBag<string>(tags.Where(t => !t.Equals(normalized, StringComparison.OrdinalIgnoreCase)));
            while (!_allTags.IsEmpty)
                _allTags.TryTake(out _);
            foreach (var t in tagList)
                _allTags.Add(t);

            _tagResults.TryRemove(normalized, out _);

            foreach (var kvp in _resultTags)
            {
                var updated = new ConcurrentBag<string>(kvp.Value.Where(t => !t.Equals(normalized, StringComparison.OrdinalIgnoreCase)));
                _resultTags[kvp.Key] = updated;
            }
        }
        finally
        {
            _lock.Release();
        }

        return true;
    }

    public async Task AddTagToResultAsync(Guid resultId, string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            throw new ArgumentException("Tag name cannot be null or empty.", nameof(tagName));

        ct.ThrowIfCancellationRequested();

        var normalized = await CreateTagAsync(tagName, ct);

        _resultTags.AddOrUpdate(
            resultId,
            _ => new ConcurrentBag<string> { normalized },
            (_, bag) =>
            {
                if (!bag.Any(t => t.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                    bag.Add(normalized);
                return bag;
            });

        _tagResults.AddOrUpdate(
            normalized,
            _ => new ConcurrentBag<Guid> { resultId },
            (_, bag) =>
            {
                if (!bag.Contains(resultId))
                    bag.Add(resultId);
                return bag;
            });
    }

    public Task RemoveTagFromResultAsync(Guid resultId, string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return Task.CompletedTask;

        ct.ThrowIfCancellationRequested();

        var normalized = tagName.Trim();

        if (_resultTags.TryGetValue(resultId, out var tags))
        {
            var updated = new ConcurrentBag<string>(tags.Where(t => !t.Equals(normalized, StringComparison.OrdinalIgnoreCase)));
            _resultTags[resultId] = updated;
        }

        if (_tagResults.TryGetValue(normalized, out var resultIds))
        {
            var updated = new ConcurrentBag<Guid>(resultIds.Where(id => id != resultId));
            _tagResults[normalized] = updated;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetTagsForResultAsync(Guid resultId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!_resultTags.TryGetValue(resultId, out var tags))
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        var result = tags.Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(result);
    }

    public Task<IReadOnlyList<Guid>> GetResultIdsByTagAsync(string tagName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tagName))
            return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());

        ct.ThrowIfCancellationRequested();

        var normalized = tagName.Trim();

        if (!_tagResults.TryGetValue(normalized, out var resultIds))
            return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());

        var result = resultIds.Distinct().ToList();
        return Task.FromResult<IReadOnlyList<Guid>>(result);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _lock.Dispose();
            _disposed = true;
        }
    }
}
