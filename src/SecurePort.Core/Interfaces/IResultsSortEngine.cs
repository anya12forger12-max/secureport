using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides sorting operations for scan result collections.
/// </summary>
public interface IResultsSortEngine
{
    /// <summary>
    /// Sorts results by the specified sort criteria.
    /// </summary>
    IReadOnlyList<ResultEntry> Sort(IReadOnlyList<ResultEntry> results, SortCriteria criteria);
}

/// <summary>
/// Defines sorting criteria for result entries.
/// </summary>
public sealed record SortCriteria
{
    /// <summary>The field to sort by.</summary>
    public required SortField Field { get; init; }

    /// <summary>Whether to sort in descending order.</summary>
    public required bool Descending { get; init; }
}

/// <summary>
/// Fields available for sorting result entries.
/// </summary>
public enum SortField
{
    /// <summary>Sort by port number.</summary>
    Port,

    /// <summary>Sort by port state.</summary>
    State,

    /// <summary>Sort by service name.</summary>
    Service,

    /// <summary>Sort by protocol.</summary>
    Protocol,

    /// <summary>Sort by response time.</summary>
    ResponseTime,

    /// <summary>Sort by target host.</summary>
    Target,

    /// <summary>Sort by scan date.</summary>
    ScanDate,

    /// <summary>Sort by scan duration.</summary>
    ScanDuration,

    /// <summary>Sort by detection confidence.</summary>
    Confidence,

    /// <summary>Sort by favorite status (favorites first).</summary>
    Favorites
}
