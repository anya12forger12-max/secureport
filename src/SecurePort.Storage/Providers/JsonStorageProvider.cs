using System.Text.Json;
using SecurePort.Core.Interfaces;

namespace SecurePort.Storage.Providers;

/// <summary>
/// Generic file-based JSON storage provider implementing <see cref="IStorageProvider{T}"/>.
/// Each item is persisted as an individual JSON file using the key as the filename.
/// All operations are thread-safe via a <see cref="SemaphoreSlim"/>.
/// </summary>
/// <typeparam name="T">The type of value stored.</typeparam>
public sealed class JsonStorageProvider<T> : IStorageProvider<T>, IDisposable
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _options;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// The file extension used for stored JSON files.
    /// </summary>
    private const string FileExtension = ".json";

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStorageProvider{T}"/> class.
    /// </summary>
    /// <param name="directory">The directory path where JSON files are stored. Created if it does not exist.</param>
    public JsonStorageProvider(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory must not be null or empty.", nameof(directory));

        _directory = directory;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        Directory.CreateDirectory(_directory);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync(string key, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        var filePath = GetFilePath(key);

        await _semaphore.WaitAsync(ct);
        try
        {
            if (!File.Exists(filePath))
                return default;

            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<T>(json, _options);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, T value, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var filePath = GetFilePath(key);
        var json = JsonSerializer.Serialize(value, _options);

        await _semaphore.WaitAsync(ct);
        try
        {
            await File.WriteAllTextAsync(filePath, json, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        var filePath = GetFilePath(key);

        await _semaphore.WaitAsync(ct);
        try
        {
            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        ThrowIfDisposed();
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be null or empty.", nameof(key));

        await _semaphore.WaitAsync(ct);
        try
        {
            return File.Exists(GetFilePath(key));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct)
    {
        ThrowIfDisposed();

        await _semaphore.WaitAsync(ct);
        try
        {
            var files = Directory.GetFiles(_directory, $"*{FileExtension}");
            var results = new List<T>(files.Length);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();

                var json = await File.ReadAllTextAsync(file, ct);
                var item = JsonSerializer.Deserialize<T>(json, _options);
                if (item is not null)
                    results.Add(item);
            }

            return results.AsReadOnly();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task ClearAsync(CancellationToken ct)
    {
        ThrowIfDisposed();

        await _semaphore.WaitAsync(ct);
        try
        {
            var files = Directory.GetFiles(_directory, $"*{FileExtension}");
            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                File.Delete(file);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Maps a storage key to its corresponding file path.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <returns>The full file path for the key.</returns>
    private string GetFilePath(string key) =>
        Path.Combine(_directory, $"{key}{FileExtension}");

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
