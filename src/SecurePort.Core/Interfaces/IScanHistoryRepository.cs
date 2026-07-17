using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides persistence operations for scan session history.
/// </summary>
public interface IScanHistoryRepository
{
    /// <summary>
    /// Saves a completed scan session to history.
    /// </summary>
    /// <param name="session">The scan session to persist.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when the session has been saved.</returns>
    Task SaveAsync(ScanSession session, CancellationToken ct);

    /// <summary>
    /// Retrieves a scan session by its unique identifier.
    /// </summary>
    /// <param name="id">The session identifier.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The scan session, or null if not found.</returns>
    Task<ScanSession?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Retrieves all stored scan sessions ordered by creation time descending.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of scan sessions.</returns>
    Task<IReadOnlyList<ScanSession>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves a page of scan sessions.
    /// </summary>
    /// <param name="offset">The number of sessions to skip.</param>
    /// <param name="limit">The maximum number of sessions to return.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of scan sessions for the requested page.</returns>
    Task<IReadOnlyList<ScanSession>> GetPageAsync(int offset, int limit, CancellationToken ct);

    /// <summary>
    /// Deletes a scan session by its unique identifier.
    /// </summary>
    /// <param name="id">The session identifier to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the session was removed; false if it was not found.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Gets the total number of stored scan sessions.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The count of stored sessions.</returns>
    Task<int> CountAsync(CancellationToken ct);

    /// <summary>
    /// Deletes all stored scan sessions.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when all sessions have been removed.</returns>
    Task ClearAsync(CancellationToken ct);

    /// <summary>
    /// Removes sessions older than the specified date.
    /// </summary>
    /// <param name="olderThan">The cutoff date; sessions before this date will be removed.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of sessions that were removed.</returns>
    Task<int> PurgeOlderThanAsync(DateTimeOffset olderThan, CancellationToken ct);
}
