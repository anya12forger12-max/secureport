using System.Diagnostics;
using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.Results.Engines;
using SecurePort.Security.Validation;

namespace SecurePort.Benchmarks;

/// <summary>
/// Lightweight benchmark runner for SecurePort engines and managers.
/// Run with: dotnet run --project benchmarks -c Release
/// </summary>
public static class Program
{
    private const int WarmupIterations = 100;
    private const int BenchmarkIterations = 10_000;

    public static async Task Main()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           SecurePort Performance Benchmarks                ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"  Warmup: {WarmupIterations} iterations | Benchmark: {BenchmarkIterations} iterations");
        Console.WriteLine();

        await BenchmarkSearchEngine();
        await BenchmarkFilterEngine();
        await BenchmarkSortEngine();
        await BenchmarkStatisticsEngine();
        BenchmarkTargetValidator();
        BenchmarkPortValidator();

        Console.WriteLine();
        Console.WriteLine("All benchmarks completed.");
    }

    private static async Task BenchmarkSearchEngine()
    {
        var engine = new ResultsSearchEngine();
        var entries = GenerateResultEntries(1000);
        foreach (var entry in entries)
            await engine.IndexAsync(entry, CancellationToken.None);

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            await engine.SearchAsync("ssh", CancellationToken.None);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            await engine.SearchAsync("http", CancellationToken.None);
        sw.Stop();

        PrintResult("ResultsSearchEngine.SearchAsync", BenchmarkIterations, sw.ElapsedMilliseconds);
    }

    private static async Task BenchmarkFilterEngine()
    {
        var engine = new ResultsFilterEngine();
        var entries = GenerateResultEntries(1000);
        var criteria = new ResultFilterCriteria { PortMin = 80, PortMax = 443 };

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            engine.Filter(entries, criteria);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            engine.Filter(entries, criteria);
        sw.Stop();

        PrintResult("ResultsFilterEngine.Filter", BenchmarkIterations, sw.ElapsedMilliseconds);
    }

    private static async Task BenchmarkSortEngine()
    {
        var engine = new ResultsSortEngine();
        var entries = GenerateResultEntries(1000);
        var criteria = new SortCriteria { Field = SortField.Port, Descending = false };

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            engine.Sort(entries, criteria);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            engine.Sort(entries, criteria);
        sw.Stop();

        PrintResult("ResultsSortEngine.Sort", BenchmarkIterations, sw.ElapsedMilliseconds);
    }

    private static async Task BenchmarkStatisticsEngine()
    {
        var engine = new StatisticsEngine();
        var entries = GenerateResultEntries(100);

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            engine.CalculateStatistics(entries);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            engine.CalculateStatistics(entries);
        sw.Stop();

        PrintResult("StatisticsEngine.CalculateStatistics", BenchmarkIterations, sw.ElapsedMilliseconds);
    }

    private static void BenchmarkTargetValidator()
    {
        var validator = new TargetValidator();
        var targets = new[] { "192.168.1.1", "example.com", "10.0.0.1", "invalid..host", "" };

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            foreach (var t in targets) validator.Validate(t);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            foreach (var t in targets) validator.Validate(t);
        sw.Stop();

        PrintResult("TargetValidator.Validate", BenchmarkIterations * targets.Length, sw.ElapsedMilliseconds);
    }

    private static void BenchmarkPortValidator()
    {
        var validator = new PortValidator();
        var ports = new[] { 80, 443, 22, 3306, 65535, 0, -1, 99999 };

        // Warmup
        for (int i = 0; i < WarmupIterations; i++)
            foreach (var p in ports) validator.ValidatePort(p);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < BenchmarkIterations; i++)
            foreach (var p in ports) validator.ValidatePort(p);
        sw.Stop();

        PrintResult("PortValidator.ValidatePort", BenchmarkIterations * ports.Length, sw.ElapsedMilliseconds);
    }

    private static void PrintResult(string name, long operations, long elapsedMs)
    {
        var opsPerSec = elapsedMs > 0 ? (operations * 1000.0 / elapsedMs) : 0;
        Console.WriteLine($"  {name,-40} {elapsedMs,6} ms  ({opsPerSec:N0} ops/sec)");
    }

    private static IReadOnlyList<ResultEntry> GenerateResultEntries(int count)
    {
        var entries = new List<ResultEntry>();
        var random = new Random(42);
        var states = new[] { PortState.Open, PortState.Closed, PortState.Filtered, PortState.Timeout };
        var services = new[] { "http", "ssh", "ftp", "smtp", "dns", "https", "mysql", "rdp", null };

        for (int i = 0; i < count; i++)
        {
            entries.Add(new ResultEntry
            {
                Id = Guid.NewGuid(),
                ScanId = Guid.NewGuid(),
                Target = $"192.168.{random.Next(1, 255)}.{random.Next(1, 255)}",
                Port = random.Next(1, 65535),
                Protocol = ProtocolType.TCP,
                State = states[random.Next(states.Length)],
                ServiceName = services[random.Next(services.Length)],
                ServiceDescription = null,
                ResponseTime = TimeSpan.FromMilliseconds(random.Next(1, 5000)),
                Banner = random.Next(3) == 0 ? "SSH-2.0-OpenSSH_8.9" : null,
                DetectionConfidence = random.NextDouble(),
                Tags = Array.Empty<string>(),
                Notes = null,
                IsFavorite = random.Next(5) == 0,
                ScanDate = DateTimeOffset.UtcNow.AddMinutes(-random.Next(0, 10000)),
                ScanProfile = "default"
            });
        }

        return entries;
    }
}
