using System.Collections.Immutable;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;

namespace SecurePort.Themes.Engines;

/// <summary>
/// Manages application themes including dark, light, and high-contrast modes.
/// Provides a color palette dictionary for each theme and raises events on theme changes.
/// </summary>
public sealed class ThemeManager : IThemeService
{
    private ThemeMode _currentTheme;
    private bool _isHighContrast;
    private bool _isReducedMotion;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManager"/> class
    /// with the default dark theme applied.
    /// </summary>
    public ThemeManager()
    {
        _currentTheme = ThemeMode.Dark;
        ColorPalettes = BuildAllPalettes();
    }

    /// <inheritdoc />
    public ThemeMode CurrentTheme => _currentTheme;

    /// <inheritdoc />
    public bool IsHighContrast => _isHighContrast;

    /// <inheritdoc />
    public bool IsReducedMotion => _isReducedMotion;

    /// <summary>
    /// Gets the immutable dictionary of all theme color palettes keyed by <see cref="ThemeMode"/>.
    /// </summary>
    public ImmutableDictionary<ThemeMode, IReadOnlyDictionary<string, ThemeColor>> ColorPalettes { get; }

    /// <summary>
    /// Gets the color palette for the currently active theme.
    /// </summary>
    public IReadOnlyDictionary<string, ThemeColor> CurrentPalette => ColorPalettes[_currentTheme];

    /// <inheritdoc />
    public event EventHandler<ThemeMode>? ThemeChanged;

    /// <summary>
    /// Gets the available theme modes supported by the application.
    /// </summary>
    /// <returns>A read-only list of available theme modes.</returns>
    public IReadOnlyList<ThemeMode> GetAvailableThemes()
    {
        return new ThemeMode[]
        {
            ThemeMode.Dark,
            ThemeMode.Light,
            ThemeMode.HighContrast,
            ThemeMode.System
        };
    }

    /// <summary>
    /// Sets the active visual theme for the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>A completed task after the theme has been applied.</returns>
    public Task SetThemeAsync(ThemeMode theme)
    {
        var resolved = theme == ThemeMode.System ? DetectSystemTheme() : theme;

        _isHighContrast = resolved == ThemeMode.HighContrast;
        _currentTheme = resolved;

        ThemeChanged?.Invoke(this, resolved);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the reduced motion preference. When enabled, UI animations should be suppressed.
    /// </summary>
    /// <param name="enabled">Whether reduced motion is active.</param>
    public void SetReducedMotion(bool enabled)
    {
        _isReducedMotion = enabled;
    }

    /// <summary>
    /// Gets a named color from the current theme palette.
    /// </summary>
    /// <param name="colorName">The well-known color name (e.g. "Background", "Primary").</param>
    /// <returns>The requested <see cref="ThemeColor"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="colorName"/> is not defined in the palette.</exception>
    public ThemeColor GetColor(string colorName)
    {
        if (CurrentPalette.TryGetValue(colorName, out var color))
            return color;

        throw new KeyNotFoundException($"Color '{colorName}' is not defined in the {_currentTheme} theme palette.");
    }

    /// <summary>
    /// Gets a named color as a hex string (e.g. "#E94560") from the current palette.
    /// </summary>
    /// <param name="colorName">The well-known color name.</param>
    /// <returns>The hex string representation of the color.</returns>
    public string GetColorHex(string colorName)
    {
        return GetColor(colorName).ToHex();
    }

    /// <summary>
    /// Gets a named color from a specific theme palette.
    /// </summary>
    /// <param name="theme">The theme to query.</param>
    /// <param name="colorName">The well-known color name.</param>
    /// <returns>The requested <see cref="ThemeColor"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the color is not defined in the specified palette.</exception>
    public ThemeColor GetColor(ThemeMode theme, string colorName)
    {
        var resolved = theme == ThemeMode.System ? DetectSystemTheme() : theme;

        if (ColorPalettes.TryGetValue(resolved, out var palette) &&
            palette.TryGetValue(colorName, out var color))
            return color;

        throw new KeyNotFoundException($"Color '{colorName}' is not defined in the {resolved} theme palette.");
    }

    /// <summary>
    /// Builds the complete set of color palettes for all supported themes.
    /// </summary>
    /// <returns>An immutable dictionary mapping each <see cref="ThemeMode"/> to its palette.</returns>
    private static ImmutableDictionary<ThemeMode, IReadOnlyDictionary<string, ThemeColor>> BuildAllPalettes()
    {
        return ImmutableDictionary<ThemeMode, IReadOnlyDictionary<string, ThemeColor>>.Empty
            .Add(ThemeMode.Dark, BuildDarkPalette())
            .Add(ThemeMode.Light, BuildLightPalette())
            .Add(ThemeMode.HighContrast, BuildHighContrastPalette())
            .Add(ThemeMode.System, BuildDarkPalette());
    }

    /// <summary>
    /// Builds the dark theme color palette with deep grays and vibrant accents.
    /// </summary>
    private static IReadOnlyDictionary<string, ThemeColor> BuildDarkPalette()
    {
        return new Dictionary<string, ThemeColor>
        {
            ["Background"] = new ThemeColor(0xFF, 0x1A, 0x1A, 0x2E),
            ["Surface"] = new ThemeColor(0xFF, 0x16, 0x21, 0x3E),
            ["Card"] = new ThemeColor(0xFF, 0x0F, 0x34, 0x60),
            ["Primary"] = new ThemeColor(0xFF, 0x53, 0x34, 0x83),
            ["Secondary"] = new ThemeColor(0xFF, 0x3A, 0x1F, 0x6E),
            ["Accent"] = new ThemeColor(0xFF, 0xE9, 0x45, 0x60),
            ["Text"] = new ThemeColor(0xFF, 0xF0, 0xF0, 0xF0),
            ["TextSecondary"] = new ThemeColor(0xFF, 0xA0, 0xA0, 0xB8),
            ["Border"] = new ThemeColor(0xFF, 0x2A, 0x2A, 0x4A),
            ["Success"] = new ThemeColor(0xFF, 0x00, 0xD2, 0xFF),
            ["Warning"] = new ThemeColor(0xFF, 0xFF, 0xB7, 0x4D),
            ["Error"] = new ThemeColor(0xFF, 0xE9, 0x45, 0x60),
            ["Info"] = new ThemeColor(0xFF, 0x00, 0xD2, 0xFF),
            ["Disabled"] = new ThemeColor(0xFF, 0x4A, 0x4A, 0x5E),
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Builds the light theme color palette with clean whites and professional blues.
    /// </summary>
    private static IReadOnlyDictionary<string, ThemeColor> BuildLightPalette()
    {
        return new Dictionary<string, ThemeColor>
        {
            ["Background"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0xFF),
            ["Surface"] = new ThemeColor(0xFF, 0xF8, 0xF9, 0xFA),
            ["Card"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0xFF),
            ["Primary"] = new ThemeColor(0xFF, 0x21, 0x96, 0xF3),
            ["Secondary"] = new ThemeColor(0xFF, 0x19, 0x76, 0xD2),
            ["Accent"] = new ThemeColor(0xFF, 0x03, 0xA9, 0xF4),
            ["Text"] = new ThemeColor(0xFF, 0x21, 0x21, 0x21),
            ["TextSecondary"] = new ThemeColor(0xFF, 0x75, 0x75, 0x75),
            ["Border"] = new ThemeColor(0xFF, 0xE0, 0xE0, 0xE0),
            ["Success"] = new ThemeColor(0xFF, 0x4C, 0xAF, 0x50),
            ["Warning"] = new ThemeColor(0xFF, 0xFF, 0x98, 0x00),
            ["Error"] = new ThemeColor(0xFF, 0xF4, 0x43, 0x36),
            ["Info"] = new ThemeColor(0xFF, 0x21, 0x96, 0xF3),
            ["Disabled"] = new ThemeColor(0xFF, 0xBD, 0xBD, 0xBD),
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Builds the high-contrast theme color palette using black, white, and yellow
    /// for maximum accessibility visibility.
    /// </summary>
    private static IReadOnlyDictionary<string, ThemeColor> BuildHighContrastPalette()
    {
        return new Dictionary<string, ThemeColor>
        {
            ["Background"] = new ThemeColor(0xFF, 0x00, 0x00, 0x00),
            ["Surface"] = new ThemeColor(0xFF, 0x0A, 0x0A, 0x0A),
            ["Card"] = new ThemeColor(0xFF, 0x14, 0x14, 0x14),
            ["Primary"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0x00),
            ["Secondary"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0xFF),
            ["Accent"] = new ThemeColor(0xFF, 0xFF, 0xD7, 0x00),
            ["Text"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0xFF),
            ["TextSecondary"] = new ThemeColor(0xFF, 0xFF, 0xD7, 0x00),
            ["Border"] = new ThemeColor(0xFF, 0xFF, 0xFF, 0xFF),
            ["Success"] = new ThemeColor(0xFF, 0x00, 0xFF, 0x00),
            ["Warning"] = new ThemeColor(0xFF, 0xFF, 0xD7, 0x00),
            ["Error"] = new ThemeColor(0xFF, 0xFF, 0x00, 0x00),
            ["Info"] = new ThemeColor(0xFF, 0x00, 0xFF, 0xFF),
            ["Disabled"] = new ThemeColor(0xFF, 0x80, 0x80, 0x80),
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// Detects the system theme preference. Defaults to dark when detection is unavailable.
    /// </summary>
    /// <returns>The resolved <see cref="ThemeMode"/>.</returns>
    private static ThemeMode DetectSystemTheme()
    {
        try
        {
            var preference = Environment.GetEnvironmentVariable("PREFERRED_COLOR_SCHEME");
            if (string.Equals(preference, "light", StringComparison.OrdinalIgnoreCase))
                return ThemeMode.Light;
        }
        catch
        {
            // Detection failed; fall back to dark.
        }

        return ThemeMode.Dark;
    }
}
