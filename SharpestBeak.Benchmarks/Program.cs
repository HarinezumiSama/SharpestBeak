using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using SharpestBeak.Benchmarks;
using SharpestBeak.Benchmarks.Physics;

try
{
    var defaultConfig = DefaultConfig.Instance;

    IColumn[] columns =
    [
        BaselineColumn.Default,
        StatisticColumn.Min,
        StatisticColumn.Mean,
        StatisticColumn.Median,
        StatisticColumn.Max,
        StatisticColumn.StdDev,
        StatisticColumn.OperationsPerSecond,
        StatisticColumn.MValue,
        BaselineRatioColumn.RatioMean
    ];

    IExporter[] exporters =
    [
        DefaultExporters.Plain,
        DefaultExporters.Html,
        DefaultExporters.Csv,
        //// DefaultExporters.JsonFull,
        MarkdownExporter.GitHub
        //// DefaultExporters.RPlot
    ];

    var validators = defaultConfig.GetValidators().Append(ReturnValueValidator.FailOnError).Distinct().ToArray();

    var artifactsPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location).EnsureNotNull(), """..\..\.benchmarks"""));

    var config = ManualConfig.CreateEmpty()
        .AddLogger(Helper.Logger)
        .AddColumnProvider(defaultConfig.GetColumnProviders().ToArray())
        .AddColumn(columns)
        .AddExporter(exporters)
        .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
        .AddValidator(validators)
        .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest))
        .WithCategoryDiscoverer(defaultConfig.CategoryDiscoverer!)
        .WithUnionRule(ConfigUnionRule.Union)
        .WithCultureInfo(defaultConfig.CultureInfo!)
        .WithOptions(defaultConfig.Options)
        .WithSummaryStyle(defaultConfig.SummaryStyle)
        .WithBuildTimeout(defaultConfig.BuildTimeout)
        .WithArtifactsPath(artifactsPath);

    var summaries = BenchmarkSwitcher
        .FromTypes([typeof(CollisionDetectorBenchmarks)])
        .RunAll(config, args)
        .ToArray();

    if (summaries.Length == 0)
    {
        if (args is ["--list", ..])
        {
            return 0;
        }

        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("No benchmarks were run.");
        Console.ResetColor();

        Console.WriteLine();

        return 1;
    }

    var failedSummaries = summaries.Where(s => s.ValidationErrors.Any() || s.Reports.Any(r => !r.Success)).ToArray();
    if (failedSummaries.Length != 0)
    {
        var details = string.Join(",\x0020", failedSummaries.Select(s => $"'{s.Title}'"));
        throw new ApplicationException($"The following benchmarks failed: [{details}]");
    }
}
catch (Exception ex)
{
    Console.ResetColor();
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex);
    Console.ResetColor();

    Console.WriteLine();
    return sbyte.MaxValue;
}

return 0;