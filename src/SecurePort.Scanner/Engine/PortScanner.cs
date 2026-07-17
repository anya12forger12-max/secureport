using System.Diagnostics;
using System.Net.Sockets;
using SecurePort.Core.Enums;
using SecurePort.Core.Exceptions;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Scanner.Engine;

/// <summary>
/// Core port scanner implementing <see cref="IScanner"/>. Manages the full scan lifecycle
/// including validation, progress reporting through a reactive <see cref="IObservable{T}"/>
/// subject, concurrency control, cancellation, and result aggregation.
/// </summary>
public sealed class PortScanner : IScanner, IAsyncDisposable
{
    private readonly ITargetValidator _validator;
    private readonly INetworkResolver _resolver;
    private readonly ConnectScannerStrategy _strategy;
    private readonly ILoggingProvider _logger;
    private readonly Subject<ScanProgress> _progressSubject;
    private CancellationTokenSource? _cts;
    private readonly object _scanLock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PortScanner"/> class.
    /// </summary>
    /// <param name="validator">The target validator for input validation.</param>
    /// <param name="resolver">The network resolver for hostname-to-IP translation.</param>
    /// <param name="strategy">The TCP connect scan strategy.</param>
    /// <param name="logger">The logging provider for diagnostic output.</param>
    public PortScanner(
        ITargetValidator validator,
        INetworkResolver resolver,
        ConnectScannerStrategy strategy,
        ILoggingProvider logger)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _progressSubject = new Subject<ScanProgress>();
    }

    /// <inheritdoc />
    public IObservable<ScanProgress> ProgressChanged => _progressSubject;

    /// <inheritdoc />
    public bool IsScanning { get; private set; }

    /// <inheritdoc />
    public async Task<ScanSession> StartScanAsync(ScanTarget target, CancellationToken ct)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PortScanner));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var validationErrors = _validator.Validate(target);
        if (validationErrors.Count > 0)
        {
            throw new ValidationException($"Scan target validation failed: {string.Join("; ", validationErrors)}");
        }

        var sessionId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        lock (_scanLock)
        {
            if (IsScanning)
                throw new InvalidOperationException("A scan is already in progress. Cancel the current scan before starting a new one.");

            IsScanning = true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        }

        var openCount = 0;
        var closedCount = 0;
        var filteredCount = 0;
        var errorCount = 0;

        try
        {
            _logger.Info("Starting scan session {SessionId} against {Host} ports {PortStart}-{PortEnd}",
                sessionId, target.Host, target.PortStart, target.PortEnd);

            var orchestrator = new ScanOrchestrator(_resolver, _strategy, _logger);

            var progressCallback = new Progress<ScanProgress>(p => _progressSubject.OnNext(p));
            var results = await orchestrator.OrchestrateAsync(target, progressCallback, _cts.Token);

            stopwatch.Stop();

            foreach (var result in results)
            {
                switch (result.State)
                {
                    case PortState.Open:
                        openCount++;
                        break;
                    case PortState.Closed:
                        closedCount++;
                        break;
                    case PortState.Filtered:
                    case PortState.Timeout:
                        filteredCount++;
                        break;
                    case PortState.Unknown:
                        errorCount++;
                        break;
                }
            }

            var session = new ScanSession
            {
                Id = sessionId,
                Target = target,
                Results = results,
                Status = ScanStatus.Completed,
                StartTime = DateTimeOffset.UtcNow - stopwatch.Elapsed,
                EndTime = DateTimeOffset.UtcNow,
                TotalPortsScanned = results.Count,
                OpenPortsFound = openCount,
                ClosedPortsFound = closedCount,
                FilteredPortsFound = filteredCount,
                ErrorsEncountered = errorCount,
                Duration = stopwatch.Elapsed
            };

            _logger.Info("Scan session {SessionId} completed: {Open} open, {Closed} closed, {Filtered} filtered, {Errors} errors in {Duration}",
                sessionId, openCount, closedCount, filteredCount, errorCount, stopwatch.Elapsed);

            EmitFinalProgress(sessionId, target, results.Count, openCount, stopwatch.Elapsed);

            return session;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            stopwatch.Stop();

            _logger.Warning("Scan session {SessionId} was cancelled", sessionId);

            EmitFinalProgress(sessionId, target, 0, 0, stopwatch.Elapsed);

            return new ScanSession
            {
                Id = sessionId,
                Target = target,
                Results = Array.Empty<ScanResult>(),
                Status = ScanStatus.Cancelled,
                StartTime = DateTimeOffset.UtcNow - stopwatch.Elapsed,
                EndTime = DateTimeOffset.UtcNow,
                TotalPortsScanned = 0,
                OpenPortsFound = 0,
                ClosedPortsFound = 0,
                FilteredPortsFound = 0,
                ErrorsEncountered = 0,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.Error(ex, "Scan session {SessionId} failed", sessionId);

            EmitFinalProgress(sessionId, target, 0, 0, stopwatch.Elapsed);

            return new ScanSession
            {
                Id = sessionId,
                Target = target,
                Results = Array.Empty<ScanResult>(),
                Status = ScanStatus.Failed,
                StartTime = DateTimeOffset.UtcNow - stopwatch.Elapsed,
                EndTime = DateTimeOffset.UtcNow,
                TotalPortsScanned = 0,
                OpenPortsFound = 0,
                ClosedPortsFound = 0,
                FilteredPortsFound = 0,
                ErrorsEncountered = 1,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            };
        }
        finally
        {
            lock (_scanLock)
            {
                IsScanning = false;
            }

            await DisposeScanResourcesAsync();
        }
    }

    /// <inheritdoc />
    public Task CancelScanAsync()
    {
        CancellationTokenSource? cts;
        lock (_scanLock)
        {
            cts = _cts;
        }

        if (cts is not null && !cts.IsCancellationRequested)
        {
            _logger.Info("Cancelling active scan");
            cts.Cancel();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Performs a TCP connect scan on a single port and returns the result.
    /// This method is the core scanning primitive used by the scanner engine.
    /// </summary>
    /// <param name="host">The resolved IP address to scan.</param>
    /// <param name="port">The port number to scan.</param>
    /// <param name="timeout">The connection timeout.</param>
    /// <param name="ct">A cancellation token to abort the scan.</param>
    /// <returns>A <see cref="ScanResult"/> for the specified port.</returns>
    internal async Task<ScanResult> ScanSinglePortAsync(string host, int port, TimeSpan timeout, CancellationToken ct)
    {
        var portScanResult = await _strategy.ScanAsync(host, port, timeout, ct);

        return new ScanResult
        {
            Host = host,
            Port = port,
            State = portScanResult.State,
            ResponseTime = portScanResult.ResponseTime,
            Protocol = SecurePort.Core.Enums.ProtocolType.TCP,
            ScannedAt = DateTimeOffset.UtcNow
        };
    }

    private void EmitFinalProgress(Guid sessionId, ScanTarget target, int totalScanned, int openPorts, TimeSpan elapsed)
    {
        var totalPorts = target.PortEnd - target.PortStart + 1;
        _progressSubject.OnNext(new ScanProgress
        {
            SessionId = sessionId,
            TotalPorts = totalPorts,
            ScannedPorts = totalScanned,
            OpenPorts = openPorts,
            CurrentPort = target.PortEnd,
            ElapsedTime = elapsed,
            PercentageComplete = 100.0
        });
    }

    private Task DisposeScanResourcesAsync()
    {
        CancellationTokenSource? cts;
        lock (_scanLock)
        {
            cts = _cts;
            _cts = null;
        }

        cts?.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Releases all resources held by the scanner, including the progress subject and cancellation tokens.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _progressSubject.Dispose();

        await DisposeScanResourcesAsync();
    }

    /// <summary>
    /// A minimal thread-safe observable subject for emitting progress updates.
    /// Snapshot-based dispatch ensures that observers added or removed during
    /// emission are handled safely.
    /// </summary>
    private sealed class Subject<T> : IObservable<T>, IDisposable
    {
        private readonly List<IObserver<T>> _observers = new();
        private readonly object _lock = new();
        private bool _completed;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            lock (_lock)
            {
                if (_completed)
                {
                    return EmptyDisposable.Instance;
                }

                _observers.Add(observer);
            }

            return new Subscription(_observers, observer, _lock);
        }

        public void OnNext(T value)
        {
            IObserver<T>[] snapshot;
            lock (_lock)
            {
                if (_completed) return;
                snapshot = _observers.ToArray();
            }

            foreach (var observer in snapshot)
            {
                try
                {
                    observer.OnNext(value);
                }
                catch
                {
                    // Swallow observer exceptions to protect the emission pipeline.
                }
            }
        }

        public void OnCompleted()
        {
            IObserver<T>[] snapshot;
            lock (_lock)
            {
                if (_completed) return;
                _completed = true;
                snapshot = _observers.ToArray();
            }

            foreach (var observer in snapshot)
            {
                try
                {
                    observer.OnCompleted();
                }
                catch
                {
                    // Swallow observer exceptions.
                }
            }
        }

        public void OnError(Exception error)
        {
            IObserver<T>[] snapshot;
            lock (_lock)
            {
                if (_completed) return;
                _completed = true;
                snapshot = _observers.ToArray();
            }

            foreach (var observer in snapshot)
            {
                try
                {
                    observer.OnError(error);
                }
                catch
                {
                    // Swallow observer exceptions.
                }
            }
        }

        public void Dispose()
        {
            OnCompleted();
        }

        private sealed class Subscription : IDisposable
        {
            private readonly List<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;
            private readonly object _lock;
            private bool _disposed;

            public Subscription(List<IObserver<T>> observers, IObserver<T> observer, object @lock)
            {
                _observers = observers;
                _observer = observer;
                _lock = @lock;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                lock (_lock)
                {
                    _observers.Remove(_observer);
                }
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
