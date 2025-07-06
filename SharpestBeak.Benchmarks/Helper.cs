using BenchmarkDotNet.Loggers;

namespace SharpestBeak.Benchmarks;

internal static class Helper
{
    public static readonly ILogger Logger = DynamicallyPrefixedLogger.CreateTimestampPrefixedLogger(ConsoleLogger.Default);
}