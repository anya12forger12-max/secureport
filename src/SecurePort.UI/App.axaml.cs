using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SecurePort.Core.Interfaces;
using SecurePort.UI.Services;
using SecurePort.UI.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecurePort.UI;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    private IThemeService? _themeService;
    private ILocalizationService? _localizationService;
    private IConfigurationService? _configurationService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = InitializeServices();
        ServiceProvider = services;

        _themeService = services.GetService(typeof(IThemeService)) as IThemeService;
        _localizationService = services.GetService(typeof(ILocalizationService)) as ILocalizationService;
        _configurationService = services.GetService(typeof(IConfigurationService)) as IConfigurationService;

        if (_themeService is not null)
        {
            _themeService.ThemeChanged += OnThemeChanged;
        }

        if (_configurationService is not null)
        {
            _configurationService.ConfigurationChanged += OnConfigurationChanged;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = new MainViewModel(
                services,
                _themeService,
                _localizationService,
                _configurationService);

            desktop.MainWindow = new Views.MainWindow
            {
                DataContext = mainViewModel
            };

            desktop.Exit += OnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider InitializeServices()
    {
        var navigationService = new NavigationService();
        var dialogService = new DialogService();

        return new SecurePortServiceCollection(navigationService, dialogService)
            .BuildServiceProvider();
    }

    private void OnThemeChanged(object? sender, Core.Enums.ThemeMode e)
    {
        var variant = e switch
        {
            Core.Enums.ThemeMode.Light => Avalonia.Controls.WindowTitleBarBrush.Light,
            _ => Avalonia.Controls.WindowTitleBarBrush.Default
        };

        RequestedThemeVariant = e switch
        {
            Core.Enums.ThemeMode.Light => ThemeVariant.Light,
            Core.Enums.ThemeMode.HighContrast => ThemeVariant.Dark,
            Core.Enums.ThemeMode.System => ThemeVariant.Default,
            _ => ThemeVariant.Dark
        };
    }

    private void OnConfigurationChanged(object? sender, Core.Models.AppConfiguration e)
    {
        if (_themeService is not null)
        {
            _ = _themeService.SetThemeAsync(e.UI.Theme);
        }
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_themeService is not null)
        {
            _themeService.ThemeChanged -= OnThemeChanged;
        }

        if (_configurationService is not null)
        {
            _configurationService.ConfigurationChanged -= OnConfigurationChanged;
        }
    }
}

internal sealed class SecurePortServiceCollection
{
    private readonly NavigationService _navigationService;
    private readonly DialogService _dialogService;

    public SecurePortServiceCollection(NavigationService navigationService, DialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    public IServiceProvider BuildServiceProvider()
    {
        return new SecureServiceProvider(
            _navigationService,
            _dialogService);
    }
}

internal sealed class SecureServiceProvider : IServiceProvider
{
    private readonly NavigationService _navigationService;
    private readonly DialogService _dialogService;

    public SecureServiceProvider(NavigationService navigationService, DialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(NavigationService) || serviceType == typeof(INavigationService))
            return _navigationService;

        if (serviceType == typeof(DialogService) || serviceType == typeof(IDialogService))
            return _dialogService;

        return null;
    }
}

internal static class ServiceProviderExtensions
{
    public static T? GetService<T>(this IServiceProvider provider) where T : class
    {
        return provider.GetService(typeof(T)) as T;
    }
}
