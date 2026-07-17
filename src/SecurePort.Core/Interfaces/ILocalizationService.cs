using SecurePort.Core.Enums;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides localized string resources and language management.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the currently active language.
    /// </summary>
    Language CurrentLanguage { get; }

    /// <summary>
    /// Sets the active language for the application.
    /// </summary>
    /// <param name="language">The language to apply.</param>
    /// <returns>A task that completes when the language has been applied.</returns>
    Task SetLanguageAsync(Language language);

    /// <summary>
    /// Gets a localized string by its resource key.
    /// </summary>
    /// <param name="key">The resource key for the localized string.</param>
    /// <returns>The localized string, or the key itself if no translation was found.</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized format string and applies the specified arguments.
    /// </summary>
    /// <param name="key">The resource key for the localized format string.</param>
    /// <param name="args">The arguments to inject into the format string.</param>
    /// <returns>The formatted localized string.</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Gets all available languages supported by the application.
    /// </summary>
    /// <returns>A read-only list of available languages.</returns>
    IReadOnlyList<Language> GetAvailableLanguages();

    /// <summary>
    /// Gets the display name for the specified language in its own locale.
    /// </summary>
    /// <param name="language">The language to get the display name for.</param>
    /// <returns>The native display name of the language.</returns>
    string GetLanguageName(Language language);

    /// <summary>
    /// Occurs when the active language has changed.
    /// </summary>
    event EventHandler<Language>? LanguageChanged;
}
