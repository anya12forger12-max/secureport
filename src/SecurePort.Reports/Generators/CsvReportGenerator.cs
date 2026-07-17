using System.Text;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Reports.Generators;

/// <summary>
/// Generates CSV-formatted reports from scan session data with proper escaping and header rows.
/// </summary>
public sealed class CsvReportGenerator : IReportGenerator
{
    private const char Delimiter = ',';
    private const char Quote = '"';
    private const string LineEnding = "\r\n";

    /// <summary>
    /// Gets or sets the set of columns to include in the CSV output.
    /// Defaults to all available columns.
    /// </summary>
    public IReadOnlyList<string> Columns { get; set; } = new[]
    {
        "Host",
        "Port",
        "Protocol",
        "State",
        "ServiceName",
        "BannerInfo",
        "ResponseTimeMs",
        "ScannedAt"
    };

    /// <inheritdoc />
    public async Task<string> GenerateReportAsync(ScanSession session, ExportOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(options);

        var bytes = await GenerateReportBytesAsync(session, options, ct);
        var directory = Path.GetDirectoryName(options.FilePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(options.FilePath, bytes, ct);
        return options.FilePath;
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateReportBytesAsync(ScanSession session, ExportOptions options, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(options);

        ct.ThrowIfCancellationRequested();

        var sb = new StringBuilder();

        WriteHeader(sb);

        var results = options.IncludeClosedPorts
            ? session.Results
            : session.Results.Where(r => r.State != PortState.Closed).ToList();

        foreach (var result in results)
        {
            WriteRow(sb, result, options);
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Task.FromResult(bytes);
    }

    /// <inheritdoc />
    public string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.CSV => ".csv",
        _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
    };

    private void WriteHeader(StringBuilder sb)
    {
        for (var i = 0; i < Columns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(Delimiter);
            }

            sb.Append(EscapeField(Columns[i]));
        }

        sb.Append(LineEnding);
    }

    private void WriteRow(StringBuilder sb, ScanResult result, ExportOptions options)
    {
        var fields = GetFieldValues(result, options);

        for (var i = 0; i < Columns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(Delimiter);
            }

            var value = i < fields.Length ? fields[i] : string.Empty;
            sb.Append(EscapeField(value));
        }

        sb.Append(LineEnding);
    }

    private string[] GetFieldValues(ScanResult result, ExportOptions options)
    {
        var values = new List<string>(Columns.Count);

        foreach (var column in Columns)
        {
            values.Add(column switch
            {
                "Host" => result.Host,
                "Port" => result.Port.ToString(),
                "Protocol" => result.Protocol.ToString(),
                "State" => result.State.ToString(),
                "ServiceName" => result.ServiceName ?? string.Empty,
                "BannerInfo" => result.BannerInfo ?? string.Empty,
                "ResponseTimeMs" => result.ResponseTime?.TotalMilliseconds.ToString("F2") ?? string.Empty,
                "ScannedAt" => options.IncludeTimestamps ? result.ScannedAt.ToString("o") : string.Empty,
                _ => string.Empty
            });
        }

        return values.ToArray();
    }

    private static string EscapeField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        if (field.Contains(Delimiter) || field.Contains(Quote) || field.Contains('\n') || field.Contains('\r'))
        {
            var escaped = field.Replace(Quote.ToString(), $"{Quote}{Quote}");
            return $"{Quote}{escaped}{Quote}";
        }

        return field;
    }
}
