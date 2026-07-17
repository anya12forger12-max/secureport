namespace SecurePort.Core.Models;

/// <summary>
/// Represents a single data point for chart visualization.
/// </summary>
public sealed record ChartDataPoint
{
    /// <summary>The label for this data point.</summary>
    public required string Label { get; init; }

    /// <summary>The numeric value.</summary>
    public required double Value { get; init; }

    /// <summary>Optional secondary value for dual-axis charts.</summary>
    public double? SecondaryValue { get; init; }

    /// <summary>Optional tooltip text.</summary>
    public string? Tooltip { get; init; }
}

/// <summary>
/// Represents a named data series for chart rendering.
/// </summary>
public sealed record ChartSeries
{
    /// <summary>The series name (appears in legends).</summary>
    public required string Name { get; init; }

    /// <summary>The data points in this series.</summary>
    public required IReadOnlyList<ChartDataPoint> DataPoints { get; init; }
}

/// <summary>
/// Contains all data needed to render a specific chart type.
/// </summary>
public sealed record ChartDataSet
{
    /// <summary>The chart title.</summary>
    public required string Title { get; init; }

    /// <summary>The data series to render.</summary>
    public required IReadOnlyList<ChartSeries> Series { get; init; }

    /// <summary>Textual summary of the chart data for accessibility.</summary>
    public required string AccessibilitySummary { get; init; }
}
