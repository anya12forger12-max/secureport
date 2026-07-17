using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;
using SecurePort.Results.Engines;

namespace SecurePort.Tests;

public class ResultsSearchEngineTests
{
    private readonly ResultsSearchEngine _engine = new();

    [Fact]
    public void Search_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.Search(null!, "query"));
    }

    [Fact]
    public void Search_NullQuery_ReturnsAllResults()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 443)
        };

        var found = _engine.Search(results, null!);
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void Search_WhitespaceQuery_ReturnsAllResults()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80)
        };

        var found = _engine.Search(results, "   ");
        Assert.Single(found);
    }

    [Fact]
    public void Search_SingleKeyword_FindsMatchingPort()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 443)
        };

        var found = _engine.Search(results, "80");
        Assert.Single(found);
        Assert.Equal(80, found[0].Port);
    }

    [Fact]
    public void Search_MultiKeywordAndLogic_MustMatchAll()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, serviceName: "HTTP"),
            TestDataFactory.CreateResult(port: 443, serviceName: "HTTPS"),
            TestDataFactory.CreateResult(port: 22, serviceName: "SSH")
        };

        var found = _engine.Search(results, "80 HTTP");
        Assert.Single(found);
        Assert.Equal(80, found[0].Port);
    }

    [Fact]
    public void Search_CaseInsensitive_FindsMatch()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, serviceName: "HTTP")
        };

        var found = _engine.Search(results, "http");
        Assert.Single(found);
    }

    [Fact]
    public void Search_MatchesTarget()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(target: "192.168.1.1"),
            TestDataFactory.CreateResult(target: "10.0.0.1")
        };

        var found = _engine.Search(results, "192.168");
        Assert.Single(found);
        Assert.Equal("192.168.1.1", found[0].Target);
    }

    [Fact]
    public void Search_NoMatches_ReturnsEmpty()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80)
        };

        var found = _engine.Search(results, "nonexistent");
        Assert.Empty(found);
    }

    [Fact]
    public void FindMatches_NullText_ReturnsEmpty()
    {
        var matches = _engine.FindMatches(null!, "query");
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_EmptyText_ReturnsEmpty()
    {
        var matches = _engine.FindMatches("", "query");
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_NullQuery_ReturnsEmpty()
    {
        var matches = _engine.FindMatches("hello world", null!);
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_FindsMultipleMatches()
    {
        var matches = _engine.FindMatches("foo bar foo", "foo");
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void FindMatches_CaseInsensitive()
    {
        var matches = _engine.FindMatches("Hello WORLD hello", "hello");
        Assert.Equal(2, matches.Count);
    }
}

public class ResultsFilterEngineTests
{
    private readonly ResultsFilterEngine _engine = new();

    private static List<ResultEntry> CreateTestData() => new()
    {
        TestDataFactory.CreateResult(port: 80, state: PortState.Open, protocol: ProtocolType.TCP, serviceName: "HTTP", scanProfile: "fast"),
        TestDataFactory.CreateResult(port: 443, state: PortState.Open, protocol: ProtocolType.TCP, serviceName: "HTTPS"),
        TestDataFactory.CreateResult(port: 22, state: PortState.Closed, protocol: ProtocolType.TCP, serviceName: "SSH"),
        TestDataFactory.CreateResult(port: 53, state: PortState.Open, protocol: ProtocolType.UDP, serviceName: "DNS", isFavorite: true),
        TestDataFactory.CreateResult(port: 8080, state: PortState.Filtered, protocol: ProtocolType.TCP, serviceName: "HTTP-Proxy", scanDate: DateTimeOffset.UtcNow.AddDays(-10))
    };

    [Fact]
    public void ApplyFilters_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.ApplyFilters(null!, new ResultFilterCriteria()));
    }

    [Fact]
    public void ApplyFilters_NullCriteria_ReturnsAll()
    {
        var results = CreateTestData();
        var filtered = _engine.ApplyFilters(results, null!);
        Assert.Equal(5, filtered.Count);
    }

    [Fact]
    public void ApplyFilters_StateFilter_FiltersCorrectly()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { StateFilter = PortState.Open };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Equal(3, filtered.Count);
        Assert.All(filtered, r => Assert.Equal(PortState.Open, r.State));
    }

    [Fact]
    public void ApplyFilters_ProtocolFilter_FiltersCorrectly()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { ProtocolFilter = ProtocolType.UDP };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
        Assert.Equal(53, filtered[0].Port);
    }

    [Fact]
    public void ApplyFilters_ServiceFilter_PartialMatch()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { ServiceFilter = "HTTP-Proxy" };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
    }

    [Fact]
    public void ApplyFilters_PortRange_FiltersCorrectly()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { PortRangeMin = 100, PortRangeMax = 500 };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
        Assert.Equal(443, filtered[0].Port);
    }

    [Fact]
    public void ApplyFilters_FavoritesOnly_FiltersCorrectly()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { FavoritesOnly = true };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
        Assert.Equal(53, filtered[0].Port);
    }

    [Fact]
    public void ApplyFilters_TargetFilter_PartialMatch()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria { TargetFilter = "192" };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Equal(5, filtered.Count);
    }

    [Fact]
    public void ApplyFilters_ScanIdFilter_FiltersCorrectly()
    {
        var scanId = Guid.NewGuid();
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(scanId: scanId, port: 80),
            TestDataFactory.CreateResult(port: 443)
        };
        var criteria = new ResultFilterCriteria { ScanIdFilter = scanId };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
    }

    [Fact]
    public void ApplyFilters_MultipleFilters_NarrowsResults()
    {
        var results = CreateTestData();
        var criteria = new ResultFilterCriteria
        {
            StateFilter = PortState.Open,
            ProtocolFilter = ProtocolType.TCP
        };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void ApplyFilters_ResponseTimeFilter_FiltersCorrectly()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, responseTime: TimeSpan.FromMilliseconds(10)),
            TestDataFactory.CreateResult(port: 443, responseTime: TimeSpan.FromMilliseconds(100)),
            TestDataFactory.CreateResult(port: 22, responseTime: null)
        };
        var criteria = new ResultFilterCriteria { MinResponseTime = TimeSpan.FromMilliseconds(50) };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
        Assert.Equal(443, filtered[0].Port);
    }

    [Fact]
    public void ApplyFilters_DateRange_FiltersCorrectly()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, scanDate: DateTimeOffset.UtcNow.AddDays(-1)),
            TestDataFactory.CreateResult(port: 443, scanDate: DateTimeOffset.UtcNow.AddDays(-10))
        };
        var criteria = new ResultFilterCriteria
        {
            DateRangeStart = DateTimeOffset.UtcNow.AddDays(-5)
        };
        var filtered = _engine.ApplyFilters(results, criteria);
        Assert.Single(filtered);
        Assert.Equal(80, filtered[0].Port);
    }
}

public class ResultsSortEngineTests
{
    private readonly ResultsSortEngine _engine = new();

    private static List<ResultEntry> CreateUnsortedData() => new()
    {
        TestDataFactory.CreateResult(port: 443, state: PortState.Open),
        TestDataFactory.CreateResult(port: 22, state: PortState.Closed),
        TestDataFactory.CreateResult(port: 80, state: PortState.Open)
    };

    [Fact]
    public void Sort_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.Sort(null!, new SortCriteria { Field = SortField.Port, Descending = false }));
    }

    [Fact]
    public void Sort_NullCriteria_ReturnsResults()
    {
        var results = CreateUnsortedData();
        var sorted = _engine.Sort(results, null!);
        Assert.Equal(3, sorted.Count);
    }

    [Fact]
    public void Sort_ByPort_Ascending()
    {
        var results = CreateUnsortedData();
        var sorted = _engine.Sort(results, new SortCriteria { Field = SortField.Port, Descending = false });
        Assert.Equal(new[] { 22, 80, 443 }, sorted.Select(r => r.Port));
    }

    [Fact]
    public void Sort_ByPort_Descending()
    {
        var results = CreateUnsortedData();
        var sorted = _engine.Sort(results, new SortCriteria { Field = SortField.Port, Descending = true });
        Assert.Equal(new[] { 443, 80, 22 }, sorted.Select(r => r.Port));
    }

    [Fact]
    public void Sort_ByService_NullServiceName_TreatedAsEmpty()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, serviceName: "HTTP"),
            TestDataFactory.CreateResult(port: 443, serviceName: null),
            TestDataFactory.CreateResult(port: 22, serviceName: "SSH")
        };
        var sorted = _engine.Sort(results, new SortCriteria { Field = SortField.Service, Descending = false });
        Assert.Equal(443, sorted[0].Port);
    }

    [Fact]
    public void Sort_ByResponseTime_NullTreatedAsMaxValue()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, responseTime: TimeSpan.FromMilliseconds(10)),
            TestDataFactory.CreateResult(port: 443, responseTime: null),
            TestDataFactory.CreateResult(port: 22, responseTime: TimeSpan.FromMilliseconds(50))
        };
        var sorted = _engine.Sort(results, new SortCriteria { Field = SortField.ResponseTime, Descending = false });
        Assert.Equal(80, sorted[0].Port);
        Assert.Equal(443, sorted[2].Port);
    }

    [Fact]
    public void Sort_DoesNotMutateInput()
    {
        var results = CreateUnsortedData();
        var originalOrder = results.Select(r => r.Port).ToList();
        _ = _engine.Sort(results, new SortCriteria { Field = SortField.Port, Descending = true });
        Assert.Equal(originalOrder, results.Select(r => r.Port));
    }
}

public class StatisticsEngineTests
{
    private readonly StatisticsEngine _engine = new();

    [Fact]
    public void ComputeStatistics_NullSessions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.ComputeStatistics(null!, new List<ResultEntry>()));
    }

    [Fact]
    public void ComputeStatistics_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.ComputeStatistics(new List<ScanSession>(), null!));
    }

    [Fact]
    public void ComputeStatistics_EmptyCollections_ReturnsZeros()
    {
        var stats = _engine.ComputeStatistics(new List<ScanSession>(), new List<ResultEntry>());
        Assert.Equal(0, stats.TotalScans);
        Assert.Equal(0, stats.OpenPortsCount);
        Assert.Null(stats.AverageScanDuration);
        Assert.Null(stats.AverageResponseTime);
    }

    [Fact]
    public void ComputeStatistics_CountsCompletedSessionsOnly()
    {
        var sessions = new List<ScanSession>
        {
            TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(5)),
            TestDataFactory.CreateSession(status: ScanStatus.Failed),
            TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(10))
        };
        var stats = _engine.ComputeStatistics(sessions, new List<ResultEntry>());
        Assert.Equal(2, stats.TotalScans);
        Assert.Equal(TimeSpan.FromSeconds(7.5), stats.AverageScanDuration);
    }

    [Fact]
    public void ComputeStatistics_CountsPortStates()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(state: PortState.Open),
            TestDataFactory.CreateResult(state: PortState.Open),
            TestDataFactory.CreateResult(state: PortState.Closed),
            TestDataFactory.CreateResult(state: PortState.Filtered)
        };
        var stats = _engine.ComputeStatistics(new List<ScanSession>(), results);
        Assert.Equal(2, stats.OpenPortsCount);
        Assert.Equal(1, stats.ClosedPortsCount);
        Assert.Equal(1, stats.OtherPortsCount);
    }

    [Fact]
    public void ComputeStatistics_AveragesResponseTime()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, responseTime: TimeSpan.FromMilliseconds(10)),
            TestDataFactory.CreateResult(port: 443, responseTime: TimeSpan.FromMilliseconds(50)),
            TestDataFactory.CreateResult(port: 22, responseTime: null)
        };
        var stats = _engine.ComputeStatistics(new List<ScanSession>(), results);
        Assert.Equal(TimeSpan.FromMilliseconds(30), stats.AverageResponseTime);
        Assert.Equal(TimeSpan.FromMilliseconds(10), stats.FastestResponse);
        Assert.Equal(TimeSpan.FromMilliseconds(50), stats.SlowestResponse);
    }

    [Fact]
    public void ComputeStatistics_MostCommonServices()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, serviceName: "HTTP"),
            TestDataFactory.CreateResult(port: 8080, serviceName: "HTTP"),
            TestDataFactory.CreateResult(port: 443, serviceName: "HTTPS")
        };
        var stats = _engine.ComputeStatistics(new List<ScanSession>(), results);
        Assert.Equal(2, stats.MostCommonServices.Count);
        Assert.Equal("HTTP", stats.MostCommonServices[0].ServiceName);
        Assert.Equal(2, stats.MostCommonServices[0].Count);
    }

    [Fact]
    public void ComputeStatistics_MostFrequentPorts()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 443)
        };
        var stats = _engine.ComputeStatistics(new List<ScanSession>(), results);
        Assert.Equal(80, stats.MostFrequentPorts[0].Port);
        Assert.Equal(2, stats.MostFrequentPorts[0].Count);
    }
}

public class ScanComparisonEngineTests
{
    private readonly ScanComparisonEngine _engine = new();

    [Fact]
    public void Compare_NullPrevious_ThrowsArgumentNullException()
    {
        var current = TestDataFactory.CreateSession();
        Assert.Throws<ArgumentNullException>(() => _engine.Compare(null!, current));
    }

    [Fact]
    public void Compare_NullCurrent_ThrowsArgumentNullException()
    {
        var previous = TestDataFactory.CreateSession();
        Assert.Throws<ArgumentNullException>(() => _engine.Compare(previous, null!));
    }

    [Fact]
    public void Compare_IdenticalScans_AllUnchanged()
    {
        var prev = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open),
            TestDataFactory.CreateScanResult(port: 443, state: PortState.Open) });
        var curr = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open),
            TestDataFactory.CreateScanResult(port: 443, state: PortState.Open) });

        var result = _engine.Compare(prev, curr);
        Assert.Equal(0, result.NewlyOpenCount);
        Assert.Equal(0, result.NewlyClosedCount);
        Assert.Equal(0, result.ServiceChangesCount);
        Assert.Equal(2, result.UnchangedCount);
    }

    [Fact]
    public void Compare_NewPort_DetectedAsNewlyOpen()
    {
        var prev = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open) });
        var curr = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open),
            TestDataFactory.CreateScanResult(port: 443, state: PortState.Open) });

        var result = _engine.Compare(prev, curr);
        Assert.Equal(1, result.NewlyOpenCount);
        var added = result.Differences.First(d => d.DifferenceType == DifferenceType.Added);
        Assert.Equal(443, added.Port);
    }

    [Fact]
    public void Compare_ClosedPort_DetectedAsNewlyClosed()
    {
        var prev = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open),
            TestDataFactory.CreateScanResult(port: 443, state: PortState.Open) });
        var curr = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, state: PortState.Open) });

        var result = _engine.Compare(prev, curr);
        Assert.Equal(1, result.NewlyClosedCount);
        var removed = result.Differences.First(d => d.DifferenceType == DifferenceType.Removed);
        Assert.Equal(443, removed.Port);
    }

    [Fact]
    public void Compare_ServiceChanged_Detected()
    {
        var prev = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, serviceName: "HTTP") });
        var curr = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80, serviceName: "nginx") });

        var result = _engine.Compare(prev, curr);
        Assert.Equal(1, result.ServiceChangesCount);
        Assert.Contains(result.Differences, d => d.DifferenceType == DifferenceType.ServiceChanged);
    }

    [Fact]
    public void CompareMultiple_LessThan2Sessions_ReturnsEmpty()
    {
        var sessions = new List<ScanSession> { TestDataFactory.CreateSession() };
        var results = _engine.CompareMultiple(sessions);
        Assert.Empty(results);
    }

    [Fact]
    public void CompareMultiple_CompareAdjacentPairs()
    {
        var s1 = TestDataFactory.CreateSession(results: new[] { TestDataFactory.CreateScanResult(port: 80) });
        var s2 = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80),
            TestDataFactory.CreateScanResult(port: 443) });
        var s3 = TestDataFactory.CreateSession(results: new[] { TestDataFactory.CreateScanResult(port: 80) });

        var results = _engine.CompareMultiple(new List<ScanSession> { s1, s2, s3 });
        Assert.Equal(2, results.Count);
    }
}

public class VisualizationEngineTests
{
    private readonly VisualizationEngine _engine = new();

    [Fact]
    public void GeneratePortStateChart_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.GeneratePortStateChart(null!));
    }

    [Fact]
    public void GeneratePortStateChart_ReturnsChartDataSet()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(state: PortState.Open),
            TestDataFactory.CreateResult(state: PortState.Closed)
        };
        var chart = _engine.GeneratePortStateChart(results);
        Assert.NotNull(chart);
        Assert.Equal("Open vs Closed Ports", chart.Title);
        Assert.NotEmpty(chart.Series);
        Assert.NotEmpty(chart.AccessibilitySummary);
    }

    [Fact]
    public void GenerateServiceDistributionChart_EmptyOpenPorts_ReturnsNoServicesMessage()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(state: PortState.Closed, serviceName: "HTTP")
        };
        var chart = _engine.GenerateServiceDistributionChart(results);
        Assert.NotNull(chart);
        var firstPoint = chart.Series[0].DataPoints[0];
        Assert.Equal("No services detected", firstPoint.Label);
    }

    [Fact]
    public void GeneratePortRangeChart_ReturnsAllRanges()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 8080)
        };
        var chart = _engine.GeneratePortRangeChart(results);
        Assert.NotNull(chart);
        Assert.Equal("Port Range Distribution", chart.Title);
    }

    [Fact]
    public void GenerateResponseTimeChart_ReturnsChartDataSet()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, responseTime: TimeSpan.FromMilliseconds(5)),
            TestDataFactory.CreateResult(port: 443, responseTime: TimeSpan.FromMilliseconds(200))
        };
        var chart = _engine.GenerateResponseTimeChart(results);
        Assert.NotNull(chart);
        Assert.Equal("Response Time Distribution", chart.Title);
    }

    [Fact]
    public void GenerateScanDurationTrendChart_NullSessions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.GenerateScanDurationTrendChart(null!));
    }

    [Fact]
    public void GenerateScanDurationTrendChart_OnlyCompletedSessions()
    {
        var sessions = new List<ScanSession>
        {
            TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(5)),
            TestDataFactory.CreateSession(status: ScanStatus.Failed, duration: TimeSpan.FromSeconds(10))
        };
        var chart = _engine.GenerateScanDurationTrendChart(sessions);
        Assert.NotNull(chart);
        Assert.Equal("Scan Duration Trend", chart.Title);
    }

    [Fact]
    public void GenerateServicesFrequencyChart_ReturnsChartDataSet()
    {
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80, serviceName: "HTTP"),
            TestDataFactory.CreateResult(port: 443, serviceName: "HTTPS")
        };
        var chart = _engine.GenerateServicesFrequencyChart(results);
        Assert.NotNull(chart);
        Assert.Equal("Services Frequency", chart.Title);
    }

    [Fact]
    public void GenerateHistoricalTimelineChart_ReturnsChartDataSet()
    {
        var sessions = new List<ScanSession>
        {
            TestDataFactory.CreateSession(status: ScanStatus.Completed, startTime: DateTimeOffset.UtcNow.AddDays(-1)),
            TestDataFactory.CreateSession(status: ScanStatus.Completed, startTime: DateTimeOffset.UtcNow)
        };
        var chart = _engine.GenerateHistoricalTimelineChart(sessions);
        Assert.NotNull(chart);
        Assert.Equal("Historical Scan Timeline", chart.Title);
    }
}

public class ReportPreparationEngineTests
{
    private readonly ReportPreparationEngine _engine;

    public ReportPreparationEngineTests()
    {
        _engine = new ReportPreparationEngine(
            new StatisticsEngine(),
            new VisualizationEngine(),
            new ScanComparisonEngine());
    }

    [Fact]
    public void Constructor_NullStatisticsEngine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReportPreparationEngine(null!, new VisualizationEngine(), new ScanComparisonEngine()));
    }

    [Fact]
    public void Constructor_NullVisualizationEngine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReportPreparationEngine(new StatisticsEngine(), null!, new ScanComparisonEngine()));
    }

    [Fact]
    public void Constructor_NullComparisonEngine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReportPreparationEngine(new StatisticsEngine(), new VisualizationEngine(), null!));
    }

    [Fact]
    public void PrepareReport_NullTitle_ThrowsArgumentException()
    {
        var sessions = new List<ScanSession>();
        var results = new List<ResultEntry>();
        Assert.Throws<ArgumentException>(() => _engine.PrepareReport(null!, sessions, results));
    }

    [Fact]
    public void PrepareReport_NullSessions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.PrepareReport("Title", null!, new List<ResultEntry>()));
    }

    [Fact]
    public void PrepareReport_NullResults_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.PrepareReport("Title", new List<ScanSession>(), null!));
    }

    [Fact]
    public void PrepareReport_ReturnsValidReportData()
    {
        var session = TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(5));
        var results = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80)
        };

        var report = _engine.PrepareReport("Test Report", new List<ScanSession> { session }, results);

        Assert.Equal("Test Report", report.Title);
        Assert.Single(report.Sessions);
        Assert.Single(report.Results);
        Assert.NotNull(report.Statistics);
        Assert.NotEmpty(report.Charts);
        Assert.True(report.GeneratedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void PrepareReport_MultipleSessions_GeneratesMoreCharts()
    {
        var sessions = new List<ScanSession>
        {
            TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(5)),
            TestDataFactory.CreateSession(status: ScanStatus.Completed, duration: TimeSpan.FromSeconds(10))
        };
        var report = _engine.PrepareReport("Multi-Session", sessions, new List<ResultEntry>());
        Assert.True(report.Charts.Count >= 5);
    }

    [Fact]
    public void PrepareComparisonReport_ReturnsValidReport()
    {
        var prev = TestDataFactory.CreateSession(results: new[] { TestDataFactory.CreateScanResult(port: 80) });
        var curr = TestDataFactory.CreateSession(results: new[] {
            TestDataFactory.CreateScanResult(port: 80),
            TestDataFactory.CreateScanResult(port: 443) });
        var prevResults = new List<ResultEntry> { TestDataFactory.CreateResult(port: 80) };
        var currResults = new List<ResultEntry>
        {
            TestDataFactory.CreateResult(port: 80),
            TestDataFactory.CreateResult(port: 443)
        };

        var report = _engine.PrepareComparisonReport("Comparison", prev, curr, prevResults, currResults);
        Assert.NotNull(report.Comparisons);
        Assert.Single(report.Comparisons);
    }
}
