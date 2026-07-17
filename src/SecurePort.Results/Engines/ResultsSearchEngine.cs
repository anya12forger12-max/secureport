using System.Text;
using System.Text.RegularExpressions;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Provides instant local search across scan results with case-insensitive
/// partial matching, multiple keyword support, and match highlighting.
/// </summary>
public sealed class ResultsSearchEngine : IResultsSearchEngine
{
    public IReadOnlyList<ResultEntry> Search(IReadOnlyList<ResultEntry> results, string query)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        if (string.IsNullOrWhiteSpace(query))
            return results;

        var keywords = query.Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => k.Length > 0)
            .ToList();

        if (keywords.Count == 0)
            return results;

        return results.Where(r => MatchesAllKeywords(r, keywords)).ToList();
    }

    public IReadOnlyList<TextRange> FindMatches(string text, string query)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(query))
            return Array.Empty<TextRange>();

        var keywords = query.Trim()
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => Regex.Escape(k.Trim()))
            .Where(k => k.Length > 0)
            .ToList();

        if (keywords.Count == 0)
            return Array.Empty<TextRange>();

        var ranges = new List<TextRange>();
        var pattern = string.Join("|", keywords);

        foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
        {
            ranges.Add(new TextRange { Start = match.Index, Length = match.Length });
        }

        return ranges;
    }

    private static bool MatchesAllKeywords(ResultEntry entry, IReadOnlyList<string> keywords)
    {
        var searchableText = BuildSearchableText(entry);

        return keywords.All(keyword =>
            searchableText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildSearchableText(ResultEntry entry)
    {
        var sb = new StringBuilder();
        sb.Append(entry.Port);
        sb.Append(' ');
        sb.Append(entry.Target ?? string.Empty);
        sb.Append(' ');
        sb.Append(entry.Protocol.ToString());
        sb.Append(' ');
        sb.Append(entry.State.ToString());
        sb.Append(' ');
        sb.Append(entry.ServiceName ?? string.Empty);
        sb.Append(' ');
        sb.Append(entry.ServiceDescription ?? string.Empty);
        sb.Append(' ');
        sb.Append(entry.ScanId.ToString("D"));
        sb.Append(' ');
        sb.Append(entry.ScanProfile ?? string.Empty);
        sb.Append(' ');
        sb.Append(entry.ScanDate.ToString("yyyy-MM-dd"));
        sb.Append(' ');
        sb.Append(entry.Banner ?? string.Empty);

        foreach (var tag in entry.Tags)
        {
            sb.Append(' ');
            sb.Append(tag);
        }

        if (!string.IsNullOrEmpty(entry.Notes))
        {
            sb.Append(' ');
            sb.Append(entry.Notes);
        }

        return sb.ToString();
    }
}
