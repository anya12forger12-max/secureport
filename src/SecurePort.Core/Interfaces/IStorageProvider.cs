namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides generic key-value storage with CRUD operations.
/// </summary>
/// <typeparam name="T">The type of value stored.</typeparam>
public interface IStorageProvider<T>
{
    /// <summary>
    /// Retrieves a value by its key.
    /// </summary>
    /// <param name="key">The unique key identifying the stored value.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The stored value, or null if not found.</returns>
    Task<T?> GetAsync(string key, CancellationToken ct);

    /// <summary>
    /// Stores a value with the specified key, overwriting any existing value.
    /// </summary>
    /// <param name="key">The unique key identifying the stored value.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when the value has been persisted.</returns>
    Task SetAsync(string key, T value, CancellationToken ct);

    /// <summary>
    /// Removes a value by its key.
    /// </summary>
    /// <param name="key">The unique key identifying the stored value.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the value was removed; false if the key did not exist.</returns>
    Task<bool> RemoveAsync(string key, CancellationToken ct);

    /// <summary>
    /// Determines whether a key exists in the storage.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken ct);

    /// <summary>
    /// Retrieves all stored values.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A read-only list of all stored values.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Removes all stored values from the storage.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when the storage has been cleared.</returns>
    Task ClearAsync(CancellationToken ct);
}
