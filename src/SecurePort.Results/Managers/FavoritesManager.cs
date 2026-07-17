using System.Collections.Concurrent;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages favorite status for scan result entries using in-memory storage.
/// </summary>
public sealed class FavoritesManager : IFavoritesManager
{
    private readonly ConcurrentDictionary<Guid, bool> _favorites = new();
    private readonly IResultsRepository _repository;

    public FavoritesManager(IResultsRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task ToggleFavoriteAsync(Guid resultId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var current = _favorites.TryGetValue(resultId, out var isFav) && isFav;
        _favorites[resultId] = !current;

        return UpdateRepositoryFavorite(resultId, !current, ct);
    }

    public Task SetFavoriteAsync(Guid resultId, bool isFavorite, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _favorites[resultId] = isFavorite;
        return UpdateRepositoryFavorite(resultId, isFavorite, ct);
    }

    public async Task<IReadOnlyList<ResultEntry>> GetFavoritesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var favoriteIds = _favorites
            .Where(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var results = new List<ResultEntry>();
        foreach (var id in favoriteIds)
        {
            ct.ThrowIfCancellationRequested();
            var entry = await _repository.GetByIdAsync(id, ct);
            if (entry is not null)
            {
                results.Add(entry);
            }
        }

        return results;
    }

    public Task<bool> IsFavoriteAsync(Guid resultId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var isFav = _favorites.TryGetValue(resultId, out var val) && val;
        return Task.FromResult(isFav);
    }

    private async Task UpdateRepositoryFavorite(Guid resultId, bool isFavorite, CancellationToken ct)
    {
        var entry = await _repository.GetByIdAsync(resultId, ct);
        if (entry is null)
            return;

        var updated = entry with { IsFavorite = isFavorite };
        await _repository.UpdateAsync(updated, ct);
    }
}
