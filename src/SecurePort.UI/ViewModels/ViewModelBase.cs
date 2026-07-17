using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecurePort.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _isBusy;
    private bool _disposed;

    public bool IsBusy
    {
        get => _isBusy;
        protected set => SetProperty(ref _isBusy, value);
    }

    protected CancellationTokenSource? Cts { get; private set; }

    protected CancellationToken GetCancellationToken()
    {
        Cts?.Cancel();
        Cts = new CancellationTokenSource();
        return Cts.Token;
    }

    protected bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName ?? string.Empty);
        return true;
    }

    public virtual void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Cts?.Cancel();
        Cts?.Dispose();
        Cts = null;

        GC.SuppressFinalize(this);
    }
}
