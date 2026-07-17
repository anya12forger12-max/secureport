using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Provides combinable filtering operations for scan results.
/// Multiple filter criteria are applied with AND logic.
/// </summary>
public sealed class ResultsFilterEngine : IResultsFilterEngine
{
    public IReadOnlyList<ResultEntry> ApplyFilters(IReadOnlyList<ResultEntry> results, ResultFilterCriteria criteria)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        if (criteria is null)
            return results;

        IEnumerable<ResultEntry> query = results;

        if (criteria.StateFilter.HasValue)
        {
            var state = criteria.StateFilter.Value;
            query = query.Where(r => r.State == state);
        }

        if (criteria.ProtocolFilter.HasValue)
        {
            var protocol = criteria.ProtocolFilter.Value;
            query = query.Where(r => r.Protocol == protocol);
        }

        if (!string.IsNullOrWhiteSpace(criteria.ServiceFilter))
        {
            var service = criteria.ServiceFilter.Trim();
            query = query.Where(r =>
                r.ServiceName != null &&
                r.ServiceName.Contains(service, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.PortRangeMin.HasValue)
        {
            var min = criteria.PortRangeMin.Value;
            query = query.Where(r => r.Port >= min);
        }

        if (criteria.PortRangeMax.HasValue)
        {
            var max = criteria.PortRangeMax.Value;
            query = query.Where(r => r.Port <= max);
        }

        if (criteria.MinResponseTime.HasValue)
        {
            var minTime = criteria.MinResponseTime.Value;
            query = query.Where(r => r.ResponseTime.HasValue && r.ResponseTime.Value >= minTime);
        }

        if (criteria.MaxResponseTime.HasValue)
        {
            var maxTime = criteria.MaxResponseTime.Value;
            query = query.Where(r => r.ResponseTime.HasValue && r.ResponseTime.Value <= maxTime);
        }

        if (!string.IsNullOrWhiteSpace(criteria.ScanProfileFilter))
        {
            var profile = criteria.ScanProfileFilter.Trim();
            query = query.Where(r =>
                r.ScanProfile != null &&
                r.ScanProfile.Contains(profile, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.DateRangeStart.HasValue)
        {
            var start = criteria.DateRangeStart.Value;
            query = query.Where(r => r.ScanDate >= start);
        }

        if (criteria.DateRangeEnd.HasValue)
        {
            var end = criteria.DateRangeEnd.Value;
            query = query.Where(r => r.ScanDate <= end);
        }

        if (criteria.FavoritesOnly)
        {
            query = query.Where(r => r.IsFavorite);
        }

        if (criteria.TagFilter is { Count: > 0 })
        {
            var tags = new HashSet<string>(criteria.TagFilter, StringComparer.OrdinalIgnoreCase);
            query = query.Where(r => r.Tags.Any(t => tags.Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.TargetFilter))
        {
            var target = criteria.TargetFilter.Trim();
            query = query.Where(r =>
                r.Target != null &&
                r.Target.Contains(target, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.ScanIdFilter.HasValue)
        {
            var scanId = criteria.ScanIdFilter.Value;
            query = query.Where(r => r.ScanId == scanId);
        }

        return query.ToList();
    }
}
