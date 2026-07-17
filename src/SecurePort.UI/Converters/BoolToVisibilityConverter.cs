using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SecurePort.UI.Converters;

/// <summary>
/// Converts a <see cref="bool"/> value to a visibility indicator.
/// <c>true</c> maps to visible (opacity 1), <c>false</c> maps to collapsed (opacity 0).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Shared instance to avoid repeated allocations in XAML bindings.
    /// </summary>
    public static readonly BoolToVisibilityConverter Instance = new();

    /// <summary>
    /// Converts a boolean to a double representing visibility.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (expected <see cref="double"/> or <see cref="object"/>).</param>
    /// <param name="parameter">Optional parameter. Pass <c>"Invert"</c> to reverse the mapping.</param>
    /// <param name="culture">The current culture.</param>
    /// <returns><c>1.0</c> when visible, <c>0.0</c> when collapsed.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return 0.0;

        bool isInvert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        bool isVisible = isInvert ? !boolValue : boolValue;

        return isVisible ? 1.0 : 0.0;
    }

    /// <summary>
    /// Converts a visibility double back to a boolean.
    /// </summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
            return false;

        bool result = doubleValue > 0.5;

        bool isInvert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        return isInvert ? !result : result;
    }
}
