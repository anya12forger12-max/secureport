using System.Collections.Concurrent;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;

namespace SecurePort.Logging.Providers;

/// <summary>
/// File-based implementation of <see cref="ILoggingProvider"/> that writes structured
/// log entries to date-rotated text files. Uses a <see cref="ConcurrentQueue{T}"/>
/// for batched writes and supports log level filtering.
/// </summary>
public sealed class FileLoggingProvider : ILoggingProvider, IDisposable
{
    private readonly string _logDirectory;
    private readonly LogLevel _minimumLevel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentQueue<LogEntry> _queue = new();
    private readonly Timer? _flushTimer;
    private bool _disposed;

    /// <summary>
    /// The interval between automatic flush cycles.
    /// </summary>
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="FileLoggingProvider"/> class.
    /// </summary>
    /// <param name="logDirectory">The directory where log files are stored. Created if it does not exist.</param>
    /// <param name="minimumLevel">The minimum log level to capture.</param>
    public FileLoggingProvider(string logDirectory, LogLevel minimumLevel = LogLevel.Info)
    {
        if (string.IsNullOrWhiteSpace(logDirectory))
            throw new ArgumentException("Log directory must not be null or empty.", nameof(logDirectory));

        _logDirectory = logDirectory;
        _minimumLevel = minimumLevel;

        Directory.CreateDirectory(_logDirectory);

        _flushTimer = new Timer(
            _ => FlushAsync().GetAwaiter().GetResult(),
            null,
            FlushInterval,
            FlushInterval);
    }

    /// <inheritdoc/>
    public LogLevel MinimumLevel => _minimumLevel;

    /// <inheritdoc/>
    public void SetMinimumLevel(LogLevel level)
    {
        throw new NotSupportedException(
            $"Cannot change minimum level after construction. Create a new {nameof(FileLoggingProvider)} with the desired level.");
    }

    /// <inheritdoc/>
    public void Trace(string message, params object[] args) => Enqueue(LogLevel.Trace, message, args);

    /// <inheritdoc/>
    public void Debug(string message, params object[] args) => Enqueue(LogLevel.Debug, message, args);

    /// <inheritdoc/>
    public void Info(string message, params object[] args) => Enqueue(LogLevel.Info, message, args);

    /// <inheritdoc/>
    public void Warning(string message, params object[] args) => Enqueue(LogLevel.Warning, message, args);

    /// <inheritdoc/>
    public void Error(string message, params object[] args) => Enqueue(LogLevel.Error, message, args);

    /// <inheritdoc/>
    public void Error(Exception exception, string message, params object[] args)
    {
        var combined = args.Length > 0
            ? $"{string.Format(message, args)} | {exception}"
            : $"{message} | {exception}";

        Enqueue(LogLevel.Error, combined, Array.Empty<object>());
    }

    /// <inheritdoc/>
    public void Critical(string message, params object[] args) => Enqueue(LogLevel.Critical, message, args);

    /// <inheritdoc/>
    public void Log(LogLevel level, string message, params object[] args) => Enqueue(level, message, args);

    /// <summary>
    /// Enqueues a log entry if the specified level meets the minimum threshold.
    /// </summary>
    private void Enqueue(LogLevel level, string message, object[] args)
    {
        if (_disposed || level < _minimumLevel)
            return;

        var formatted = args.Length > 0 ? string.Format(message, args) : message;

        _queue.Enqueue(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Source = GetSourceName(),
            Message = formatted
        });
    }

    /// <summary>
    /// Flushes all queued log entries to disk under a semaphore.
    /// </summary>
    private async Task FlushAsync()
    {
        if (_disposed)
            return;

        await _semaphore.WaitAsync();
        try
        {
            while (_queue.TryDequeue(out var entry))
            {
                var filePath = GetLogFilePath(entry.Timestamp);
                var line = FormatEntry(entry);
                await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Constructs the log file path for the given date using the <c>log-YYYY-MM-DD.txt</c> convention.
    /// </summary>
    /// <param name="timestamp">The timestamp whose date determines the file name.</param>
    /// <returns>The full file path for the log file.</returns>
    private string GetLogFilePath(DateTime timestamp) =>
        Path.Combine(_logDirectory, $"log-{timestamp:yyyy-MM-dd}.txt");

    /// <summary>
    /// Formats a <see cref="LogEntry"/> into a structured text line.
    /// </summary>
    /// <param name="entry">The log entry to format.</param>
    /// <returns>A formatted log line.</returns>
    private static string FormatEntry(LogEntry entry) =>
        $"[{entry.Timestamp:O}] [{entry.Level}] [{entry.Source}] {entry.Message}";

    /// <summary>
    /// Derives the source caller name from the current stack frame.
    /// </summary>
    private static string GetSourceName()
    {
        var stackTrace = new System.Diagnostics.StackTrace(2, false);
        var frame = stackTrace.GetFrames()
            ?.FirstOrDefault(f => f.GetMethod()?.DeclaringType?.Namespace?.StartsWith("SecurePort") == true);

        return frame?.GetMethod()?.DeclaringType?.Name ?? "Unknown";
    }

    /// <summary>
    /// Represents a single log entry before persistence.
    /// </summary>
    private sealed class LogEntry
    {
        /// <summary>The UTC timestamp of the log entry.</summary>
        public DateTime Timestamp { get; init; }

        /// <summary>The severity level.</summary>
        public LogLevel Level { get; init; }

        /// <summary>The source component or class name.</summary>
        public string Source { get; init; } = string.Empty;

        /// <summary>The log message.</summary>
        public string Message { get; init; } = string.Empty;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _flushTimer?.Dispose();

        // Final flush of remaining entries.
        FlushAsync().GetAwaiter().GetResult();
        _semaphore.Dispose();
    }
}
