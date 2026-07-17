using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages favorite scans and result entries.
/// </summary>
public interface IFavoritesManager
{
    /// <summary>Toggles the favorite status of a result entry.</summary>
    Task ToggleFavoriteAsync(Guid resultId, CancellationToken ct);

    /// <summary>Sets the favorite status of a result entry.</summary>
    Task SetFavoriteAsync(Guid resultId, bool isFavorite, CancellationToken ct);

    /// <summary>Gets all favorited result entries.</summary>
    Task<IReadOnlyList<ResultEntry>> GetFavoritesAsync(CancellationToken ct);

    /// <summary>Checks if a result entry is favorited.</summary>
    Task<bool> IsFavoriteAsync(Guid resultId, CancellationToken ct);
}
