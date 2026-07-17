using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.Core.Interfaces;
using SecurePort.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecurePort.UI.ViewModels;

public partial class HistoryViewModel : ViewModelBase
{
    private readonly IScanHistoryRepository? _historyRepository;
    private readonly IReportGenerator? _reportGenerator;
    private readonly IDialogService? _dialogService;
    private readonly MainViewModel _mainViewModel;
    private IReadOnlyList<ScanSession> _allSessions = Array.Empty<ScanSession>();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ScanStatus? _filterStatus;

    [ObservableProperty]
    private ScanSession? _selectedScan;

    public ObservableCollection<ScanSession> ScanHistory { get; } = new();

    public HistoryViewModel(
        IScanHistoryRepository? historyRepository,
        IReportGenerator? reportGenerator,
        IDialogService? dialogService,
        MainViewModel mainViewModel)
    {
        _historyRepository = historyRepository;
        _reportGenerator = reportGenerator;
        _dialogService = dialogService;
        _mainViewModel = mainViewModel;

        _ = LoadHistoryAsync();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnFilterStatusChanged(ScanStatus? value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        if (_historyRepository is null)
            return;

        try
        {
            var ct = GetCancellationToken();
            _allSessions = await _historyRepository.GetAllAsync(ct);
            ApplyFilters();
            _mainViewModel.StatusBar = $"Loaded {_allSessions.Count} scan sessions";
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }

    [RelayCommand]
    private async Task DeleteScanAsync()
    {
        if (SelectedScan is null || _historyRepository is null || _dialogService is null)
            return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Delete Scan",
            $"Delete the scan of '{SelectedScan.Target.Host}' from {SelectedScan.StartTime:yyyy-MM-dd HH:mm}?");

        if (!confirmed)
            return;

        try
        {
            var ct = GetCancellationToken();
            await _historyRepository.DeleteAsync(SelectedScan.Id, ct);
            await LoadHistoryAsync();
            _mainViewModel.StatusBar = "Scan deleted";
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
    }

    [RelayCommand]
    private async Task ExportScanAsync()
    {
        if (SelectedScan is null || _reportGenerator is null || _dialogService is null)
            return;

        var consented = await _dialogService.ShowConfirmationAsync(
            "Export Scan",
            $"Export scan results for '{SelectedScan.Target.Host}'?");

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
                FilePath = $"scan_{SelectedScan.Id:N}.json",
                IncludeScanMetadata = true
            };

            var filePath = await _reportGenerator.GenerateReportAsync(SelectedScan, options, ct);
            _mainViewModel.StatusBar = $"Report exported to {filePath}";
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
        catch (Exception ex)
        {
            _mainViewModel.StatusBar = $"Export failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedScan is null)
            return;

        _mainViewModel.StatusBar = $"Viewing details for {SelectedScan.Target.Host} ({SelectedScan.OpenPortsFound} open ports)";
    }

    private void ApplyFilters()
    {
        ScanHistory.Clear();

        var filtered = _allSessions.AsEnumerable();

        if (FilterStatus.HasValue)
        {
            filtered = filtered.Where(s => s.Status == FilterStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = filtered.Where(s =>
                s.Target.Host.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var session in filtered)
        {
            ScanHistory.Add(session);
        }
    }
}
