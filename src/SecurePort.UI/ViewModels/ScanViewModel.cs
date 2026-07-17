using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecurePort.UI.ViewModels;

public partial class ScanViewModel : ViewModelBase
{
    private readonly Core.Interfaces.IScanner? _scanner;
    private readonly Core.Interfaces.ITargetValidator? _validator;
    private readonly Core.Interfaces.IScanHistoryRepository? _historyRepository;
    private readonly Core.Interfaces.IReportGenerator? _reportGenerator;
    private readonly IDialogService? _dialogService;
    private readonly Core.Interfaces.ILoggingProvider? _loggingProvider;
    private readonly MainViewModel _mainViewModel;
    private IDisposable? _progressSubscription;

    [ObservableProperty]
    private string _hostEntry = string.Empty;

    [ObservableProperty]
    private int _portRangeStart = 1;

    [ObservableProperty]
    private int _portRangeEnd = 1024;

    [ObservableProperty]
    private ScanType _selectedScanType = ScanType.ConnectScan;

    [ObservableProperty]
    private int _timeout = 3000;

    [ObservableProperty]
    private int _maxConcurrent = 100;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private ScanProgress? _progress;

    [ObservableProperty]
    private int _openPortCount;

    [ObservableProperty]
    private int _closedPortCount;

    [ObservableProperty]
    private int _filteredPortCount;

    [ObservableProperty]
    private ScanSession? _lastSession;

    public ObservableCollection<ScanResult> ScanResults { get; } = new();

    public ScanViewModel(
        Core.Interfaces.IScanner? scanner,
        Core.Interfaces.ITargetValidator? validator,
        Core.Interfaces.IScanHistoryRepository? historyRepository,
        Core.Interfaces.IReportGenerator? reportGenerator,
        IDialogService? dialogService,
        Core.Interfaces.ILoggingProvider? loggingProvider,
        MainViewModel mainViewModel)
    {
        _scanner = scanner;
        _validator = validator;
        _historyRepository = historyRepository;
        _reportGenerator = reportGenerator;
        _dialogService = dialogService;
        _loggingProvider = loggingProvider;
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning)
            return;

        var hostError = _validator?.ValidateHost(HostEntry);
        if (hostError is not null)
        {
            await ShowErrorAsync("Invalid Target", hostError);
            return;
        }

        var portError = _validator?.ValidatePortRange(PortRangeStart, PortRangeEnd);
        if (portError is not null)
        {
            await ShowErrorAsync("Invalid Port Range", portError);
            return;
        }

        if (_dialogService is not null)
        {
            var consented = await _dialogService.ShowLegalConsentDialog();
            if (!consented)
                return;
        }

        IsScanning = true;
        ScanResults.Clear();
        OpenPortCount = 0;
        ClosedPortCount = 0;
        FilteredPortCount = 0;
        _mainViewModel.IsScanning = true;
        _mainViewModel.StatusBar = $"Scanning {HostEntry}...";

        try
        {
            var target = new ScanTarget
            {
                Host = HostEntry,
                PortStart = PortRangeStart,
                PortEnd = PortRangeEnd,
                Protocol = ProtocolType.TCP,
                ScanType = SelectedScanType,
                Timeout = TimeSpan.FromMilliseconds(Timeout),
                MaxConcurrentConnections = MaxConcurrent,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (_scanner is not null)
            {
                SubscribeToProgress(_scanner);

                var ct = GetCancellationToken();
                var session = await _scanner.StartScanAsync(target, ct);

                LastSession = session;
                _mainViewModel.StatusBar = $"Scan complete - {session.OpenPortsFound} open ports found";

                foreach (var result in session.Results)
                {
                    ScanResults.Add(result);
                }

                OpenPortCount = session.OpenPortsFound;
                ClosedPortCount = session.ClosedPortsFound;
                FilteredPortCount = session.FilteredPortsFound;

                if (_historyRepository is not null)
                {
                    await _historyRepository.SaveAsync(session, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _mainViewModel.StatusBar = "Scan cancelled";
        }
        catch (Exception ex)
        {
            _loggingProvider?.Error(ex, "Scan failed for host {Host}", HostEntry);
            _mainViewModel.StatusBar = $"Scan failed: {ex.Message}";
            await ShowErrorAsync("Scan Error", ex.Message);
        }
        finally
        {
            IsScanning = false;
            _mainViewModel.IsScanning = false;
            _progressSubscription?.Dispose();
        }
    }

    [RelayCommand]
    private async Task CancelScanAsync()
    {
        if (_scanner is not null && _scanner.IsScanning)
        {
            await _scanner.CancelScanAsync();
            _mainViewModel.StatusBar = "Cancelling scan...";
        }
    }

    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        if (LastSession is null || _reportGenerator is null || _dialogService is null)
            return;

        var consented = await _dialogService.ShowConfirmationAsync(
            "Export Results",
            "Export the scan results to a file?");

        if (!consented)
            return;

        try
        {
            var ct = GetCancellationToken();
            var options = new ExportOptions
            {
                Format = ExportFormat.JSON,
                IncludeClosedPorts = true,
                IncludeTimestamps = true,
                FilePath = $"scan_{LastSession.Id:N}.json",
                IncludeScanMetadata = true
            };

            var filePath = await _reportGenerator.GenerateReportAsync(LastSession, options, ct);
            _mainViewModel.StatusBar = $"Report exported to {filePath}";
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
        catch (Exception ex)
        {
            _loggingProvider?.Error(ex, "Failed to export results");
            _mainViewModel.StatusBar = $"Export failed: {ex.Message}";
        }
    }

    private void SubscribeToProgress(Core.Interfaces.IScanner scanner)
    {
        _progressSubscription?.Dispose();
        _progressSubscription = scanner.ProgressChanged.Subscribe(OnProgressChanged);
    }

    private void OnProgressChanged(ScanProgress progress)
    {
        Progress = progress;
        OpenPortCount = progress.OpenPorts;
        _mainViewModel.StatusBar = $"Scanning port {progress.CurrentPort} ({progress.PercentageComplete:F1}%)";
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        if (_dialogService is not null)
        {
            await _dialogService.ShowMessageAsync(title, message);
        }
    }

    public override void Dispose()
    {
        _progressSubscription?.Dispose();
        _progressSubscription = null;
        base.Dispose();
    }
}
