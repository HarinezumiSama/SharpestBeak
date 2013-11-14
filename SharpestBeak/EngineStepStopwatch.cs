using System;
using System.Diagnostics;

namespace SharpestBeak
{
    public sealed class EngineStepStopwatch
    {
        #region Constants and Fields

        private const double MaxStepRatio = 1d;

        private readonly Stopwatch _stopwatch;

        #endregion

        #region Constructors

        private EngineStepStopwatch()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        #endregion

        #region Public Properties

        public double StepRatio
        {
            [DebuggerNonUserCode]
            get
            {
                var ratio = _stopwatch.Elapsed.TotalMilliseconds / GameConstants.LogicPollFrequency.TotalMilliseconds;
                return Math.Max(ratio, MaxStepRatio);
            }
        }

        #endregion

        #region Public Methods

        public static EngineStepStopwatch CreateAndStart()
        {
            return new EngineStepStopwatch();
        }

        #endregion
    }
}