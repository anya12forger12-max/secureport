using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.UI.Services;
using System;
using System.Threading;

namespace SecurePort.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private readonly IThemeService? _themeService;
    private readonly ILocalizationService? _localizationService;
    private readonly IConfigurationService? _configurationService;

    private ViewModelBase? _dashboardViewModel;
    private ViewModelBase? _scanViewModel;
    private ViewModelBase? _historyViewModel;
    private ViewModelBase? _settingsViewModel;
    private ViewModelBase? _aboutViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private bool _isSidebarOpen = true;

    [ObservableProperty]
    private ThemeMode _currentTheme = ThemeMode.Dark;

    [ObservableProperty]
    private string _statusBar = "Ready";

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _currentViewName = "Dashboard";

    public MainViewModel(
        IServiceProvider serviceProvider,
        IThemeService? themeService,
        ILocalizationService? localizationService,
        IConfigurationService? configurationService)
    {
        _serviceProvider = serviceProvider;
        _navigationService = serviceProvider.GetService(typeof(NavigationService)) as INavigationService
            ?? throw new InvalidOperationException("NavigationService is not registered.");
        _themeService = themeService;
        _localizationService = localizationService;
        _configurationService = configurationService;

        _navigationService.NavigationStateChanged += OnNavigationStateChanged;

        if (_themeService is not null)
        {
            _themeService.ThemeChanged += (_, theme) => CurrentTheme = theme;
        }

        NavigateToView("Dashboard");
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarOpen = !IsSidebarOpen;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        NavigateToView("Dashboard");
    }

    [RelayCommand]
    private void NavigateToScan()
    {
        NavigateToView("Scan");
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        NavigateToView("History");
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        NavigateToView("Settings");
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
        NavigateToView("About");
    }

    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private void GoForward()
    {
        _navigationService.GoForward();
    }

    private void NavigateToView(string viewName)
    {
        CurrentViewName = viewName;
        CurrentViewModel = viewName switch
        {
            "Dashboard" => GetOrCreateDashboardViewModel(),
            "Scan" => GetOrCreateScanViewModel(),
            "History" => GetOrCreateHistoryViewModel(),
            "Settings" => GetOrCreateSettingsViewModel(),
            "About" => GetOrCreateAboutViewModel(),
            _ => CurrentViewModel
        };
        _navigationService.NavigateTo(viewName);
    }

    private void OnNavigationStateChanged(object? sender, string viewName)
    {
        CurrentViewName = viewName;
        CurrentViewModel = viewName switch
        {
            "Dashboard" => GetOrCreateDashboardViewModel(),
            "Scan" => GetOrCreateScanViewModel(),
            "History" => GetOrCreateHistoryViewModel(),
            "Settings" => GetOrCreateSettingsViewModel(),
            "About" => GetOrCreateAboutViewModel(),
            _ => CurrentViewModel
        };
    }

    private ViewModelBase GetOrCreateDashboardViewModel()
    {
        if (_dashboardViewModel is null)
        {
            var historyRepo = _serviceProvider.GetService(typeof(IScanHistoryRepository)) as IScanHistoryRepository;
            _dashboardViewModel = new DashboardViewModel(historyRepo, this);
        }
        return _dashboardViewModel;
    }

    private ViewModelBase GetOrCreateScanViewModel()
    {
        if (_scanViewModel is null)
        {
            var scanner = _serviceProvider.GetService(typeof(IScanner)) as IScanner;
            var validator = _serviceProvider.GetService(typeof(ITargetValidator)) as ITargetValidator;
            var historyRepo = _serviceProvider.GetService(typeof(IScanHistoryRepository)) as IScanHistoryRepository;
            var reportGen = _serviceProvider.GetService(typeof(IReportGenerator)) as IReportGenerator;
            var dialogService = _serviceProvider.GetService(typeof(DialogService)) as IDialogService;
            var logging = _serviceProvider.GetService(typeof(ILoggingProvider)) as ILoggingProvider;
            _scanViewModel = new ScanViewModel(scanner, validator, historyRepo, reportGen, dialogService, logging, this);
        }
        return _scanViewModel;
    }

    private ViewModelBase GetOrCreateHistoryViewModel()
    {
        if (_historyViewModel is null)
        {
            var historyRepo = _serviceProvider.GetService(typeof(IScanHistoryRepository)) as IScanHistoryRepository;
            var reportGen = _serviceProvider.GetService(typeof(IReportGenerator)) as IReportGenerator;
            var dialogService = _serviceProvider.GetService(typeof(DialogService)) as IDialogService;
            _historyViewModel = new HistoryViewModel(historyRepo, reportGen, dialogService, this);
        }
        return _historyViewModel;
    }

    private ViewModelBase GetOrCreateSettingsViewModel()
    {
        if (_settingsViewModel is null)
        {
            var configService = _serviceProvider.GetService(typeof(IConfigurationService)) as IConfigurationService;
            var themeService = _serviceProvider.GetService(typeof(IThemeService)) as IThemeService;
            var localizationService = _serviceProvider.GetService(typeof(ILocalizationService)) as ILocalizationService;
            var dialogService = _serviceProvider.GetService(typeof(DialogService)) as IDialogService;
            _settingsViewModel = new SettingsViewModel(configService, themeService, localizationService, dialogService);
        }
        return _settingsViewModel;
    }

    private ViewModelBase GetOrCreateAboutViewModel()
    {
        if (_aboutViewModel is null)
        {
            _aboutViewModel = new AboutViewModel();
        }
        return _aboutViewModel;
    }

    public override void Dispose()
    {
        _navigationService.NavigationStateChanged -= OnNavigationStateChanged;
        _dashboardViewModel?.Dispose();
        _scanViewModel?.Dispose();
        _historyViewModel?.Dispose();
        _settingsViewModel?.Dispose();
        _aboutViewModel?.Dispose();
        base.Dispose();
    }
}
