using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SecurePort.UI.ViewModels;
using System;

namespace SecurePort.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateNavSelection();

        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        KeyDown += OnKeyDown;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
            vm.Dispose();
        }

        Loaded -= OnLoaded;
        Closing -= OnClosing;
        KeyDown -= OnKeyDown;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentViewName))
        {
            UpdateNavSelection();
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is MainViewModel vm && vm.IsSidebarOpen)
        {
            vm.ToggleSidebarCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void UpdateNavSelection()
    {
        if (DataContext is not MainViewModel vm)
            return;

        HighlightNavItem("Dashboard", vm.CurrentViewName == "Dashboard");
        HighlightNavItem("Scan", vm.CurrentViewName == "Scan");
        HighlightNavItem("History", vm.CurrentViewName == "History");
        HighlightNavItem("Settings", vm.CurrentViewName == "Settings");
        HighlightNavItem("About", vm.CurrentViewName == "About");
    }

    private void HighlightNavItem(string tagName, bool isSelected)
    {
        if (this.FindControl<Border>(tagName) is not Border navItem)
            return;

        if (isSelected)
        {
            navItem.Classes.Add("selected");
            SetAccentForeground(navItem, true);
        }
        else
        {
            navItem.Classes.Remove("selected");
            SetAccentForeground(navItem, false);
        }
    }

    private static void SetAccentForeground(Border navItem, bool accent)
    {
        if (navItem.Child is Grid grid)
        {
            foreach (var child in grid.Children)
            {
                if (child is StackPanel stack)
                {
                    foreach (var textBlock in stack.Children)
                    {
                        if (textBlock is Avalonia.Controls.TextBlock tb)
                        {
                            tb.Foreground = accent
                                ? Avalonia.Media.Brushes.White
                                : (Avalonia.Media.IBrush?)Avalonia.Media.Brushes.White;
                        }
                    }
                }
                else if (child is Avalonia.Controls.TextBlock icon)
                {
                    icon.Foreground = accent
                        ? Avalonia.Media.Brushes.White
                        : (Avalonia.Media.IBrush?)new Avalonia.Media.SolidColorBrush(
                            Avalonia.Media.Color.Parse("#a0aec0"));
                }
            }
        }
    }

    private void OnDashboardTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToDashboardCommand.Execute(null);
    }

    private void OnScanTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToScanCommand.Execute(null);
    }

    private void OnHistoryTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToHistoryCommand.Execute(null);
    }

    private void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToSettingsCommand.Execute(null);
    }

    private void OnAboutTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.NavigateToAboutCommand.Execute(null);
    }
}
