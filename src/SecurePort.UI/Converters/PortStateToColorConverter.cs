using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SecurePort.Core.Enums;

namespace SecurePort.UI.Converters;

/// <summary>
/// Converts a <see cref="PortState"/> enum value to the corresponding theme brush.
/// </summary>
public sealed class PortStateToColorConverter : IValueConverter
{
    /// <summary>
    /// Shared instance to avoid repeated allocations in XAML bindings.
    /// </summary>
    public static readonly PortStateToColorConverter Instance = new();

    /// <summary>
    /// Maps <see cref="PortState"/> values to theme brush resource keys.
    /// </summary>
    private static readonly SolidColorBrush OpenBrush = new(Color.Parse("#48bb78"));
    private static readonly SolidColorBrush ClosedBrush = new(Color.Parse("#fc8181"));
    private static readonly SolidColorBrush FilteredBrush = new(Color.Parse("#ed8936"));
    private static readonly SolidColorBrush TimeoutBrush = new(Color.Parse("#4299e1"));
    private static readonly SolidColorBrush UnknownBrush = new(Color.Parse("#4a5568"));

    /// <summary>
    /// Converts a <see cref="PortState"/> to a <see cref="IBrush"/>.
    /// </summary>
    /// <param name="value">The <see cref="PortState"/> value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">Optional parameter. Not used.</param>
    /// <param name="culture">The current culture.</param>
    /// <returns>A <see cref="SolidColorBrush"/> corresponding to the port state.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PortState state)
            return UnknownBrush;

        return state switch
        {
            PortState.Open => OpenBrush,
            PortState.Closed => ClosedBrush,
            PortState.Filtered => FilteredBrush,
            PortState.Timeout => TimeoutBrush,
            PortState.Unknown => UnknownBrush,
            _ => UnknownBrush
        };
    }

    /// <summary>
    /// Not supported. Returns <see cref="PortState.Unknown"/>.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return PortState.Unknown;
    }
}
