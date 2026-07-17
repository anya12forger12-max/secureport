using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.Core.Interfaces;
using SecurePort.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecurePort.UI.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService? _configurationService;
    private readonly IThemeService? _themeService;
    private readonly ILocalizationService? _localizationService;
    private readonly IDialogService? _dialogService;
    private AppConfiguration? _originalConfiguration;

    [ObservableProperty]
    private ThemeMode _selectedTheme = ThemeMode.Dark;

    [ObservableProperty]
    private Language _selectedLanguage = Language.English;

    [ObservableProperty]
    private double _fontSize = 14.0;

    [ObservableProperty]
    private int _defaultTimeout = 3000;

    [ObservableProperty]
    private int _defaultMaxConcurrent = 100;

    [ObservableProperty]
    private int _historyRetentionDays = 90;

    [ObservableProperty]
    private bool _autoDelete;

    [ObservableProperty]
    private bool _encryptionEnabled = true;

    [ObservableProperty]
    private LogLevel _logLevel = LogLevel.Info;

    [ObservableProperty]
    private int _logRetentionDays = 30;

    public IReadOnlyList<ThemeMode> AvailableThemes { get; }
    public IReadOnlyList<Language> AvailableLanguages { get; }

    public SettingsViewModel(
        IConfigurationService? configurationService,
        IThemeService? themeService,
        ILocalizationService? localizationService,
        IDialogService? dialogService)
    {
        _configurationService = configurationService;
        _themeService = themeService;
        _localizationService = localizationService;
        _dialogService = dialogService;

        AvailableThemes = _themeService?.GetAvailableThemes()
            ?? Enum.GetValues<ThemeMode>().ToArray();

        AvailableLanguages = _localizationService?.GetAvailableLanguages()
            ?? Enum.GetValues<Language>().ToArray();

        _ = LoadConfigurationAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_configurationService is null)
            return;

        try
        {
            var ct = GetCancellationToken();

            var config = new AppConfiguration
            {
                Scan = new ScanSettings
                {
                    DefaultTimeout = TimeSpan.FromMilliseconds(DefaultTimeout),
                    DefaultMaxConcurrent = DefaultMaxConcurrent,
                    DefaultPortRangeStart = 1,
                    DefaultPortRangeEnd = 1024
                },
                UI = new UISettings
                {
                    Theme = SelectedTheme,
                    Language = SelectedLanguage,
                    FontSize = FontSize,
                    HighContrast = SelectedTheme == ThemeMode.HighContrast,
                    ReducedMotion = false,
                    ScreenReaderMode = false
                },
                Storage = new StorageSettings
                {
                    HistoryRetentionDays = HistoryRetentionDays,
                    AutoDelete = AutoDelete,
                    EncryptionEnabled = EncryptionEnabled
                },
                Log = new LogSettings
                {
                    Level = LogLevel,
                    RetentionDays = LogRetentionDays
                }
            };

            await _configurationService.SaveConfigurationAsync(config, ct);

            if (_themeService is not null)
            {
                await _themeService.SetThemeAsync(SelectedTheme);
            }

            if (_localizationService is not null)
            {
                await _localizationService.SetLanguageAsync(SelectedLanguage);
            }

            _originalConfiguration = config;

            if (_dialogService is not null)
            {
                await _dialogService.ShowMessageAsync("Settings", "Settings saved successfully.");
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }

    [RelayCommand]
    private async Task ResetDefaultsAsync()
    {
        if (_dialogService is not null)
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Reset Settings",
                "Reset all settings to their default values?");

            if (!confirmed)
                return;
        }

        if (_configurationService is not null)
        {
            try
            {
                var ct = GetCancellationToken();
                var defaults = await _configurationService.ResetToDefaultsAsync(ct);
                ApplyConfiguration(defaults);
            }
            catch (OperationCanceledException)
            {
                // Expected during disposal
            }
        }
        else
        {
            SelectedTheme = ThemeMode.Dark;
            SelectedLanguage = Language.English;
            FontSize = 14.0;
            DefaultTimeout = 3000;
            DefaultMaxConcurrent = 100;
            HistoryRetentionDays = 90;
            AutoDelete = false;
            EncryptionEnabled = true;
            LogLevel = LogLevel.Info;
            LogRetentionDays = 30;
        }
    }

    private async Task LoadConfigurationAsync()
    {
        if (_configurationService is null)
            return;

        try
        {
            var ct = GetCancellationToken();
            var config = await _configurationService.GetConfigurationAsync(ct);
            ApplyConfiguration(config);
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }

    private void ApplyConfiguration(AppConfiguration config)
    {
        SelectedTheme = config.UI.Theme;
        SelectedLanguage = config.UI.Language;
        FontSize = config.UI.FontSize;
        DefaultTimeout = (int)config.Scan.DefaultTimeout.TotalMilliseconds;
        DefaultMaxConcurrent = config.Scan.DefaultMaxConcurrent;
        HistoryRetentionDays = config.Storage.HistoryRetentionDays;
        AutoDelete = config.Storage.AutoDelete;
        EncryptionEnabled = config.Storage.EncryptionEnabled;
        LogLevel = config.Log.Level;
        LogRetentionDays = config.Log.RetentionDays;
    }
}
