using System.Net.Sockets;

namespace SecurePort.Networking.Sockets;

/// <summary>
/// Configuration options for <see cref="TcpClient"/> instances created by <see cref="SocketFactory"/>.
/// </summary>
public sealed class SocketOptions
{
    /// <summary>
    /// Gets or sets the receive buffer size in bytes.
    /// Defaults to 8192 bytes.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 8192;

    /// <summary>
    /// Gets or sets the send buffer size in bytes.
    /// Defaults to 8192 bytes.
    /// </summary>
    public int SendBufferSize { get; set; } = 8192;

    /// <summary>
    /// Gets or sets the linger option applied to the underlying socket.
    /// When <c>null</c>, the system default linger behavior is used.
    /// Defaults to <c>null</c>.
    /// </summary>
    public LingerOption? LingerOption { get; set; }
}

/// <summary>
/// Factory for creating pre-configured <see cref="TcpClient"/> instances
/// with settings optimised for high-throughput port scanning.
/// </summary>
public static class SocketFactory
{
    /// <summary>
    /// The default socket options applied to every client created by <see cref="CreateClient"/>.
    /// </summary>
    private static readonly SocketOptions s_defaultOptions = new();

    /// <summary>
    /// Creates a new <see cref="TcpClient"/> with sensible defaults for port scanning.
    /// The Nagle algorithm is disabled (<c>NoDelay = true</c>) to minimise latency.
    /// </summary>
    /// <returns>A configured <see cref="TcpClient"/> ready to connect.</returns>
    public static TcpClient CreateClient()
    {
        return CreateClient(s_defaultOptions);
    }

    /// <summary>
    /// Creates a new <see cref="TcpClient"/> using the supplied <paramref name="options"/>.
    /// The Nagle algorithm is disabled (<c>NoDelay = true</c>) to minimise latency.
    /// </summary>
    /// <param name="options">The socket configuration to apply. Must not be <c>null</c>.</param>
    /// <returns>A configured <see cref="TcpClient"/> ready to connect.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    public static TcpClient CreateClient(SocketOptions options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var client = new TcpClient();
        client.NoDelay = true;

        client.ReceiveBufferSize = options.ReceiveBufferSize;
        client.SendBufferSize = options.SendBufferSize;

        if (options.LingerOption is not null)
            client.LingerState = options.LingerOption;

        return client;
    }

    /// <summary>
    /// Creates a new <see cref="TcpClient"/> bound to a specific local <paramref name="address"/>.
    /// Useful when the host has multiple network interfaces and traffic must originate
    /// from a known source.
    /// </summary>
    /// <param name="address">The local IP address to bind the socket to.</param>
    /// <returns>A configured <see cref="TcpClient"/> bound to the specified address.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> is <c>null</c>.</exception>
    public static TcpClient CreateClient(System.Net.IPAddress address)
    {
        if (address is null)
            throw new ArgumentNullException(nameof(address));

        var client = new TcpClient(address.AddressFamily);
        client.NoDelay = true;
        client.Client.Bind(new System.Net.IPEndPoint(address, 0));
        return client;
    }

    /// <summary>
    /// Creates a new <see cref="TcpClient"/> bound to a specific local <paramref name="address"/>
    /// with the supplied <paramref name="options"/>.
    /// </summary>
    /// <param name="address">The local IP address to bind the socket to.</param>
    /// <param name="options">The socket configuration to apply. Must not be <c>null</c>.</param>
    /// <returns>A configured <see cref="TcpClient"/> bound to the specified address.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="address"/> or <paramref name="options"/> is <c>null</c>.</exception>
    public static TcpClient CreateClient(System.Net.IPAddress address, SocketOptions options)
    {
        if (address is null)
            throw new ArgumentNullException(nameof(address));
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        var client = new TcpClient(address.AddressFamily);
        client.NoDelay = true;
        client.Client.Bind(new System.Net.IPEndPoint(address, 0));

        client.ReceiveBufferSize = options.ReceiveBufferSize;
        client.SendBufferSize = options.SendBufferSize;

        if (options.LingerOption is not null)
            client.LingerState = options.LingerOption;

        return client;
    }
}
