using System;
using System.Collections.Generic;
using SharpestBeak.Diagnostics;

namespace SharpestBeak;

public sealed class ThreadSafeRandom : Random
{
    private static readonly object InstanceCountLock = new();
    private static ulong _instanceCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThreadSafeRandom"/> class
    ///     using the specified seed value.
    /// </summary>
    public ThreadSafeRandom(int seed)
        : base(seed)
    {
        InitialSeed = seed;
        InstanceId = GetNextInstanceId();

        DebugHelper.WriteLineForced(
            "{0} #{1}: InitialSeed = {2}",
            GetType().FullName,
            InstanceId,
            InitialSeed);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThreadSafeRandom"/> class.
    /// </summary>
    public ThreadSafeRandom()
        : this(Environment.TickCount)
    {
        // Nothing to do
    }

    public ulong InstanceId { get; }

    public int InitialSeed { get; }

    public object SyncLock { get; } = new();

    public override int Next()
    {
        lock (SyncLock)
        {
            return base.Next();
        }
    }

    public override int Next(int minValue, int maxValue)
    {
        lock (SyncLock)
        {
            return base.Next(minValue, maxValue);
        }
    }

    public override int Next(int maxValue)
    {
        lock (SyncLock)
        {
            return base.Next(maxValue);
        }
    }

    public override void NextBytes(byte[] buffer)
    {
        lock (SyncLock)
        {
            base.NextBytes(buffer);
        }
    }

    public override double NextDouble()
    {
        lock (SyncLock)
        {
            return base.NextDouble();
        }
    }

    public override string ToString() => $"{GetType().Name}. InstanceId = {InstanceId}, InitialSeed = {InitialSeed}";

    public T ChooseRandomValue<T>(IList<T> list)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        return list[Next(list.Count)];
    }

    protected override double Sample()
    {
        lock (SyncLock)
        {
            return base.Sample();
        }
    }

    private static ulong GetNextInstanceId()
    {
        lock (InstanceCountLock)
        {
            _instanceCount++;
            return _instanceCount;
        }
    }
}