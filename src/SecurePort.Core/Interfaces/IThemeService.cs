using SecurePort.Core.Enums;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages the application visual theme and appearance settings.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    ThemeMode CurrentTheme { get; }

    /// <summary>
    /// Sets the active visual theme for the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>A task that completes when the theme has been applied.</returns>
    Task SetThemeAsync(ThemeMode theme);

    /// <summary>
    /// Gets the available themes supported by the application.
    /// </summary>
    /// <returns>A read-only list of available theme modes.</returns>
    IReadOnlyList<ThemeMode> GetAvailableThemes();

    /// <summary>
    /// Gets a value indicating whether high-contrast mode is currently active.
    /// </summary>
    bool IsHighContrast { get; }

    /// <summary>
    /// Gets a value indicating whether reduced motion is currently active.
    /// </summary>
    bool IsReducedMotion { get; }

    /// <summary>
    /// Occurs when the active theme has changed.
    /// </summary>
    event EventHandler<ThemeMode>? ThemeChanged;
}
