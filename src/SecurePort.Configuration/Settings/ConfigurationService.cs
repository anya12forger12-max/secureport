using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Configuration.Settings;

/// <summary>
/// Implements <see cref="IConfigurationService"/> by loading and persisting
/// <see cref="AppConfiguration"/> as a JSON file via <see cref="IStorageProvider{T}"/>.
/// Provides thread-safe access, default creation, and change notifications.
/// </summary>
public sealed class ConfigurationService : IConfigurationService, IDisposable
{
    private const string ConfigurationKey = "app-configuration";

    private readonly IStorageProvider<AppConfiguration> _storage;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private AppConfiguration? _current;
    private bool _disposed;

    /// <inheritdoc/>
    public event EventHandler<AppConfiguration>? ConfigurationChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="storage">The storage provider used to persist configuration.</param>
    public ConfigurationService(IStorageProvider<AppConfiguration> storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc/>
    public async Task<AppConfiguration> GetConfigurationAsync(CancellationToken ct)
    {
        ThrowIfDisposed();

        if (_current is not null)
            return _current;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_current is not null)
                return _current;

            _current = await _storage.GetAsync(ConfigurationKey, ct) ?? CreateDefault();
            return _current;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SaveConfigurationAsync(AppConfiguration configuration, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        await _semaphore.WaitAsync(ct);
        try
        {
            await _storage.SetAsync(ConfigurationKey, configuration, ct);
            _current = configuration;
            OnConfigurationChanged(configuration);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task UpdateConfigurationAsync(Func<AppConfiguration, AppConfiguration> update, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (update is null)
            throw new ArgumentNullException(nameof(update));

        await _semaphore.WaitAsync(ct);
        try
        {
            var current = _current ?? await _storage.GetAsync(ConfigurationKey, ct) ?? CreateDefault();
            var updated = update(current);

            await _storage.SetAsync(ConfigurationKey, updated, ct);
            _current = updated;
            OnConfigurationChanged(updated);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<AppConfiguration> ResetToDefaultsAsync(CancellationToken ct)
    {
        ThrowIfDisposed();

        var defaults = CreateDefault();
        await SaveConfigurationAsync(defaults, ct);
        return defaults;
    }

    /// <summary>
    /// Raises the <see cref="ConfigurationChanged"/> event.
    /// </summary>
    /// <param name="configuration">The new configuration value.</param>
    private void OnConfigurationChanged(AppConfiguration configuration) =>
        ConfigurationChanged?.Invoke(this, configuration);

    /// <summary>
    /// Creates the default application configuration.
    /// </summary>
    /// <returns>A new <see cref="AppConfiguration"/> with sensible defaults.</returns>
    private static AppConfiguration CreateDefault() => new()
    {
        Scan = new ScanSettings
        {
            DefaultTimeout = TimeSpan.FromSeconds(5),
            DefaultMaxConcurrent = 100,
            DefaultPortRangeStart = 1,
            DefaultPortRangeEnd = 1024
        },
        UI = new UISettings
        {
            Theme = ThemeMode.Dark,
            Language = Language.English,
            FontSize = 14.0,
            HighContrast = false,
            ReducedMotion = false,
            ScreenReaderMode = false
        },
        Storage = new StorageSettings
        {
            HistoryRetentionDays = 90,
            AutoDelete = true,
            EncryptionEnabled = false
        },
        Log = new LogSettings
        {
            Level = LogLevel.Info,
            RetentionDays = 30
        }
    };

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if this instance has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _semaphore.Dispose();
    }
}
