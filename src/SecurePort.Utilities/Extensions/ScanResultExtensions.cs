using System.Text;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Utilities.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="ScanResult"/>
/// collections, including filtering, grouping, and export formatting.
/// </summary>
public static class ScanResultExtensions
{
    /// <summary>
    /// Filters the results to only include ports in the specified state.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <param name="state">The port state to filter by.</param>
    /// <returns>A filtered enumerable of matching results.</returns>
    public static IEnumerable<ScanResult> FilterByState(this IEnumerable<ScanResult> results, PortState state)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        return results.Where(r => r.State == state);
    }

    /// <summary>
    /// Filters the results to only include open ports.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>An enumerable of results representing open ports.</returns>
    public static IEnumerable<ScanResult> OpenPorts(this IEnumerable<ScanResult> results)
    {
        return results.FilterByState(PortState.Open);
    }

    /// <summary>
    /// Filters the results to only include closed ports.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>An enumerable of results representing closed ports.</returns>
    public static IEnumerable<ScanResult> ClosedPorts(this IEnumerable<ScanResult> results)
    {
        return results.FilterByState(PortState.Closed);
    }

    /// <summary>
    /// Filters the results to only include filtered ports.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>An enumerable of results representing filtered ports.</returns>
    public static IEnumerable<ScanResult> FilteredPorts(this IEnumerable<ScanResult> results)
    {
        return results.FilterByState(PortState.Filtered);
    }

    /// <summary>
    /// Groups scan results by their <see cref="PortState"/> and returns
    /// a dictionary mapping each state to its result count.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>A dictionary mapping port states to counts.</returns>
    public static IReadOnlyDictionary<PortState, int> CountByState(this IEnumerable<ScanResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        return results
            .GroupBy(r => r.State)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Groups scan results by their detected service name.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>A dictionary mapping service names to their matching results.</returns>
    public static IReadOnlyDictionary<string, List<ScanResult>> GroupByService(this IEnumerable<ScanResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        return results
            .Where(r => !string.IsNullOrEmpty(r.ServiceName))
            .GroupBy(r => r.ServiceName!)
            .ToDictionary(g => g.Key!, g => g.ToList());
    }

    /// <summary>
    /// Calculates the average response time across all results that recorded one.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>The average <see cref="TimeSpan"/>, or <c>null</c> if no response times were recorded.</returns>
    public static TimeSpan? AverageResponseTime(this IEnumerable<ScanResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var timedResults = results.Where(r => r.ResponseTime.HasValue).ToList();
        if (timedResults.Count == 0)
            return null;

        var ticks = timedResults.Average(r => r.ResponseTime!.Value.Ticks);
        return TimeSpan.FromTicks((long)ticks);
    }

    /// <summary>
    /// Returns the results sorted by port number in ascending order.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>An enumerable of results ordered by port.</returns>
    public static IEnumerable<ScanResult> OrderByPort(this IEnumerable<ScanResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        return results.OrderBy(r => r.Port);
    }

    /// <summary>
    /// Generates a CSV-formatted string from the scan results.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <param name="includeHeader">Whether to include a CSV header row.</param>
    /// <returns>A CSV-formatted string.</returns>
    public static string ToCsv(this IEnumerable<ScanResult> results, bool includeHeader = true)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var sb = new StringBuilder();

        if (includeHeader)
            sb.AppendLine("Host,Port,Protocol,State,Service,Banner,ResponseTime,ScannedAt");

        foreach (var r in results)
        {
            var responseTime = r.ResponseTime.HasValue ? r.ResponseTime.Value.TotalMilliseconds.ToString("F2") : "";
            var service = EscapeCsv(r.ServiceName ?? "");
            var banner = EscapeCsv(r.BannerInfo ?? "");

            sb.AppendLine($"{EscapeCsv(r.Host)},{r.Port},{r.Protocol},{r.State},{service},{banner},{responseTime},{r.ScannedAt:O}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts the unique list of service names detected across all results.
    /// </summary>
    /// <param name="results">The collection of scan results.</param>
    /// <returns>A read-only list of unique, non-empty service names.</returns>
    public static IReadOnlyList<string> DistinctServices(this IEnumerable<ScanResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        return results
            .Where(r => !string.IsNullOrEmpty(r.ServiceName))
            .Select(r => r.ServiceName!)
            .Distinct()
            .OrderBy(s => s)
            .ToList();
    }

    /// <summary>
    /// Escapes a value for safe inclusion in a CSV field.
    /// Wraps in quotes if the value contains commas, quotes, or newlines.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped CSV value.</returns>
    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
