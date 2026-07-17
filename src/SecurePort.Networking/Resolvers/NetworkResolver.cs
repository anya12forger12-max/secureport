using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SecurePort.Core.Interfaces;
using SecurePort.Networking.Exceptions;

namespace SecurePort.Networking.Resolvers;

/// <summary>
/// Entry in the DNS cache that stores resolved addresses alongside their expiration time.
/// </summary>
internal sealed record DnsCacheEntry(IReadOnlyList<IPAddress> Addresses, DateTime ExpiresAt);

/// <summary>
/// Resolves hostnames to IP addresses with TTL-based caching and reverse DNS support.
/// Implements <see cref="INetworkResolver"/> from the Core layer.
/// </summary>
public sealed class NetworkResolver : INetworkResolver
{
    private readonly ConcurrentDictionary<string, DnsCacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _ttl;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkResolver"/> class
    /// with the specified cache time-to-live.
    /// </summary>
    /// <param name="ttl">How long resolved entries remain valid in the cache.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ttl"/> is negative.</exception>
    public NetworkResolver(TimeSpan ttl)
    {
        if (ttl < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(ttl), "TTL must not be negative.");

        _ttl = ttl;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkResolver"/> class
    /// with a default cache TTL of five minutes.
    /// </summary>
    public NetworkResolver() : this(TimeSpan.FromMinutes(5))
    {
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IPAddress>> ResolveHostAsync(string host, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new NetworkException("Hostname must not be null or empty.");

        if (_cache.TryGetValue(host, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            return cached.Addresses;

        IReadOnlyList<IPAddress> addresses;
        try
        {
            var resolved = await Dns.GetHostAddressesAsync(host, ct);
            if (resolved.Length == 0)
                throw new NetworkException($"DNS resolution for '{host}' returned no addresses.");

            addresses = Array.AsReadOnly(resolved);
        }
        catch (SocketException ex)
        {
            throw new NetworkException($"Failed to resolve hostname '{host}': {ex.SocketErrorCode}.", ex);
        }
        catch (NetworkException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new NetworkException($"Unexpected error resolving hostname '{host}'.", ex);
        }

        _cache[host] = new DnsCacheEntry(addresses, DateTime.UtcNow + _ttl);
        return addresses;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This method cannot use the <c>async</c> keyword because the interface requires
    /// an <c>out</c> parameter. It delegates to <see cref="ResolveHostAsync"/> and
    /// unwraps the result synchronously, which is safe in library code without a
    /// <see cref="System.Threading.SynchronizationContext"/>.
    /// </remarks>
    public Task<bool> TryResolveHostAsync(
        string host,
        out IReadOnlyList<IPAddress> addresses,
        CancellationToken ct)
    {
        try
        {
            addresses = ResolveHostAsync(host, ct).GetAwaiter().GetResult();
            return Task.FromResult(true);
        }
        catch
        {
            addresses = Array.Empty<IPAddress>();
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<string?> ReverseLookupAsync(IPAddress address, CancellationToken ct)
    {
        if (address is null)
            throw new ArgumentNullException(nameof(address));

        try
        {
            var entry = await Dns.GetHostEntryAsync(address.ToString(), ct);
            return entry.HostName;
        }
        catch (SocketException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Convenience method that resolves a hostname to a single <see cref="IPAddress"/>.
    /// Returns the first resolved address. Throws if no addresses are returned.
    /// </summary>
    /// <param name="hostname">The hostname or IP address to resolve.</param>
    /// <param name="ct">A cancellation token to cancel the resolution.</param>
    /// <returns>The first resolved IP address.</returns>
    /// <exception cref="NetworkException">Resolution failed or returned no addresses.</exception>
    public async Task<IPAddress> ResolveAsync(string hostname, CancellationToken ct)
    {
        var addresses = await ResolveHostAsync(hostname, ct);
        return addresses[0];
    }

    /// <summary>
    /// Removes all expired entries from the internal cache.
    /// </summary>
    public void PurgeExpiredEntries()
    {
        var now = DateTime.UtcNow;
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt <= now)
                _cache.TryRemove(key, out _);
        }
    }
}
