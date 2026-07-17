using System.Diagnostics;
using System.Net.Sockets;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;

namespace SecurePort.Scanner.Engine;

/// <summary>
/// Represents the outcome of a single TCP connect scan attempt.
/// </summary>
/// <param name="State">The observed state of the port.</param>
/// <param name="ResponseTime">The elapsed time for the connection attempt.</param>
public sealed record PortScanResult(PortState State, TimeSpan ResponseTime);

/// <summary>
/// Performs TCP connect scans against individual ports using <see cref="TcpClient"/>.
/// Handles connection timeouts, refused connections, and unexpected socket errors
/// by mapping them to the appropriate <see cref="PortState"/>.
/// </summary>
public sealed class ConnectScannerStrategy
{
    private readonly ILoggingProvider _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectScannerStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logging provider for diagnostic output.</param>
    public ConnectScannerStrategy(ILoggingProvider logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attempts a TCP connection to the specified host and port within the given timeout.
    /// </summary>
    /// <param name="host">The resolved IP address to connect to.</param>
    /// <param name="port">The port number to scan.</param>
    /// <param name="timeout">The maximum time to wait for a connection.</param>
    /// <param name="ct">A cancellation token to abort the scan early.</param>
    /// <returns>
    /// A <see cref="PortScanResult"/> indicating whether the port is open, closed,
    /// filtered, or timed out, along with the measured response time.
    /// </returns>
    public async Task<PortScanResult> ScanAsync(string host, int port, TimeSpan timeout, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client = new TcpClient();
            client.ReceiveTimeout = (int)timeout.TotalMilliseconds;
            client.SendTimeout = (int)timeout.TotalMilliseconds;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linkedCts.CancelAfter(timeout);

            await client.ConnectAsync(host, port, linkedCts.Token);

            stopwatch.Stop();

            _logger.Debug("Port {Port} on {Host} is open ({Elapsed}ms)", port, host, stopwatch.ElapsedMilliseconds);

            return new PortScanResult(PortState.Open, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();

            _logger.Debug("Port {Port} on {Host} timed out ({Elapsed}ms)", port, host, stopwatch.ElapsedMilliseconds);

            return new PortScanResult(PortState.Timeout, stopwatch.Elapsed);
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
        {
            stopwatch.Stop();

            _logger.Debug("Port {Port} on {Host} is closed ({Elapsed}ms)", port, host, stopwatch.ElapsedMilliseconds);

            return new PortScanResult(PortState.Closed, stopwatch.Elapsed);
        }
        catch (SocketException ex)
        {
            stopwatch.Stop();

            _logger.Debug("Port {Port} on {Host} is filtered, error: {Error} ({Elapsed}ms)",
                port, host, ex.SocketErrorCode, stopwatch.ElapsedMilliseconds);

            return new PortScanResult(PortState.Filtered, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.Error(ex, "Unexpected error scanning port {Port} on {Host}", port, host);

            return new PortScanResult(PortState.Filtered, stopwatch.Elapsed);
        }
    }
}
