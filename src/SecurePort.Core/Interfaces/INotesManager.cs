namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages user notes for scan results.
/// </summary>
public interface INotesManager
{
    /// <summary>Gets the notes for a result entry.</summary>
    Task<string?> GetNotesAsync(Guid resultId, CancellationToken ct);

    /// <summary>Saves notes for a result entry.</summary>
    Task SaveNotesAsync(Guid resultId, string notes, CancellationToken ct);

    /// <summary>Clears notes for a result entry.</summary>
    Task ClearNotesAsync(Guid resultId, CancellationToken ct);

    /// <summary>Searches notes for entries containing the specified text.</summary>
    Task<IReadOnlyList<Guid>> SearchNotesAsync(string searchText, CancellationToken ct);
}
