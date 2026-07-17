using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Manages report generation, storage, search, and organization.
/// Bridges the ReportPreparationEngine (Part 2A) with file export and persistence.
/// </summary>
public sealed class ReportManager : IReportManager, IDisposable
{
    private readonly IReportVerificationManager _verificationManager;
    private readonly IVisualizationEngine _visualizationEngine;
    private readonly IStatisticsEngine _statisticsEngine;
    private readonly IScanComparisonEngine _comparisonEngine;
    private readonly string _reportsDirectory;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly List<ReportMetadata> _metadataIndex = new();
    private bool _disposed;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<ReportMetadata> GenerateAndStoreReportAsync(
        ReportData data,
        ExportFormat format,
        string? customTitle,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(data);

        var title = customTitle ?? data.Title;
        var reportId = Guid.NewGuid();
        var extension = GetFileExtension(format);
        var fileName = $"report_{reportId:N}{extension}";
        var filePath = Path.Combine(_reportsDirectory, fileName);

        var content = format switch
        {
            ExportFormat.JSON => GenerateJsonContent(data),
            ExportFormat.CSV => GenerateCsvContent(data),
            ExportFormat.TXT => GenerateTxtContent(data),
            ExportFormat.HTML => GenerateHtmlContent(data),
            _ => GenerateJsonContent(data)
        };

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8, ct);

        var checksum = _verificationManager.ComputeChecksum(Encoding.UTF8.GetBytes(content));
        var fileInfo = new FileInfo(filePath);

        var metadata = new ReportMetadata
        {
            ReportId = reportId,
            Title = title,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = Core.Constants.AppConstants.Version,
            OperatingSystem = Environment.OSVersion.ToString(),
            Format = format,
            FileSizeBytes = fileInfo.Length,
            Checksum = checksum,
            ScanId = data.Sessions.Count > 0 ? data.Sessions[0].Id : Guid.Empty,
            ScanProfile = data.Results.Count > 0 ? data.Results[0].ScanProfile : null,
            TargetHost = data.Sessions.Count > 0 ? data.Sessions[0].Target.Host : "Multiple",
            FilePath = filePath,
            IsArchived = false,
            Tags = Array.Empty<string>()
        };

        await _lock.WaitAsync(ct);
        try
        {
            _metadataIndex.Add(metadata);
            SaveMetadataIndex();
        }
        finally
        {
            _lock.Release();
        }

        return metadata;
    }

    public async Task<IReadOnlyList<ReportMetadata>> GetAllReportsAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metadataIndex.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ReportMetadata?> GetReportByIdAsync(Guid reportId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metadataIndex.FirstOrDefault(r => r.ReportId == reportId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteReportAsync(Guid reportId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var report = _metadataIndex.FirstOrDefault(r => r.ReportId == reportId);
            if (report is null)
                return false;

            if (File.Exists(report.FilePath))
                File.Delete(report.FilePath);

            _metadataIndex.Remove(report);
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RenameReportAsync(Guid reportId, string newTitle, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            return false;

        await _lock.WaitAsync(ct);
        try
        {
            var index = _metadataIndex.FindIndex(r => r.ReportId == reportId);
            if (index < 0)
                return false;

            var old = _metadataIndex[index];
            _metadataIndex[index] = old with { Title = newTitle };
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ReportMetadata?> DuplicateReportAsync(Guid reportId, string? newTitle, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var original = _metadataIndex.FirstOrDefault(r => r.ReportId == reportId);
            if (original is null)
                return null;

            if (!File.Exists(original.FilePath))
                return null;

            var newId = Guid.NewGuid();
            var extension = Path.GetExtension(original.FilePath);
            var newFileName = $"report_{newId:N}{extension}";
            var newFilePath = Path.Combine(_reportsDirectory, newFileName);

            File.Copy(original.FilePath, newFilePath);

            var content = await File.ReadAllTextAsync(newFilePath, ct);
            var checksum = _verificationManager.ComputeChecksum(Encoding.UTF8.GetBytes(content));

            var duplicate = original with
            {
                ReportId = newId,
                Title = newTitle ?? $"{original.Title} (Copy)",
                FilePath = newFilePath,
                Checksum = checksum,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _metadataIndex.Add(duplicate);
            SaveMetadataIndex();
            return duplicate;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ArchiveReportAsync(Guid reportId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var index = _metadataIndex.FindIndex(r => r.ReportId == reportId);
            if (index < 0)
                return false;

            var old = _metadataIndex[index];
            _metadataIndex[index] = old with { IsArchived = true };
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RestoreReportAsync(Guid reportId, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var index = _metadataIndex.FindIndex(r => r.ReportId == reportId);
            if (index < 0)
                return false;

            var old = _metadataIndex[index];
            _metadataIndex[index] = old with { IsArchived = false };
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<ReportMetadata>> SearchReportsAsync(ReportSearchCriteria criteria, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        await _lock.WaitAsync(ct);
        try
        {
            IEnumerable<ReportMetadata> query = _metadataIndex;

            if (criteria.DateStart.HasValue)
                query = query.Where(r => r.CreatedAt >= criteria.DateStart.Value);

            if (criteria.DateEnd.HasValue)
                query = query.Where(r => r.CreatedAt <= criteria.DateEnd.Value);

            if (!string.IsNullOrWhiteSpace(criteria.TargetHost))
                query = query.Where(r => r.TargetHost.Contains(criteria.TargetHost, StringComparison.OrdinalIgnoreCase));

            if (criteria.ReportId.HasValue)
                query = query.Where(r => r.ReportId == criteria.ReportId.Value);

            if (criteria.ScanId.HasValue)
                query = query.Where(r => r.ScanId == criteria.ScanId.Value);

            if (criteria.Format.HasValue)
                query = query.Where(r => r.Format == criteria.Format.Value);

            if (!string.IsNullOrWhiteSpace(criteria.ScanProfile))
                query = query.Where(r => r.ScanProfile != null && r.ScanProfile.Contains(criteria.ScanProfile, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(criteria.Keywords))
            {
                var kw = criteria.Keywords.Trim();
                query = query.Where(r =>
                    r.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    r.Tags.Any(t => t.Contains(kw, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Tag))
                query = query.Where(r => r.Tags.Any(t => t.Equals(criteria.Tag, StringComparison.OrdinalIgnoreCase)));

            if (criteria.IsArchived.HasValue)
                query = query.Where(r => r.IsArchived == criteria.IsArchived.Value);

            query = criteria.SortBy switch
            {
                ReportSortField.CreatedAt => criteria.SortDescending
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                ReportSortField.Title => criteria.SortDescending
                    ? query.OrderByDescending(r => r.Title)
                    : query.OrderBy(r => r.Title),
                ReportSortField.TargetHost => criteria.SortDescending
                    ? query.OrderByDescending(r => r.TargetHost)
                    : query.OrderBy(r => r.TargetHost),
                ReportSortField.Format => criteria.SortDescending
                    ? query.OrderByDescending(r => r.Format)
                    : query.OrderBy(r => r.Format),
                ReportSortField.FileSizeBytes => criteria.SortDescending
                    ? query.OrderByDescending(r => r.FileSizeBytes)
                    : query.OrderBy(r => r.FileSizeBytes),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            return query.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> AddTagToReportAsync(Guid reportId, string tag, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        await _lock.WaitAsync(ct);
        try
        {
            var index = _metadataIndex.FindIndex(r => r.ReportId == reportId);
            if (index < 0)
                return false;

            var old = _metadataIndex[index];
            if (old.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                return true;

            var newTags = new List<string>(old.Tags) { tag.Trim() };
            _metadataIndex[index] = old with { Tags = newTags };
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveTagFromReportAsync(Guid reportId, string tag, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        await _lock.WaitAsync(ct);
        try
        {
            var index = _metadataIndex.FindIndex(r => r.ReportId == reportId);
            if (index < 0)
                return false;

            var old = _metadataIndex[index];
            var newTags = old.Tags.Where(t => !t.Equals(tag, StringComparison.OrdinalIgnoreCase)).ToList();
            _metadataIndex[index] = old with { Tags = newTags };
            SaveMetadataIndex();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> ReExportReportAsync(Guid reportId, string destinationPath, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var report = _metadataIndex.FirstOrDefault(r => r.ReportId == reportId);
            if (report is null)
                throw new InvalidOperationException($"Report not found: {reportId}");

            if (!File.Exists(report.FilePath))
                throw new FileNotFoundException($"Report file not found: {report.FilePath}");

            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.Copy(report.FilePath, destinationPath, overwrite: true);
            return destinationPath;
        }
        finally
        {
            _lock.Release();
        }
    }

    private string GenerateJsonContent(ReportData data)
    {
        var obj = new
        {
            metadata = new
            {
                reportId = Guid.NewGuid().ToString("D"),
                title = data.Title,
                generatedAt = data.GeneratedAt,
                applicationVersion = Core.Constants.AppConstants.Version,
                format = "JSON"
            },
            statistics = new
            {
                totalScans = data.Statistics.TotalScans,
                totalPortsScanned = data.Statistics.TotalPortsScanned,
                openPorts = data.Statistics.OpenPortsCount,
                closedPorts = data.Statistics.ClosedPortsCount,
                otherPorts = data.Statistics.OtherPortsCount,
                averageScanDuration = data.Statistics.AverageScanDuration?.ToString(),
                averageResponseTime = data.Statistics.AverageResponseTime?.ToString(),
                fastestResponse = data.Statistics.FastestResponse?.ToString(),
                slowestResponse = data.Statistics.SlowestResponse?.ToString()
            },
            services = data.Statistics.MostCommonServices.Select(s => new { s.ServiceName, s.Count }),
            ports = data.Statistics.MostFrequentPorts.Select(p => new { p.Port, p.Count }),
            results = data.Results.Select(r => new
            {
                id = r.Id,
                target = r.Target,
                port = r.Port,
                protocol = r.Protocol.ToString(),
                state = r.State.ToString(),
                serviceName = r.ServiceName,
                responseTime = r.ResponseTime?.TotalMilliseconds,
                banner = r.Banner,
                confidence = r.DetectionConfidence,
                tags = r.Tags,
                notes = r.Notes,
                isFavorite = r.IsFavorite
            }),
            charts = data.Charts.Select(c => new { c.Title, c.AccessibilitySummary, series = c.Series.Select(s => new { s.Name, dataPoints = s.DataPoints.Select(d => new { d.Label, d.Value }) }) })
        };

        return JsonSerializer.Serialize(obj, s_jsonOptions);
    }

    private static string GenerateCsvContent(ReportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Target,Port,Protocol,State,Service,Description,ResponseTimeMs,Confidence,Banner,Tags,Notes,IsFavorite");

        foreach (var r in data.Results)
        {
            var responseTime = r.ResponseTime?.TotalMilliseconds.ToString("F2") ?? "";
            var tags = string.Join(";", r.Tags);
            var notes = EscapeCsv(r.Notes ?? "");
            var banner = EscapeCsv(r.Banner ?? "");
            var service = EscapeCsv(r.ServiceName ?? "");
            var desc = EscapeCsv(r.ServiceDescription ?? "");

            sb.AppendLine($"{EscapeCsv(r.Target)},{r.Port},{r.Protocol},{r.State},{service},{desc},{responseTime},{r.DetectionConfidence},{banner},{EscapeCsv(tags)},{notes},{r.IsFavorite}");
        }

        return sb.ToString();
    }

    private static string GenerateTxtContent(ReportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=================================================================");
        sb.AppendLine("  SECUREPORT SCAN REPORT");
        sb.AppendLine("=================================================================");
        sb.AppendLine($"  Title: {data.Title}");
        sb.AppendLine($"  Generated: {data.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"  Application: SecurePort v{Core.Constants.AppConstants.Version}");
        sb.AppendLine("=================================================================");
        sb.AppendLine();
        sb.AppendLine("  --- STATISTICS ---");
        sb.AppendLine($"  Total Scans:       {data.Statistics.TotalScans}");
        sb.AppendLine($"  Ports Scanned:     {data.Statistics.TotalPortsScanned}");
        sb.AppendLine($"  Open Ports:        {data.Statistics.OpenPortsCount}");
        sb.AppendLine($"  Closed Ports:      {data.Statistics.ClosedPortsCount}");
        sb.AppendLine($"  Other Ports:       {data.Statistics.OtherPortsCount}");
        sb.AppendLine();
        sb.AppendLine("  --- RESULTS ---");
        sb.AppendLine($"  {"Port",-8} {"Proto",-6} {"State",-10} {"Service",-20} {"Response",-12}");
        sb.AppendLine($"  {new string('-', 60)}");

        foreach (var r in data.Results)
        {
            var rt = r.ResponseTime?.TotalMilliseconds.ToString("F2") + "ms" ?? "N/A";
            sb.AppendLine($"  {r.Port,-8} {r.Protocol,-6} {r.State,-10} {(r.ServiceName ?? "—"),-20} {rt,-12}");
        }

        sb.AppendLine();
        sb.AppendLine("=================================================================");
        sb.AppendLine($"  Report generated by SecurePort v{Core.Constants.AppConstants.Version}");
        sb.AppendLine("=================================================================");

        return sb.ToString();
    }

    private static string GenerateHtmlContent(ReportData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"UTF-8\">");
        sb.AppendLine("<title>SecurePort Report</title><style>");
        sb.AppendLine("body{font-family:sans-serif;background:#f5f7fa;color:#333;padding:2rem;}");
        sb.AppendLine("h1{color:#1a237e;} table{width:100%;border-collapse:collapse;margin:1rem 0;}");
        sb.AppendLine("th{background:#283593;color:#fff;padding:8px;text-align:left;}");
        sb.AppendLine("td{padding:8px;border-bottom:1px solid #eee;}");
        sb.AppendLine(".open{color:#2e7d32;font-weight:bold;}");
        sb.AppendLine(".closed{color:#c62828;font-weight:bold;}</style></head><body>");
        sb.AppendLine($"<h1>{EscapeHtml(data.Title)}</h1>");
        sb.AppendLine($"<p>Generated: {data.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}</p>");
        sb.AppendLine("<h2>Statistics</h2><ul>");
        sb.AppendLine($"<li>Total Scans: {data.Statistics.TotalScans}</li>");
        sb.AppendLine($"<li>Open Ports: {data.Statistics.OpenPortsCount}</li>");
        sb.AppendLine($"<li>Closed Ports: {data.Statistics.ClosedPortsCount}</li></ul>");
        sb.AppendLine("<h2>Results</h2><table><tr><th>Port</th><th>Protocol</th><th>State</th><th>Service</th><th>Response</th></tr>");

        foreach (var r in data.Results)
        {
            var stateClass = r.State == Core.Enums.PortState.Open ? "open" : "closed";
            var rt = r.ResponseTime?.TotalMilliseconds.ToString("F2") + "ms" ?? "N/A";
            sb.AppendLine($"<tr><td>{r.Port}</td><td>{r.Protocol}</td><td class=\"{stateClass}\">{r.State}</td><td>{EscapeHtml(r.ServiceName ?? "—")}</td><td>{rt}</td></tr>");
        }

        sb.AppendLine("</table></body></html>");
        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string EscapeHtml(string text)
    {
        return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }

    private static string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.JSON => ".json",
        ExportFormat.CSV => ".csv",
        ExportFormat.TXT => ".txt",
        ExportFormat.HTML => ".html",
        ExportFormat.PDF => ".pdf",
        _ => ".json"
    };

    private readonly string _metadataFilePath;

    public ReportManager(
        IReportVerificationManager verificationManager,
        IVisualizationEngine visualizationEngine,
        IStatisticsEngine statisticsEngine,
        IScanComparisonEngine comparisonEngine,
        string reportsDirectory)
    {
        _verificationManager = verificationManager ?? throw new ArgumentNullException(nameof(verificationManager));
        _visualizationEngine = visualizationEngine ?? throw new ArgumentNullException(nameof(visualizationEngine));
        _statisticsEngine = statisticsEngine ?? throw new ArgumentNullException(nameof(statisticsEngine));
        _comparisonEngine = comparisonEngine ?? throw new ArgumentNullException(nameof(comparisonEngine));
        _reportsDirectory = reportsDirectory ?? throw new ArgumentNullException(nameof(reportsDirectory));
        _metadataFilePath = Path.Combine(_reportsDirectory, "reports_metadata.json");

        Directory.CreateDirectory(_reportsDirectory);
        LoadMetadataIndex();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _lock.Dispose();
            _disposed = true;
        }
    }

    private void LoadMetadataIndex()
    {

        if (!File.Exists(_metadataFilePath))
            return;

        try
        {
            var json = File.ReadAllText(_metadataFilePath);
            var loaded = JsonSerializer.Deserialize<List<ReportMetadata>>(json, s_jsonOptions);
            if (loaded is not null)
                _metadataIndex.AddRange(loaded);
        }
        catch
        {
            // Gracefully handle corrupted metadata
        }
    }

    private void SaveMetadataIndex()
    {
        try
        {
            var json = JsonSerializer.Serialize(_metadataIndex, s_jsonOptions);
            File.WriteAllText(_metadataFilePath, json);
        }
        catch
        {
            // Log but don't crash on metadata save failure
        }
    }
}
