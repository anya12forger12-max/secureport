using System.Collections.Concurrent;
using System.Net;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Scanner.Engine;

/// <summary>
/// Orchestrates parallel port scanning across a range of ports on a resolved target.
/// Manages concurrency via <see cref="SemaphoreSlim"/>, reports real-time progress through
/// <see cref="IProgress{T}"/>, and supports cooperative cancellation and rate limiting.
/// </summary>
public sealed class ScanOrchestrator : IAsyncDisposable
{
    private readonly INetworkResolver _resolver;
    private readonly ConnectScannerStrategy _strategy;
    private readonly ILoggingProvider _logger;
    private SemaphoreSlim? _semaphore;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScanOrchestrator"/> class.
    /// </summary>
    /// <param name="resolver">The network resolver for hostname-to-IP translation.</param>
    /// <param name="strategy">The TCP connect scan strategy.</param>
    /// <param name="logger">The logging provider for diagnostic output.</param>
    public ScanOrchestrator(
        INetworkResolver resolver,
        ConnectScannerStrategy strategy,
        ILoggingProvider logger)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves the target host to an IP address and scans every port in the configured range
    /// using bounded parallelism.
    /// </summary>
    /// <param name="target">The scan target specifying host, port range, timeout, and concurrency.</param>
    /// <param name="progress">Optional progress reporter invoked after each port is scanned.</param>
    /// <param name="ct">A cancellation token to stop the scan early.</param>
    /// <returns>An immutable list of individual port scan results.</returns>
    public async Task<IReadOnlyList<ScanResult>> OrchestrateAsync(
        ScanTarget target,
        IProgress<ScanProgress>? progress,
        CancellationToken ct)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScanOrchestrator));

        var sessionId = Guid.NewGuid();
        var addresses = await _resolver.ResolveHostAsync(target.Host, ct);
        if (addresses.Count == 0)
        {
            throw new InvalidOperationException($"Unable to resolve host '{target.Host}' to any IP address.");
        }

        var resolvedHost = addresses.First().ToString();
        _logger.Info("Resolved {Host} to {ResolvedIp}", target.Host, resolvedHost);

        var ports = Enumerable.Range(target.PortStart, target.PortEnd - target.PortStart + 1).ToList();
        var totalPorts = ports.Count;
        var scannedCount = 0;
        var openCount = 0;

        _semaphore = new SemaphoreSlim(target.MaxConcurrentConnections, target.MaxConcurrentConnections);

        var results = new ConcurrentBag<ScanResult>();

        _logger.Info("Starting scan of {TotalPorts} ports on {Host} with {MaxConcurrent} concurrent connections",
            totalPorts, resolvedHost, target.MaxConcurrentConnections);

        await Parallel.ForEachAsync(ports, new ParallelOptions
        {
            MaxDegreeOfParallelism = target.MaxConcurrentConnections,
            CancellationToken = ct
        },
        async (port, cancellationToken) =>
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var portResult = await _strategy.ScanAsync(resolvedHost, port, target.Timeout, cancellationToken);

                var scanResult = new ScanResult
                {
                    Host = target.Host,
                    Port = port,
                    State = portResult.State,
                    ResponseTime = portResult.ResponseTime,
                    Protocol = target.Protocol,
                    ScannedAt = DateTimeOffset.UtcNow
                };

                results.Add(scanResult);

                var currentCount = Interlocked.Increment(ref scannedCount);
                if (portResult.State == PortState.Open)
                {
                    Interlocked.Increment(ref openCount);
                }

                var percentage = totalPorts > 0 ? (double)currentCount / totalPorts * 100.0 : 0.0;
                TimeSpan? estimatedRemaining = null;
                if (currentCount > 0 && currentCount < totalPorts)
                {
                    estimatedRemaining = TimeSpan.FromMilliseconds(
                        target.Timeout.TotalMilliseconds * (totalPorts - currentCount) / target.MaxConcurrentConnections);
                }

                progress?.Report(new ScanProgress
                {
                    SessionId = sessionId,
                    TotalPorts = totalPorts,
                    ScannedPorts = currentCount,
                    OpenPorts = openCount,
                    CurrentPort = port,
                    ElapsedTime = TimeSpan.Zero,
                    EstimatedTimeRemaining = estimatedRemaining,
                    PercentageComplete = percentage
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error scanning port {Port} on {Host}", port, resolvedHost);

                results.Add(new ScanResult
                {
                    Host = target.Host,
                    Port = port,
                    State = PortState.Unknown,
                    Protocol = target.Protocol,
                    ScannedAt = DateTimeOffset.UtcNow
                });

                Interlocked.Increment(ref scannedCount);
            }
            finally
            {
                _semaphore.Release();
            }
        });

        _logger.Info("Scan completed: {OpenCount} open, {ScannedCount} total ports scanned on {Host}",
            openCount, scannedCount, target.Host);

        return results.OrderBy(r => r.Port).ToList();
    }

    /// <summary>
    /// Releases all resources held by the orchestrator.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _semaphore?.Dispose();
        _semaphore = null;

        await ValueTask.CompletedTask;
    }
}
