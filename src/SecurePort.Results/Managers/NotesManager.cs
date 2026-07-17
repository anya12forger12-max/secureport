using System.Collections.Concurrent;
using SecurePort.Core.Interfaces;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages user notes for scan result entries using in-memory storage.
/// </summary>
public sealed class NotesManager : INotesManager
{
    private readonly ConcurrentDictionary<Guid, string> _notes = new();
    private readonly IResultsRepository _repository;

    public NotesManager(IResultsRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<string?> GetNotesAsync(Guid resultId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _notes.TryGetValue(resultId, out var notes);
        return Task.FromResult(notes);
    }

    public async Task SaveNotesAsync(Guid resultId, string notes, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notes);
        ct.ThrowIfCancellationRequested();

        _notes[resultId] = notes;

        var entry = await _repository.GetByIdAsync(resultId, ct);
        if (entry is not null)
        {
            var updated = entry with { Notes = notes };
            await _repository.UpdateAsync(updated, ct);
        }
    }

    public async Task ClearNotesAsync(Guid resultId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _notes.TryRemove(resultId, out _);

        var entry = await _repository.GetByIdAsync(resultId, ct);
        if (entry is not null)
        {
            var updated = entry with { Notes = null };
            await _repository.UpdateAsync(updated, ct);
        }
    }

    public Task<IReadOnlyList<Guid>> SearchNotesAsync(string searchText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());

        ct.ThrowIfCancellationRequested();

        var matches = _notes
            .Where(kvp => kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList();

        return Task.FromResult<IReadOnlyList<Guid>>(matches);
    }
}
