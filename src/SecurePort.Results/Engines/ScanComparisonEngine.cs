using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Engines;

/// <summary>
/// Compares two or more completed scan sessions and identifies port-level differences.
/// </summary>
public sealed class ScanComparisonEngine : IScanComparisonEngine
{
    public ScanComparisonResult Compare(ScanSession previous, ScanSession current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var previousResults = previous.Results.ToDictionary(r => (r.Port, r.Protocol));
        var currentResults = current.Results.ToDictionary(r => (r.Port, r.Protocol));

        var allKeys = previousResults.Keys
            .Union(currentResults.Keys)
            .OrderBy(k => k.Port)
            .ThenBy(k => k.Protocol)
            .ToList();

        var differences = new List<PortDifference>();
        int newlyOpen = 0, newlyClosed = 0, serviceChanges = 0, unchanged = 0;

        foreach (var (port, protocol) in allKeys)
        {
            previousResults.TryGetValue((port, protocol), out var prev);
            currentResults.TryGetValue((port, protocol), out var curr);

            if (prev is not null && curr is not null)
            {
                var diffType = ClassifyDifference(prev, curr);
                if (diffType == DifferenceType.Unchanged)
                {
                    unchanged++;
                }
                else if (diffType == DifferenceType.ServiceChanged)
                {
                    serviceChanges++;
                }

                differences.Add(new PortDifference
                {
                    Port = port,
                    Protocol = protocol,
                    PreviousState = prev.State,
                    CurrentState = curr.State,
                    PreviousService = prev.ServiceName,
                    CurrentService = curr.ServiceName,
                    PreviousResponseTime = prev.ResponseTime,
                    CurrentResponseTime = curr.ResponseTime,
                    DifferenceType = diffType
                });
            }
            else if (prev is null && curr is not null)
            {
                newlyOpen++;
                differences.Add(new PortDifference
                {
                    Port = port,
                    Protocol = protocol,
                    PreviousState = null,
                    CurrentState = curr.State,
                    PreviousService = null,
                    CurrentService = curr.ServiceName,
                    PreviousResponseTime = null,
                    CurrentResponseTime = curr.ResponseTime,
                    DifferenceType = DifferenceType.Added
                });
            }
            else if (prev is not null && curr is null)
            {
                newlyClosed++;
                differences.Add(new PortDifference
                {
                    Port = port,
                    Protocol = protocol,
                    PreviousState = prev.State,
                    CurrentState = null,
                    PreviousService = prev.ServiceName,
                    CurrentService = null,
                    PreviousResponseTime = prev.ResponseTime,
                    CurrentResponseTime = null,
                    DifferenceType = DifferenceType.Removed
                });
            }
        }

        TimeSpan? durationDiff = null;
        if (previous.Duration.HasValue && current.Duration.HasValue)
        {
            durationDiff = current.Duration.Value - previous.Duration.Value;
        }

        return new ScanComparisonResult
        {
            PreviousScanId = previous.Id,
            CurrentScanId = current.Id,
            Differences = differences,
            NewlyOpenCount = newlyOpen,
            NewlyClosedCount = newlyClosed,
            ServiceChangesCount = serviceChanges,
            UnchangedCount = unchanged,
            DurationDifference = durationDiff,
            ComparedAt = DateTimeOffset.UtcNow
        };
    }

    public IReadOnlyList<ScanComparisonResult> CompareMultiple(IReadOnlyList<ScanSession> sessions)
    {
        if (sessions is null)
            throw new ArgumentNullException(nameof(sessions));

        if (sessions.Count < 2)
            return Array.Empty<ScanComparisonResult>();

        var sorted = sessions.OrderBy(s => s.StartTime).ToList();
        var comparisons = new List<ScanComparisonResult>();

        for (int i = 1; i < sorted.Count; i++)
        {
            comparisons.Add(Compare(sorted[i - 1], sorted[i]));
        }

        return comparisons;
    }

    private static DifferenceType ClassifyDifference(ScanResult prev, ScanResult curr)
    {
        if (prev.State != curr.State)
        {
            return (prev.State, curr.State) switch
            {
                (PortState.Open, _) when curr.State != PortState.Open => DifferenceType.NewlyClosed,
                (_, PortState.Open) when prev.State != PortState.Open => DifferenceType.NewlyOpen,
                _ => DifferenceType.Unchanged
            };
        }

        if (!string.Equals(prev.ServiceName, curr.ServiceName, StringComparison.OrdinalIgnoreCase))
            return DifferenceType.ServiceChanged;

        if (prev.ResponseTime.HasValue && curr.ResponseTime.HasValue)
        {
            var diff = Math.Abs((prev.ResponseTime.Value - curr.ResponseTime.Value).TotalMilliseconds);
            if (diff > 100)
                return DifferenceType.ResponseTimeChanged;
        }

        return DifferenceType.Unchanged;
    }
}
