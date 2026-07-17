using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides storage and retrieval operations for result entries.
/// </summary>
public interface IResultsRepository
{
    /// <summary>Saves a single result entry.</summary>
    Task SaveAsync(ResultEntry entry, CancellationToken ct);

    /// <summary>Saves multiple result entries in a batch.</summary>
    Task SaveBatchAsync(IReadOnlyList<ResultEntry> entries, CancellationToken ct);

    /// <summary>Retrieves a result entry by its unique identifier.</summary>
    Task<ResultEntry?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Retrieves all result entries for a given scan session.</summary>
    Task<IReadOnlyList<ResultEntry>> GetByScanIdAsync(Guid scanId, CancellationToken ct);

    /// <summary>Retrieves all result entries.</summary>
    Task<IReadOnlyList<ResultEntry>> GetAllAsync(CancellationToken ct);

    /// <summary>Updates an existing result entry.</summary>
    Task UpdateAsync(ResultEntry entry, CancellationToken ct);

    /// <summary>Deletes a result entry by its unique identifier.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    /// <summary>Deletes all result entries for a given scan session.</summary>
    Task<int> DeleteByScanIdAsync(Guid scanId, CancellationToken ct);

    /// <summary>Gets the total count of result entries.</summary>
    Task<int> CountAsync(CancellationToken ct);

    /// <summary>Gets the count of result entries for a given scan session.</summary>
    Task<int> CountByScanIdAsync(Guid scanId, CancellationToken ct);
}
