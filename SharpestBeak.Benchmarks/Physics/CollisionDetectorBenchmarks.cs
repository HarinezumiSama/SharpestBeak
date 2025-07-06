using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using Newtonsoft.Json;
using Omnifactotum;
using SharpestBeak.Benchmarks.Properties;
using SharpestBeak.Physics;
using SharpestBeak.Recording;

namespace SharpestBeak.Benchmarks.Physics;

[SimpleJob(RuntimeMoniker.HostProcess)]
[MemoryDiagnoser]
[IterationCount(10)]
[WarmupCount(5)]
[ProcessCount(1)]
public class CollisionDetectorBenchmarks
{
    private static readonly Process CurrentProcess = Process.GetCurrentProcess();
    private static readonly List<CollisionCheckData> CachedCollisionCheckDatas = [];

    private List<CollisionCheckData>? _collisionCheckDatas;

    [GlobalSetup]
    public void ExecuteGlobalSetup()
    {
        Helper.Logger.WriteLineInfo(
            $"[{nameof(ExecuteGlobalSetup)}] PID: {CurrentProcess.Id}. Process file: {(CurrentProcess.MainModule?.FileName).ToUIString()}. CPU count: {
                Environment.ProcessorCount}");

        _collisionCheckDatas = null;
        ReadData();

        lock (CachedCollisionCheckDatas)
        {
            _collisionCheckDatas = new List<CollisionCheckData>(CachedCollisionCheckDatas);
        }

        TestCollisionDetector();
    }

    [Benchmark(Baseline = true)]
    public void BenchmarkCollisionDetector()
    {
        //// Helper.Logger.WriteLineInfo($"[{nameof(BenchmarkCollisionDetector)}] Starting.");
        foreach (var data in _collisionCheckDatas.EnsureNotNull())
        {
            CollisionDetector.CheckCollision(data.First, data.Second);
        }

        ////Helper.Logger.WriteLineInfo($"[{nameof(BenchmarkCollisionDetector)}] Completed.");
    }

    //// Private methods

    private static void ReadData()
    {
        lock (CachedCollisionCheckDatas)
        {
            if (CachedCollisionCheckDatas.Count != 0)
            {
                return;
            }

            Helper.Logger.WriteLineHint($"[{nameof(ReadData)}] Started.");

            CachedCollisionCheckDatas.Clear();

            var benchmarksFilePath = Path.GetFullPath(Settings.Default.CollisionDetectorBenchmarksFilePath.EnsureNotNull());
            if (!File.Exists(benchmarksFilePath))
            {
                throw new FileNotFoundException($"The required file {benchmarksFilePath.ToUIString()} is not found.", benchmarksFilePath);
            }

            var serializer = CollisionCheckRecorder.CreateSerializer();

            Helper.Logger.WriteLineInfo($"[{nameof(ReadData)}] Reading {benchmarksFilePath.ToUIString()}.");

            string[] entryNames;
            using (var zipArchive = ZipFile.OpenRead(benchmarksFilePath))
            {
                entryNames = zipArchive.Entries.Select(static entry => entry.Name).OrderBy(Factotum.For<string>.IdentityMethod).ToArray();
                Helper.Logger.WriteLineInfo($"[{nameof(ReadData)}] Entries: {entryNames.Length}.");
            }

            var datas = entryNames
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .SelectMany(
                    (name, index) =>
                    {
                        Helper.Logger.WriteLineInfo($"[{nameof(ReadData)}] Reading entry {name.ToUIString()} ({index + 1} of {entryNames.Length})...");

                        using var archive = ZipFile.OpenRead(benchmarksFilePath);

                        var entry = archive.GetEntry(name).EnsureNotNull();
                        using var stream = entry.Open();
                        using var streamReader = new StreamReader(stream);
                        using var jsonTextReader = new JsonTextReader(streamReader);

                        var readDatas = serializer.Deserialize<CollisionCheckData[]>(jsonTextReader).EnsureNotNull();

                        Helper.Logger.WriteLineInfo($"[{nameof(ReadData)}] Reading entry {name.ToUIString()} ({index + 1} of {entryNames.Length}) - DONE.");
                        return readDatas;
                    })
                .OrderBy(static data => data.Index)
                .ToArray();

            if (datas.Length == 0)
            {
                throw new InvalidOperationException($"No benchmark data in {benchmarksFilePath.ToUIString()}.");
            }

            CachedCollisionCheckDatas.AddRange(datas);

            var counts = datas
                .GroupBy(
                    static data =>
                    {
                        var arr = new[] { CollisionCheckRecorder.GetSerializedName(data.First), CollisionCheckRecorder.GetSerializedName(data.Second) }
                            .OrderBy(Factotum.For<string>.IdentityMethod)
                            .ToArray();

                        return (arr[0], arr[1]);
                    })
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

            var countsString = counts
                .OrderByDescending(pair => pair.Value)
                .Select(pair => $"{pair.Key}: {pair.Value:N0} ({(decimal)pair.Value / datas.Length:P1})")
                .Join(",\x0020");

            Helper.Logger.WriteLineHint($"[{nameof(ReadData)}] Finished. Items: {CachedCollisionCheckDatas.Count:N0}. Counts: {countsString}.");
        }
    }

    [SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
    private void TestCollisionDetector()
    {
        var collisionCheckDatas = _collisionCheckDatas.EnsureNotNull();

        var resultCounts = collisionCheckDatas.Aggregate(
            (False: 0, True: 0),
            ((acc, data) => data.Result ? (acc.False, acc.True + 1) : (acc.False + 1, acc.True)));

        Helper.Logger.WriteLineInfo($"[{nameof(TestCollisionDetector)}] Starting. Items: {collisionCheckDatas.Count:N0}. Results: {resultCounts}.");

        var errorCount = 0;
        foreach (var data in collisionCheckDatas)
        {
            if (CollisionDetector.CheckCollision(data.First, data.Second) != data.Result)
            {
                errorCount++;
            }
        }

        if (errorCount != 0)
        {
            throw new Exception($"{nameof(TestCollisionDetector)} failed. Error count: {errorCount}.");
        }

        Helper.Logger.WriteLineInfo($"[{nameof(TestCollisionDetector)}] Completed successfully.");
    }
}