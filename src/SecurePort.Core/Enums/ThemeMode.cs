namespace SecurePort.Core.Enums;

/// <summary>
/// Specifies the visual theme applied to the application interface.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// Dark color scheme with reduced brightness.
    /// </summary>
    Dark,

    /// <summary>
    /// Light color scheme with standard brightness.
    /// </summary>
    Light,

    /// <summary>
    /// High-contrast color scheme for improved accessibility.
    /// </summary>
    HighContrast,

    /// <summary>
    /// Automatically matches the operating system theme preference.
    /// </summary>
    System
}
