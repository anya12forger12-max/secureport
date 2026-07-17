using System.Collections.Concurrent;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.History.Repositories;

/// <summary>
/// Implements <see cref="IScanHistoryRepository"/> using an <see cref="IStorageProvider{T}"/>
/// for persistence and an in-memory index for thread-safe search and pagination.
/// </summary>
public sealed class ScanHistoryRepository : IScanHistoryRepository
{
    private readonly IStorageProvider<ScanSession> _storage;
    private readonly int _retentionDays;
    private readonly bool _autoDelete;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ConcurrentDictionary<Guid, ScanSession> _index = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanHistoryRepository"/> class.
    /// </summary>
    /// <param name="storage">The storage provider used for persistence.</param>
    /// <param name="retentionDays">The number of days to retain scan history entries.</param>
    /// <param name="autoDelete">When true, old entries are automatically purged on write operations.</param>
    public ScanHistoryRepository(IStorageProvider<ScanSession> storage, int retentionDays, bool autoDelete)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _retentionDays = retentionDays;
        _autoDelete = autoDelete;
    }

    /// <inheritdoc />
    public async Task SaveAsync(ScanSession session, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(session);

        await _lock.WaitAsync(ct);
        try
        {
            await _storage.SetAsync(session.Id.ToString("D"), session, ct);
            _index[session.Id] = session;

            if (_autoDelete)
            {
                await PurgeOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-_retentionDays), ct);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ScanSession?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        if (_index.TryGetValue(id, out var cached))
        {
            return cached;
        }

        var session = await _storage.GetAsync(id.ToString("D"), ct);

        if (session is not null)
        {
            _index[session.Id] = session;
        }

        return session;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScanSession>> GetAllAsync(CancellationToken ct)
    {
        await RebuildIndexIfNeeded(ct);

        var sessions = _index.Values
            .OrderByDescending(s => s.StartTime)
            .ToList();

        return sessions;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScanSession>> GetPageAsync(int offset, int limit, CancellationToken ct)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be non-negative.");
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be positive.");
        }

        await RebuildIndexIfNeeded(ct);

        var sessions = _index.Values
            .OrderByDescending(s => s.StartTime)
            .Skip(offset)
            .Take(limit)
            .ToList();

        return sessions;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var removed = await _storage.RemoveAsync(id.ToString("D"), ct);
            _index.TryRemove(id, out _);
            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken ct)
    {
        await RebuildIndexIfNeeded(ct);
        return _index.Count;
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await _storage.ClearAsync(ct);
            _index.Clear();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<int> PurgeOlderThanAsync(DateTimeOffset olderThan, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var allSessions = await _storage.GetAllAsync(ct);
            var toRemove = allSessions.Where(s => s.StartTime < olderThan).ToList();

            foreach (var session in toRemove)
            {
                await _storage.RemoveAsync(session.Id.ToString("D"), ct);
                _index.TryRemove(session.Id, out _);
            }

            return toRemove.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Searches for scan sessions matching the specified criteria.
    /// </summary>
    /// <param name="host">The hostname or IP to filter by, or null to match all hosts.</param>
    /// <param name="startDate">The earliest scan start time, or null for no lower bound.</param>
    /// <param name="endDate">The latest scan start time, or null for no upper bound.</param>
    /// <param name="status">The scan status to filter by, or null to match all statuses.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of matching scan sessions ordered by start time descending.</returns>
    public async Task<IReadOnlyList<ScanSession>> SearchAsync(
        string? host,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        Core.Enums.ScanStatus? status,
        CancellationToken ct)
    {
        await RebuildIndexIfNeeded(ct);

        IEnumerable<ScanSession> query = _index.Values;

        if (!string.IsNullOrEmpty(host))
        {
            query = query.Where(s =>
                s.Target.Host.Contains(host, StringComparison.OrdinalIgnoreCase));
        }

        if (startDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= endDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        return query
            .OrderByDescending(s => s.StartTime)
            .ToList();
    }

    private async Task RebuildIndexIfNeeded(CancellationToken ct)
    {
        if (_index.IsEmpty)
        {
            var allSessions = await _storage.GetAllAsync(ct);
            foreach (var session in allSessions)
            {
                _index[session.Id] = session;
            }
        }
    }
}
