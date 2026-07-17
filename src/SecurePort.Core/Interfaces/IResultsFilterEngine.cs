using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides combinable filtering operations for scan results.
/// </summary>
public interface IResultsFilterEngine
{
    /// <summary>
    /// Applies the specified filter criteria to a collection of results.
    /// Multiple criteria are combined with AND logic.
    /// </summary>
    IReadOnlyList<ResultEntry> ApplyFilters(IReadOnlyList<ResultEntry> results, ResultFilterCriteria criteria);
}

/// <summary>
/// Defines combinable criteria for filtering scan results.
/// </summary>
public sealed record ResultFilterCriteria
{
    /// <summary>Filter by port state, or null for all states.</summary>
    public PortState? StateFilter { get; init; }

    /// <summary>Filter by protocol, or null for all protocols.</summary>
    public ProtocolType? ProtocolFilter { get; init; }

    /// <summary>Filter by service name (partial match), or null for all services.</summary>
    public string? ServiceFilter { get; init; }

    /// <summary>Minimum port number (inclusive), or null for no lower bound.</summary>
    public int? PortRangeMin { get; init; }

    /// <summary>Maximum port number (inclusive), or null for no upper bound.</summary>
    public int? PortRangeMax { get; init; }

    /// <summary>Minimum response time, or null for no lower bound.</summary>
    public TimeSpan? MinResponseTime { get; init; }

    /// <summary>Maximum response time, or null for no upper bound.</summary>
    public TimeSpan? MaxResponseTime { get; init; }

    /// <summary>Filter by scan profile name, or null for all profiles.</summary>
    public string? ScanProfileFilter { get; init; }

    /// <summary>Start of date range (inclusive), or null for no lower bound.</summary>
    public DateTimeOffset? DateRangeStart { get; init; }

    /// <summary>End of date range (inclusive), or null for no upper bound.</summary>
    public DateTimeOffset? DateRangeEnd { get; init; }

    /// <summary>When true, only return favorited results.</summary>
    public bool FavoritesOnly { get; init; }

    /// <summary>Filter by tag. Results must contain at least one of the specified tags.</summary>
    public IReadOnlyList<string>? TagFilter { get; init; }

    /// <summary>Filter by target host (partial match), or null for all targets.</summary>
    public string? TargetFilter { get; init; }

    /// <summary>Filter by scan ID, or null for all scans.</summary>
    public Guid? ScanIdFilter { get; init; }
}
