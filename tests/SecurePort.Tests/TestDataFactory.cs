using SecurePort.Core.Enums;
using SecurePort.Core.Models;

namespace SecurePort.Tests;

internal static class TestDataFactory
{
    public static ResultEntry CreateResult(
        int port = 80,
        PortState state = PortState.Open,
        ProtocolType protocol = ProtocolType.TCP,
        string? serviceName = "HTTP",
        string target = "192.168.1.1",
        Guid? scanId = null,
        DateTimeOffset? scanDate = null,
        TimeSpan? responseTime = null,
        bool isFavorite = false,
        string[]? tags = null,
        string? scanProfile = null,
        string? notes = null)
    {
        return new ResultEntry
        {
            Id = Guid.NewGuid(),
            ScanId = scanId ?? Guid.NewGuid(),
            Target = target,
            Port = port,
            Protocol = protocol,
            State = state,
            DetectionConfidence = 85,
            Tags = tags ?? Array.Empty<string>(),
            IsFavorite = isFavorite,
            ScanDate = scanDate ?? DateTimeOffset.UtcNow,
            ServiceName = serviceName,
            ServiceDescription = $"Service on port {port}",
            ResponseTime = responseTime,
            ScanProfile = scanProfile,
            Notes = notes
        };
    }

    public static ScanSession CreateSession(
        ScanStatus status = ScanStatus.Completed,
        DateTimeOffset? startTime = null,
        TimeSpan? duration = null,
        ScanResult[]? results = null)
    {
        var target = new ScanTarget
        {
            Host = "192.168.1.1",
            PortStart = 1,
            PortEnd = 1024,
            Protocol = ProtocolType.TCP,
            ScanType = ScanType.ConnectScan,
            Timeout = TimeSpan.FromSeconds(3),
            MaxConcurrentConnections = 100,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return new ScanSession
        {
            Id = Guid.NewGuid(),
            Target = target,
            Results = results?.Length > 0 ? results : Array.Empty<ScanResult>(),
            Status = status,
            StartTime = startTime ?? DateTimeOffset.UtcNow,
            TotalPortsScanned = results?.Length ?? 0,
            OpenPortsFound = results?.Count(r => r.State == PortState.Open) ?? 0,
            FilteredPortsFound = results?.Count(r => r.State == PortState.Filtered) ?? 0,
            ClosedPortsFound = results?.Count(r => r.State == PortState.Closed) ?? 0,
            ErrorsEncountered = 0,
            Duration = duration
        };
    }

    public static ScanResult CreateScanResult(
        string host = "192.168.1.1",
        int port = 80,
        PortState state = PortState.Open,
        ProtocolType protocol = ProtocolType.TCP,
        string? serviceName = "HTTP")
    {
        return new ScanResult
        {
            Host = host,
            Port = port,
            State = state,
            Protocol = protocol,
            ScannedAt = DateTimeOffset.UtcNow,
            ServiceName = serviceName
        };
    }

    public static ReportData CreateReportData(string? title = null)
    {
        var stats = new ResultsStatistics
        {
            TotalScans = 1,
            TotalPortsScanned = 100,
            OpenPortsCount = 10,
            ClosedPortsCount = 90,
            OtherPortsCount = 0,
            MostCommonServices = new List<ServiceFrequency>(),
            MostFrequentPorts = new List<PortFrequency>(),
            ScanFrequencyOverTime = new List<DailyScanCount>()
        };

        return new ReportData
        {
            Title = title ?? "Test Report",
            GeneratedAt = DateTimeOffset.UtcNow,
            Sessions = new List<ScanSession>(),
            Results = new List<ResultEntry>(),
            Statistics = stats,
            Charts = new List<ChartDataSet>()
        };
    }

    public static ReportMetadata CreateReportMetadata(
        Guid? reportId = null,
        string title = "Test Report",
        ExportFormat format = ExportFormat.JSON)
    {
        return new ReportMetadata
        {
            ReportId = reportId ?? Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTimeOffset.UtcNow,
            ApplicationVersion = "1.0.0",
            OperatingSystem = "Linux",
            Format = format,
            FileSizeBytes = 1024,
            Checksum = "abc123",
            ScanId = Guid.NewGuid(),
            TargetHost = "192.168.1.1",
            FilePath = "/tmp/test.json",
            IsArchived = false,
            Tags = new List<string>()
        };
    }
}
