using System.Text.Json;
using System.Text.Json.Serialization;
using SecurePort.Core.Enums;

namespace SecurePort.Logging.Formatters;

/// <summary>
/// Formats log entries into structured text lines with ISO 8601 timestamps,
/// log level, source, and message. Supports both plain-text and JSON output modes.
/// </summary>
public sealed class LogFormatter
{
    private readonly bool _useJson;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LogFormatter"/> class.
    /// </summary>
    /// <param name="useJson">If true, output is formatted as a single-line JSON object.</param>
    public LogFormatter(bool useJson = false)
    {
        _useJson = useJson;
    }

    /// <summary>
    /// Formats a log entry into a structured string.
    /// </summary>
    /// <param name="timestamp">The UTC timestamp of the log entry.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="source">The source component or class name.</param>
    /// <param name="message">The log message.</param>
    /// <returns>A formatted log line.</returns>
    public string Format(DateTime timestamp, LogLevel level, string source, string message)
    {
        if (_useJson)
            return FormatAsJson(timestamp, level, source, message);

        return FormatAsText(timestamp, level, source, message);
    }

    /// <summary>
    /// Formats a log entry as a structured text line: <c>[TIMESTAMP] [LEVEL] [SOURCE] Message</c>.
    /// Timestamps use ISO 8601 format (UTC).
    /// </summary>
    /// <param name="timestamp">The UTC timestamp of the log entry.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="source">The source component or class name.</param>
    /// <param name="message">The log message.</param>
    /// <returns>A structured text log line.</returns>
    private static string FormatAsText(DateTime timestamp, LogLevel level, string source, string message) =>
        $"[{timestamp:O}] [{level}] [{source}] {message}";

    /// <summary>
    /// Formats a log entry as a single-line JSON object with camelCase property names.
    /// </summary>
    /// <param name="timestamp">The UTC timestamp of the log entry.</param>
    /// <param name="level">The severity level.</param>
    /// <param name="source">The source component or class name.</param>
    /// <param name="message">The log message.</param>
    /// <returns>A single-line JSON string.</returns>
    private static string FormatAsJson(DateTime timestamp, LogLevel level, string source, string message)
    {
        var entry = new JsonLogEntry
        {
            Timestamp = timestamp,
            Level = level.ToString(),
            Source = source,
            Message = message
        };

        return JsonSerializer.Serialize(entry, JsonOptions);
    }

    /// <summary>
    /// DTO used for JSON serialization of a log entry.
    /// </summary>
    private sealed class JsonLogEntry
    {
        /// <summary>The UTC timestamp.</summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; init; }

        /// <summary>The log level as a string.</summary>
        [JsonPropertyName("level")]
        public string Level { get; init; } = string.Empty;

        /// <summary>The source component or class name.</summary>
        [JsonPropertyName("source")]
        public string Source { get; init; } = string.Empty;

        /// <summary>The log message.</summary>
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;
    }
}
