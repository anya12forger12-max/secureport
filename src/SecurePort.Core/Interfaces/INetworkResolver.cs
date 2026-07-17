using System.Net;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Resolves hostnames to their associated IP addresses.
/// </summary>
public interface INetworkResolver
{
    /// <summary>
    /// Resolves a hostname to one or more IP addresses.
    /// </summary>
    /// <param name="host">The hostname or IP address to resolve.</param>
    /// <param name="ct">A cancellation token to cancel the resolution.</param>
    /// <returns>A collection of resolved IP addresses.</returns>
    Task<IReadOnlyList<IPAddress>> ResolveHostAsync(string host, CancellationToken ct);

    /// <summary>
    /// Attempts to resolve a hostname to an IP address, returning false on failure.
    /// </summary>
    /// <param name="host">The hostname to resolve.</param>
    /// <param name="addresses">The resolved IP addresses, or an empty list on failure.</param>
    /// <param name="ct">A cancellation token to cancel the resolution.</param>
    /// <returns>True if resolution succeeded; otherwise, false.</returns>
    Task<bool> TryResolveHostAsync(string host, out IReadOnlyList<IPAddress> addresses, CancellationToken ct);

    /// <summary>
    /// Performs a reverse DNS lookup on an IP address to find its hostname.
    /// </summary>
    /// <param name="address">The IP address to look up.</param>
    /// <param name="ct">A cancellation token to cancel the lookup.</param>
    /// <returns>The hostname associated with the IP address, or null if not found.</returns>
    Task<string?> ReverseLookupAsync(IPAddress address, CancellationToken ct);
}
