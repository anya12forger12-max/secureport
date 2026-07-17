using System.Globalization;
using System.Text.Json;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;

namespace SecurePort.Localization.Providers;

/// <summary>
/// Provides localized string resources and language management by loading
/// embedded JSON resource files for each supported language.
/// Falls back to English when a key is not found in the active language.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private static readonly Dictionary<Language, string> CultureMap = new()
    {
        [Language.English] = "en-US",
        [Language.Spanish] = "es-ES",
        [Language.French] = "fr-FR",
        [Language.German] = "de-DE",
        [Language.Japanese] = "ja-JP",
        [Language.Portuguese] = "pt-BR",
        [Language.Arabic] = "ar-SA",
        [Language.Hindi] = "hi-IN",
        [Language.Chinese] = "zh-CN",
    };

    private static readonly Dictionary<Language, string> DisplayNameMap = new()
    {
        [Language.English] = "English",
        [Language.Spanish] = "Español",
        [Language.French] = "Français",
        [Language.German] = "Deutsch",
        [Language.Japanese] = "日本語",
        [Language.Portuguese] = "Português",
        [Language.Arabic] = "العربية",
        [Language.Hindi] = "हिन्दी",
        [Language.Chinese] = "中文",
    };

    private readonly Dictionary<Language, Dictionary<string, string>> _stringTables = new();
    private Dictionary<string, string> _currentStrings;
    private Dictionary<string, string> _fallbackStrings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class,
    /// preloading the English string table as the fallback language.
    /// </summary>
    public LocalizationService()
    {
        _currentStrings = new Dictionary<string, string>();
        _fallbackStrings = LoadEmbeddedStrings(Language.English);

        _stringTables[Language.English] = _fallbackStrings;
        _currentStrings = _fallbackStrings;
    }

    /// <inheritdoc />
    public Language CurrentLanguage { get; private set; } = Language.English;

    /// <inheritdoc />
    public event EventHandler<Language>? LanguageChanged;

    /// <inheritdoc />
    public Task SetLanguageAsync(Language language)
    {
        if (CurrentLanguage == language)
            return Task.CompletedTask;

        if (!_stringTables.ContainsKey(language))
        {
            var loaded = LoadEmbeddedStrings(language);
            _stringTables[language] = loaded;
        }

        _currentStrings = _stringTables[language];
        CurrentLanguage = language;

        LanguageChanged?.Invoke(this, language);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        if (_currentStrings.TryGetValue(key, out var value))
            return value;

        if (_fallbackStrings.TryGetValue(key, out var fallback))
            return fallback;

        return key;
    }

    /// <inheritdoc />
    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);

        if (args.Length == 0)
            return template;

        try
        {
            var culture = GetCultureInfo(CurrentLanguage);
            return string.Format(culture, template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<Language> GetAvailableLanguages()
    {
        return Enum.GetValues<Language>();
    }

    /// <inheritdoc />
    public string GetLanguageName(Language language)
    {
        return DisplayNameMap.TryGetValue(language, out var name) ? name : language.ToString();
    }

    /// <summary>
    /// Formats a <see cref="DateTime"/> using the culture settings for the specified language.
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <param name="language">The target language whose culture formatting rules to apply.</param>
    /// <param name="format">An optional custom format string. If <c>null</c>, the culture's short date pattern is used.</param>
    /// <returns>The formatted date string.</returns>
    public string FormatDate(DateTime date, Language language, string? format = null)
    {
        var culture = GetCultureInfo(language);
        return date.ToString(format ?? culture.DateTimeFormat.ShortDatePattern, culture);
    }

    /// <summary>
    /// Formats a numeric value using the culture settings for the specified language.
    /// </summary>
    /// <param name="value">The number to format.</param>
    /// <param name="language">The target language whose culture formatting rules to apply.</param>
    /// <param name="format">An optional numeric format string (e.g. "N2", "C").</param>
    /// <returns>The formatted number string.</returns>
    public string FormatNumber(double value, Language language, string? format = null)
    {
        var culture = GetCultureInfo(language);
        return value.ToString(format ?? "N0", culture);
    }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> associated with the specified language.
    /// </summary>
    /// <param name="language">The language to resolve.</param>
    /// <returns>The corresponding <see cref="CultureInfo"/>.</returns>
    public static CultureInfo GetCultureInfo(Language language)
    {
        var cultureName = CultureMap.TryGetValue(language, out var name) ? name : "en-US";
        return CultureInfo.GetCultureInfo(cultureName);
    }

    /// <summary>
    /// Loads the string table for the given language from embedded JSON resources.
    /// If the embedded resource is unavailable, returns an empty dictionary.
    /// </summary>
    /// <param name="language">The language whose strings to load.</param>
    /// <returns>A dictionary mapping resource keys to localized values.</returns>
    private static Dictionary<string, string> LoadEmbeddedStrings(Language language)
    {
        var resourceName = GetResourceName(language);

        var assembly = typeof(LocalizationService).Assembly;
        var fullyQualified = $"{assembly.GetName().Name}.Resources.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullyQualified);
        if (stream is null)
            return new Dictionary<string, string>();

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Maps a <see cref="Language"/> enum value to its embedded resource file name.
    /// </summary>
    /// <param name="language">The language to map.</param>
    /// <returns>The resource file name (e.g. "en.json").</returns>
    private static string GetResourceName(Language language)
    {
        return language switch
        {
            Language.English => "en.json",
            Language.Spanish => "es.json",
            Language.French => "fr.json",
            Language.German => "de.json",
            Language.Japanese => "ja.json",
            Language.Portuguese => "pt.json",
            Language.Arabic => "ar.json",
            Language.Hindi => "hi.json",
            Language.Chinese => "zh.json",
            _ => "en.json",
        };
    }
}
