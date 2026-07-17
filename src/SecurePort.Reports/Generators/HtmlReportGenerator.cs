using System.Text;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Reports.Generators;

/// <summary>
/// Generates self-contained HTML reports with embedded CSS, summary statistics, and color-coded port results.
/// </summary>
public sealed class HtmlReportGenerator : IReportGenerator
{
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

        var html = BuildHtml(session, options);
        var bytes = Encoding.UTF8.GetBytes(html);
        return Task.FromResult(bytes);
    }

    /// <inheritdoc />
    public string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.HTML => ".html",
        _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
    };

    private static string BuildHtml(ScanSession session, ExportOptions options)
    {
        var results = options.IncludeClosedPorts
            ? session.Results
            : session.Results.Where(r => r.State != PortState.Closed).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>SecurePort Scan Report</title>");
        AppendStyles(sb);
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        AppendHeader(sb, session);
        AppendSummary(sb, session, results);

        if (options.IncludeScanMetadata)
        {
            AppendMetadata(sb, session);
        }

        AppendResultsTable(sb, results, options);
        AppendFooter(sb);

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void AppendStyles(StringBuilder sb)
    {
        sb.AppendLine("<style>");
        sb.AppendLine("  *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }");
        sb.AppendLine("  body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: #f5f7fa; color: #333; line-height: 1.6; padding: 2rem; }");
        sb.AppendLine("  .container { max-width: 1100px; margin: 0 auto; }");
        sb.AppendLine("  header { background: linear-gradient(135deg, #1a237e, #283593); color: #fff; padding: 2rem; border-radius: 8px 8px 0 0; }");
        sb.AppendLine("  header h1 { font-size: 1.75rem; font-weight: 600; }");
        sb.AppendLine("  header p { opacity: 0.85; margin-top: 0.25rem; font-size: 0.95rem; }");
        sb.AppendLine("  .card { background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); margin-bottom: 1.5rem; overflow: hidden; }");
        sb.AppendLine("  .card-title { font-size: 1.1rem; font-weight: 600; padding: 1rem 1.5rem; border-bottom: 1px solid #e0e0e0; background: #fafafa; }");
        sb.AppendLine("  .card-body { padding: 1.5rem; }");
        sb.AppendLine("  .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: 1rem; }");
        sb.AppendLine("  .stat-item { text-align: center; padding: 1rem; border-radius: 6px; background: #f5f7fa; }");
        sb.AppendLine("  .stat-value { font-size: 1.75rem; font-weight: 700; }");
        sb.AppendLine("  .stat-label { font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.05em; color: #666; margin-top: 0.25rem; }");
        sb.AppendLine("  .stat-open .stat-value { color: #2e7d32; }");
        sb.AppendLine("  .stat-closed .stat-value { color: #c62828; }");
        sb.AppendLine("  .stat-filtered .stat-value { color: #f57f17; }");
        sb.AppendLine("  .stat-total .stat-value { color: #1565c0; }");
        sb.AppendLine("  .metadata-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem 2rem; font-size: 0.9rem; }");
        sb.AppendLine("  .metadata-grid dt { font-weight: 600; color: #555; }");
        sb.AppendLine("  .metadata-grid dd { margin-bottom: 0.5rem; }");
        sb.AppendLine("  table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }");
        sb.AppendLine("  thead th { background: #283593; color: #fff; padding: 0.75rem 1rem; text-align: left; font-weight: 500; white-space: nowrap; }");
        sb.AppendLine("  tbody td { padding: 0.6rem 1rem; border-bottom: 1px solid #eee; }");
        sb.AppendLine("  tbody tr:hover { background: #f5f7fa; }");
        sb.AppendLine("  .state-open { background: #e8f5e9; color: #2e7d32; padding: 0.2rem 0.6rem; border-radius: 4px; font-weight: 600; display: inline-block; }");
        sb.AppendLine("  .state-closed { background: #ffebee; color: #c62828; padding: 0.2rem 0.6rem; border-radius: 4px; font-weight: 600; display: inline-block; }");
        sb.AppendLine("  .state-filtered { background: #fff8e1; color: #f57f17; padding: 0.2rem 0.6rem; border-radius: 4px; font-weight: 600; display: inline-block; }");
        sb.AppendLine("  .state-unknown, .state-timeout { background: #f3e5f5; color: #6a1b9a; padding: 0.2rem 0.6rem; border-radius: 4px; font-weight: 600; display: inline-block; }");
        sb.AppendLine("  footer { text-align: center; padding: 1.5rem; font-size: 0.8rem; color: #999; }");
        sb.AppendLine("  .no-results { padding: 2rem; text-align: center; color: #999; font-style: italic; }");
        sb.AppendLine("</style>");
    }

    private static void AppendHeader(StringBuilder sb, ScanSession session)
    {
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine("<header>");
        sb.AppendLine("  <h1>SecurePort Scan Report</h1>");
        sb.AppendLine($"  <p>Target: {EscapeHtml(session.Target.Host)} &mdash; Ports {session.Target.PortStart}&ndash;{session.Target.PortEnd} ({session.Target.Protocol})</p>");
        sb.AppendLine("</header>");
    }

    private static void AppendSummary(StringBuilder sb, ScanSession session, IReadOnlyList<ScanResult> results)
    {
        sb.AppendLine("<div class=\"card\">");
        sb.AppendLine("  <div class=\"card-title\">Summary</div>");
        sb.AppendLine("  <div class=\"card-body\">");
        sb.AppendLine("    <div class=\"stats-grid\">");

        sb.AppendLine($"      <div class=\"stat-item stat-total\">");
        sb.AppendLine($"        <div class=\"stat-value\">{session.TotalPortsScanned}</div>");
        sb.AppendLine($"        <div class=\"stat-label\">Total Scanned</div>");
        sb.AppendLine($"      </div>");

        sb.AppendLine($"      <div class=\"stat-item stat-open\">");
        sb.AppendLine($"        <div class=\"stat-value\">{session.OpenPortsFound}</div>");
        sb.AppendLine($"        <div class=\"stat-label\">Open</div>");
        sb.AppendLine($"      </div>");

        sb.AppendLine($"      <div class=\"stat-item stat-closed\">");
        sb.AppendLine($"        <div class=\"stat-value\">{session.ClosedPortsFound}</div>");
        sb.AppendLine($"        <div class=\"stat-label\">Closed</div>");
        sb.AppendLine($"      </div>");

        sb.AppendLine($"      <div class=\"stat-item stat-filtered\">");
        sb.AppendLine($"        <div class=\"stat-value\">{session.FilteredPortsFound}</div>");
        sb.AppendLine($"        <div class=\"stat-label\">Filtered</div>");
        sb.AppendLine($"      </div>");

        sb.AppendLine($"      <div class=\"stat-item\">");
        sb.AppendLine($"        <div class=\"stat-value\">{results.Count}</div>");
        sb.AppendLine($"        <div class=\"stat-label\">Results Shown</div>");
        sb.AppendLine($"      </div>");

        sb.AppendLine("    </div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");
    }

    private static void AppendMetadata(StringBuilder sb, ScanSession session)
    {
        sb.AppendLine("<div class=\"card\">");
        sb.AppendLine("  <div class=\"card-title\">Scan Metadata</div>");
        sb.AppendLine("  <div class=\"card-body\">");
        sb.AppendLine("    <dl class=\"metadata-grid\">");

        AppendMetadataEntry(sb, "Session ID", session.Id.ToString("D"));
        AppendMetadataEntry(sb, "Status", session.Status.ToString());
        AppendMetadataEntry(sb, "Start Time", session.StartTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        AppendMetadataEntry(sb, "End Time", session.EndTime?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "N/A");
        AppendMetadataEntry(sb, "Duration", session.Duration?.ToString(@"hh\:mm\:ss\.fff") ?? "N/A");
        AppendMetadataEntry(sb, "Scan Type", session.Target.ScanType.ToString());
        AppendMetadataEntry(sb, "Timeout", session.Target.Timeout.ToString());
        AppendMetadataEntry(sb, "Max Concurrent", session.Target.MaxConcurrentConnections.ToString());
        AppendMetadataEntry(sb, "Errors", session.ErrorsEncountered.ToString());

        if (!string.IsNullOrEmpty(session.ErrorMessage))
        {
            AppendMetadataEntry(sb, "Error Message", session.ErrorMessage);
        }

        sb.AppendLine("    </dl>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");
    }

    private static void AppendMetadataEntry(StringBuilder sb, string label, string value)
    {
        sb.AppendLine($"      <dt>{EscapeHtml(label)}</dt>");
        sb.AppendLine($"      <dd>{EscapeHtml(value)}</dd>");
    }

    private static void AppendResultsTable(StringBuilder sb, IReadOnlyList<ScanResult> results, ExportOptions options)
    {
        sb.AppendLine("<div class=\"card\">");
        sb.AppendLine("  <div class=\"card-title\">Port Results</div>");

        if (results.Count == 0)
        {
            sb.AppendLine("  <div class=\"no-results\">No port results to display.</div>");
            sb.AppendLine("</div>");
            return;
        }

        sb.AppendLine("  <div class=\"card-body\" style=\"padding:0;\">");
        sb.AppendLine("  <table>");
        sb.AppendLine("    <thead>");
        sb.AppendLine("      <tr>");
        sb.AppendLine("        <th>Host</th>");
        sb.AppendLine("        <th>Port</th>");
        sb.AppendLine("        <th>Protocol</th>");
        sb.AppendLine("        <th>State</th>");
        sb.AppendLine("        <th>Service</th>");
        sb.AppendLine("        <th>Banner</th>");
        sb.AppendLine("        <th>Response (ms)</th>");

        if (options.IncludeTimestamps)
        {
            sb.AppendLine("        <th>Scanned At</th>");
        }

        sb.AppendLine("      </tr>");
        sb.AppendLine("    </thead>");
        sb.AppendLine("    <tbody>");

        foreach (var result in results)
        {
            sb.AppendLine("      <tr>");
            sb.AppendLine($"        <td>{EscapeHtml(result.Host)}</td>");
            sb.AppendLine($"        <td>{result.Port}</td>");
            sb.AppendLine($"        <td>{result.Protocol}</td>");
            sb.AppendLine($"        <td><span class=\"state-{result.State.ToString().ToLowerInvariant()}\">{result.State}</span></td>");
            sb.AppendLine($"        <td>{EscapeHtml(result.ServiceName ?? "&mdash;")}</td>");
            sb.AppendLine($"        <td>{EscapeHtml(TruncateBanner(result.BannerInfo))}</td>");
            sb.AppendLine($"        <td>{result.ResponseTime?.TotalMilliseconds.ToString("F2") ?? "&mdash;"}</td>");

            if (options.IncludeTimestamps)
            {
                sb.AppendLine($"        <td>{result.ScannedAt:yyyy-MM-dd HH:mm:ss}</td>");
            }

            sb.AppendLine("      </tr>");
        }

        sb.AppendLine("    </tbody>");
        sb.AppendLine("  </table>");
        sb.AppendLine("  </div>");
        sb.AppendLine("</div>");
    }

    private static void AppendFooter(StringBuilder sb)
    {
        sb.AppendLine($"<footer>Generated by SecurePort &mdash; {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss UTC}</footer>");
        sb.AppendLine("</div>");
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string TruncateBanner(string? banner, int maxLength = 60)
    {
        if (string.IsNullOrEmpty(banner))
        {
            return "&mdash;";
        }

        var escaped = EscapeHtml(banner);
        return escaped.Length <= maxLength ? escaped : escaped[..maxLength] + "&hellip;";
    }
}
