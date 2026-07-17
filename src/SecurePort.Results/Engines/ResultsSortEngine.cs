using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Provides sorting operations for result entries by multiple fields.
/// </summary>
public sealed class ResultsSortEngine : IResultsSortEngine
{
    public IReadOnlyList<ResultEntry> Sort(IReadOnlyList<ResultEntry> results, SortCriteria criteria)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        if (criteria is null)
            return results;

        IOrderedEnumerable<ResultEntry> ordered = criteria.Field switch
        {
            SortField.Port => criteria.Descending
                ? results.OrderByDescending(r => r.Port)
                : results.OrderBy(r => r.Port),

            SortField.State => criteria.Descending
                ? results.OrderByDescending(r => r.State.ToString())
                : results.OrderBy(r => r.State.ToString()),

            SortField.Service => criteria.Descending
                ? results.OrderByDescending(r => r.ServiceName ?? string.Empty)
                : results.OrderBy(r => r.ServiceName ?? string.Empty),

            SortField.Protocol => criteria.Descending
                ? results.OrderByDescending(r => r.Protocol.ToString())
                : results.OrderBy(r => r.Protocol.ToString()),

            SortField.ResponseTime => criteria.Descending
                ? results.OrderByDescending(r => r.ResponseTime ?? TimeSpan.MaxValue)
                : results.OrderBy(r => r.ResponseTime ?? TimeSpan.MaxValue),

            SortField.Target => criteria.Descending
                ? results.OrderByDescending(r => r.Target ?? string.Empty)
                : results.OrderBy(r => r.Target ?? string.Empty),

            SortField.ScanDate => criteria.Descending
                ? results.OrderByDescending(r => r.ScanDate)
                : results.OrderBy(r => r.ScanDate),

            SortField.ScanDuration => criteria.Descending
                ? results.OrderByDescending(r => r.ScanDuration ?? TimeSpan.Zero)
                : results.OrderBy(r => r.ScanDuration ?? TimeSpan.Zero),

            SortField.Confidence => criteria.Descending
                ? results.OrderByDescending(r => r.DetectionConfidence)
                : results.OrderBy(r => r.DetectionConfidence),

            SortField.Favorites => criteria.Descending
                ? results.OrderByDescending(r => r.IsFavorite).ThenBy(r => r.Port)
                : results.OrderBy(r => r.IsFavorite).ThenBy(r => r.Port),

            _ => results.OrderBy(r => r.Port)
        };

        return ordered.ToList();
    }
}
