namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages user-defined tags for scan results.
/// </summary>
public interface ITagManager
{
    /// <summary>Gets all available tags.</summary>
    Task<IReadOnlyList<string>> GetAllTagsAsync(CancellationToken ct);

    /// <summary>Creates a new tag.</summary>
    Task<string> CreateTagAsync(string tagName, CancellationToken ct);

    /// <summary>Renames an existing tag.</summary>
    Task<bool> RenameTagAsync(string oldName, string newName, CancellationToken ct);

    /// <summary>Deletes a tag.</summary>
    Task<bool> DeleteTagAsync(string tagName, CancellationToken ct);

    /// <summary>Adds a tag to a result entry.</summary>
    Task AddTagToResultAsync(Guid resultId, string tagName, CancellationToken ct);

    /// <summary>Removes a tag from a result entry.</summary>
    Task RemoveTagFromResultAsync(Guid resultId, string tagName, CancellationToken ct);

    /// <summary>Gets all tags assigned to a result entry.</summary>
    Task<IReadOnlyList<string>> GetTagsForResultAsync(Guid resultId, CancellationToken ct);

    /// <summary>Gets all result IDs that have the specified tag.</summary>
    Task<IReadOnlyList<Guid>> GetResultIdsByTagAsync(string tagName, CancellationToken ct);
}
