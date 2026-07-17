using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace SecurePort.UI.Dialogs;

/// <summary>
/// Modal dialog requiring the user to acknowledge legal authorization before scanning.
/// </summary>
public partial class LegalConsentDialog : UserControl, INotifyPropertyChanged
{
    private bool _dialogResult;
    private bool _isOpen;

    /// <summary>
    /// Occurs when the dialog requests to be closed by the parent view.
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    /// <summary>
    /// Occurs when the user chooses to remember consent for the remainder of the session.
    /// </summary>
    public event EventHandler? RememberForSessionRequested;

    /// <summary>
    /// Gets the result of the dialog after the user interacts with it.
    /// <c>true</c> if the user accepted; <c>false</c> if cancelled.
    /// </summary>
    public bool DialogResult
    {
        get => _dialogResult;
        private set
        {
            if (_dialogResult == value) return;
            _dialogResult = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether the dialog is currently visible.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;
            _isOpen = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsVisible));
            IsVisible = value;
        }
    }

    /// <summary>
    /// Command executed when the user clicks "Accept".
    /// Sets <see cref="DialogResult"/> to <c>true</c> and closes the dialog.
    /// </summary>
    public IRelayCommand AcceptCommand { get; }

    /// <summary>
    /// Command executed when the user clicks "Cancel".
    /// Sets <see cref="DialogResult"/> to <c>false</c> and closes the dialog.
    /// </summary>
    public IRelayCommand CancelCommand { get; }

    /// <summary>
    /// Command executed when the user clicks "Remember for this Session".
    /// Sets <see cref="DialogResult"/> to <c>true</c>, raises <see cref="RememberForSessionRequested"/>,
    /// and closes the dialog.
    /// </summary>
    public IRelayCommand RememberForSessionCommand { get; }

    public LegalConsentDialog()
    {
        AcceptCommand = new RelayCommand(ExecuteAccept);
        CancelCommand = new RelayCommand(ExecuteCancel);
        RememberForSessionCommand = new RelayCommand(ExecuteRememberForSession);

        DataContext = this;
        InitializeComponent();
    }

    /// <summary>
    /// Shows the dialog by setting <see cref="IsOpen"/> to <c>true</c>.
    /// </summary>
    public void Show()
    {
        DialogResult = false;
        IsOpen = true;
    }

    /// <summary>
    /// Closes the dialog with the current <see cref="DialogResult"/>.
    /// </summary>
    public void Close()
    {
        IsOpen = false;
        CloseRequested?.Invoke(this, DialogResult);
    }

    private void ExecuteAccept()
    {
        DialogResult = true;
        Close();
    }

    private void ExecuteCancel()
    {
        DialogResult = false;
        Close();
    }

    private void ExecuteRememberForSession()
    {
        DialogResult = true;
        RememberForSessionRequested?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        ExecuteCancel();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
