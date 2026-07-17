using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecurePort.Core.Models;
using SecurePort.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecurePort.UI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IScanHistoryRepository? _historyRepository;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private int _totalScans;

    [ObservableProperty]
    private int _openPorts;

    [ObservableProperty]
    private string _lastScanDate = "Never";

    [ObservableProperty]
    private int _activeTargets;

    public ObservableCollection<ScanSession> RecentScans { get; } = new();

    public DashboardViewModel(IScanHistoryRepository? historyRepository, MainViewModel mainViewModel)
    {
        _historyRepository = historyRepository;
        _mainViewModel = mainViewModel;

        _ = LoadRecentDataAsync();
    }

    [RelayCommand]
    private void QuickScan()
    {
        _mainViewModel.NavigateToScanCommand.Execute(null);
    }

    [RelayCommand]
    private void ViewHistory()
    {
        _mainViewModel.NavigateToHistoryCommand.Execute(null);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadRecentDataAsync();
    }

    private async Task LoadRecentDataAsync()
    {
        if (_historyRepository is null)
            return;

        try
        {
            var ct = GetCancellationToken();
            var sessions = await _historyRepository.GetPageAsync(0, 10, ct);

            TotalScans = await _historyRepository.CountAsync(ct);

            RecentScans.Clear();
            int totalOpen = 0;

            foreach (var session in sessions)
            {
                RecentScans.Add(session);
                totalOpen += session.OpenPortsFound;
            }

            OpenPorts = totalOpen;

            if (sessions.Count > 0 && sessions[0].EndTime.HasValue)
            {
                LastScanDate = sessions[0].EndTime.Value.ToString("yyyy-MM-dd HH:mm");
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }
}
