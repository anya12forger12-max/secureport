using System;
using System.Collections.Generic;

namespace SecurePort.UI.Services;

public interface INavigationService
{
    string CurrentView { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    event EventHandler<string>? NavigationStateChanged;
    void NavigateTo(string viewName);
    void GoBack();
    void GoForward();
}

public sealed class NavigationService : INavigationService
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentView = "Dashboard";

    public string CurrentView => _currentView;

    public bool CanGoBack => _backStack.Count > 0;

    public bool CanGoForward => _forwardStack.Count > 0;

    public event EventHandler<string>? NavigationStateChanged;

    public void NavigateTo(string viewName)
    {
        if (string.Equals(_currentView, viewName, StringComparison.Ordinal))
            return;

        _backStack.Push(_currentView);
        _forwardStack.Clear();
        _currentView = viewName;

        NavigationStateChanged?.Invoke(this, viewName);
    }

    public void GoBack()
    {
        if (_backStack.Count == 0)
            return;

        _forwardStack.Push(_currentView);
        _currentView = _backStack.Pop();

        NavigationStateChanged?.Invoke(this, _currentView);
    }

    public void GoForward()
    {
        if (_forwardStack.Count == 0)
            return;

        _backStack.Push(_currentView);
        _currentView = _forwardStack.Pop();

        NavigationStateChanged?.Invoke(this, _currentView);
    }
}
