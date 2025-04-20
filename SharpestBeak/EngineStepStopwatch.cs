using System;
using System.Diagnostics;

namespace SharpestBeak;

public sealed class EngineStepStopwatch
{
    private const double MaxStepRatio = 1d;

    private readonly Stopwatch _stopwatch;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EngineStepStopwatch"/> class.
    /// </summary>
    internal EngineStepStopwatch() => _stopwatch = new Stopwatch();

    [DebuggerNonUserCode]
    public float StepRatio
    {
        get
        {
            var ratio = _stopwatch.Elapsed.TotalMilliseconds / GameConstants.LogicPollFrequency.TotalMilliseconds;
            return (float)Math.Min(ratio, MaxStepRatio);
        }
    }

    internal void Start() => _stopwatch.Restart();
}