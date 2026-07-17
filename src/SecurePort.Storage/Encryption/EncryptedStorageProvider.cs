using System.Text;
using System.Text.Json;
using SecurePort.Core.Interfaces;
using SecurePort.Security.Encryption;

namespace SecurePort.Storage.Encryption;

/// <summary>
/// Decorator that wraps an <see cref="IStorageProvider{T}"/> with transparent
/// AES-256-GCM encryption via <see cref="IEncryptor"/>. Values are encrypted on
/// write and decrypted on read, so callers interact with plaintext objects.
/// </summary>
/// <typeparam name="T">The type of value stored.</typeparam>
public sealed class EncryptedStorageProvider<T> : IStorageProvider<T>
{
    private readonly IStorageProvider<T> _inner;
    private readonly IEncryptor _encryptor;
    private readonly byte[] _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptedStorageProvider{T}"/> class.
    /// </summary>
    /// <param name="inner">The underlying storage provider to decorate.</param>
    /// <param name="encryptor">The encryptor used for encryption and decryption.</param>
    /// <param name="key">The 256-bit encryption key.</param>
    public EncryptedStorageProvider(IStorageProvider<T> inner, IEncryptor encryptor, byte[] key)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        _key = key ?? throw new ArgumentNullException(nameof(key));
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync(string key, CancellationToken ct)
    {
        var stored = await _inner.GetAsync(key, ct);
        if (stored is null)
            return default;

        var ciphertext = Convert.FromBase64String(stored!.ToString()!);
        var plaintext = _encryptor.Decrypt(ciphertext, _key);
        var json = Encoding.UTF8.GetString(plaintext);

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <inheritdoc/>
    public async Task SetAsync(string key, T value, CancellationToken ct)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var json = JsonSerializer.Serialize(value, JsonOptions);
        var encrypted = _encryptor.Encrypt(Encoding.UTF8.GetBytes(json), _key);
        var encoded = Convert.ToBase64String(encrypted);

        await _inner.SetAsync(key, (T)(object)encoded, ct);
    }

    /// <inheritdoc/>
    public Task<bool> RemoveAsync(string key, CancellationToken ct) =>
        _inner.RemoveAsync(key, ct);

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string key, CancellationToken ct) =>
        _inner.ExistsAsync(key, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct)
    {
        var raw = await _inner.GetAllAsync(ct);
        var results = new List<T>(raw.Count);

        foreach (var item in raw)
        {
            ct.ThrowIfCancellationRequested();

            var ciphertext = Convert.FromBase64String(item!.ToString()!);
            var plaintext = _encryptor.Decrypt(ciphertext, _key);
            var json = Encoding.UTF8.GetString(plaintext);
            var deserialized = JsonSerializer.Deserialize<T>(json, JsonOptions);

            if (deserialized is not null)
                results.Add(deserialized);
        }

        return results.AsReadOnly();
    }

    /// <inheritdoc/>
    public Task ClearAsync(CancellationToken ct) =>
        _inner.ClearAsync(ct);

    /// <summary>
    /// Shared JSON serialization options for this provider.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}
