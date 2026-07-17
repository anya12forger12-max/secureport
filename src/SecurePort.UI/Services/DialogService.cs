using System;
using System.Threading.Tasks;

namespace SecurePort.UI.Services;

public interface IDialogService
{
    event EventHandler? DialogOpen;
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task<bool> ShowLegalConsentDialog();
}

public sealed class DialogService : IDialogService
{
    private Func<string, string, Task>? _messageHandler;
    private Func<string, string, Task<bool>>? _confirmationHandler;
    private Func<Task<bool>>? _legalConsentHandler;

    public event EventHandler? DialogOpen;

    public void RegisterMessageHandler(Func<string, string, Task> handler)
    {
        _messageHandler = handler;
    }

    public void RegisterConfirmationHandler(Func<string, string, Task<bool>> handler)
    {
        _confirmationHandler = handler;
    }

    public void RegisterLegalConsentHandler(Func<Task<bool>> handler)
    {
        _legalConsentHandler = handler;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        DialogOpen?.Invoke(this, EventArgs.Empty);

        if (_messageHandler is not null)
        {
            await _messageHandler(title, message);
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        DialogOpen?.Invoke(this, EventArgs.Empty);

        if (_confirmationHandler is not null)
        {
            return await _confirmationHandler(title, message);
        }

        return false;
    }

    public async Task<bool> ShowLegalConsentDialog()
    {
        DialogOpen?.Invoke(this, EventArgs.Empty);

        if (_legalConsentHandler is not null)
        {
            return await _legalConsentHandler();
        }

        return false;
    }
}
