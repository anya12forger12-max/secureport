using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides instant local search across scan results.
/// </summary>
public interface IResultsSearchEngine
{
    /// <summary>
    /// Searches results matching the given query across all searchable fields.
    /// Supports multiple keywords, case-insensitive partial matching.
    /// </summary>
    IReadOnlyList<ResultEntry> Search(IReadOnlyList<ResultEntry> results, string query);

    /// <summary>
    /// Highlights matching portions of a text string.
    /// Returns the text with match boundaries marked.
    /// </summary>
    IReadOnlyList<TextRange> FindMatches(string text, string query);
}

/// <summary>
/// Represents a matched range within a text string.
/// </summary>
public sealed record TextRange
{
    /// <summary>The start index of the match.</summary>
    public required int Start { get; init; }

    /// <summary>The length of the match.</summary>
    public required int Length { get; init; }
}
