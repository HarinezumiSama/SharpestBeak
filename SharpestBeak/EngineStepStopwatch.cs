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

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineStepStopwatch"/> class.
        /// </summary>
        internal EngineStepStopwatch()
        {
            _stopwatch = new Stopwatch();
        }

        #endregion

        #region Public Properties

        public float StepRatio
        {
            [DebuggerNonUserCode]
            get
            {
                var ratio = _stopwatch.Elapsed.TotalMilliseconds / GameConstants.LogicPollFrequency.TotalMilliseconds;
                return (float)Math.Min(ratio, MaxStepRatio);
            }
        }

        #endregion

        #region Internal Methods

        internal void Start()
        {
            _stopwatch.Restart();
        }

        #endregion
    }
}