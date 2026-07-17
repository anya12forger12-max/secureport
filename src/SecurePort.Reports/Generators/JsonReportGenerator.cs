using System.Text;
using System.Text.Json;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Reports.Generators;

/// <summary>
/// Generates JSON-formatted reports from scan session data using System.Text.Json.
/// </summary>
public sealed class JsonReportGenerator : IReportGenerator
{
    private static readonly JsonSerializerOptions s_indentedOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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

        var report = BuildReportObject(session, options);
        var json = JsonSerializer.Serialize(report, s_indentedOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Task.FromResult(bytes);
    }

    /// <inheritdoc />
    public string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.JSON => ".json",
        _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
    };

    private static object BuildReportObject(ScanSession session, ExportOptions options)
    {
        var results = options.IncludeClosedPorts
            ? session.Results
            : session.Results.Where(r => r.State != PortState.Closed).ToList();

        var reportObject = new
        {
            metadata = new
            {
                reportId = Guid.NewGuid().ToString("D"),
                generatedAt = DateTimeOffset.UtcNow,
                format = "JSON",
                version = "1.0"
            },
            scan = options.IncludeScanMetadata
                ? new
                {
                    sessionId = session.Id,
                    status = session.Status.ToString(),
                    startTime = session.StartTime,
                    endTime = session.EndTime,
                    duration = session.Duration?.ToString(),
                    totalPortsScanned = session.TotalPortsScanned,
                    errorMessage = session.ErrorMessage
                }
                : null,
            target = new
            {
                host = session.Target.Host,
                portStart = session.Target.PortStart,
                portEnd = session.Target.PortEnd,
                protocol = session.Target.Protocol.ToString(),
                scanType = session.Target.ScanType.ToString(),
                timeout = session.Target.Timeout,
                maxConcurrentConnections = session.Target.MaxConcurrentConnections
            },
            statistics = new
            {
                openPorts = session.OpenPortsFound,
                closedPorts = session.ClosedPortsFound,
                filteredPorts = session.FilteredPortsFound,
                errors = session.ErrorsEncountered,
                totalScanned = session.TotalPortsScanned
            },
            results = results.Select(r => BuildResultObject(r, options)).ToList()
        };

        return reportObject;
    }

    private static object BuildResultObject(ScanResult result, ExportOptions options)
    {
        var obj = new
        {
            host = result.Host,
            port = result.Port,
            state = result.State.ToString(),
            protocol = result.Protocol.ToString(),
            serviceName = result.ServiceName,
            bannerInfo = result.BannerInfo,
            responseTime = result.ResponseTime?.TotalMilliseconds
        };

        if (options.IncludeTimestamps)
        {
            return new
            {
                obj.host,
                obj.port,
                obj.state,
                obj.protocol,
                obj.serviceName,
                obj.bannerInfo,
                obj.responseTime,
                scannedAt = result.ScannedAt
            };
        }

        return obj;
    }
}
