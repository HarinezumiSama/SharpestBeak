using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Diagnostics
{
    internal sealed class PerformanceCounterHelper
    {
        #region Constants

        public static readonly string CategoryName = typeof(PerformanceCounterHelper).Assembly.GetName().Name;
        public static readonly string CollisionCountPerStepCounterName = "CollisionCountPerStep";
        public static readonly string CollisionCountPerStepBaseCounterName = "CollisionCountPerStepBase";

        #endregion

        #region Fields

        private static readonly PerformanceCounterHelper s_instance = new PerformanceCounterHelper();

        #endregion

        #region Constructors

        private PerformanceCounterHelper()
        {
            Setup();

            this.CollisionCountPerStep = new PerformanceCounter(
                CategoryName,
                CollisionCountPerStepCounterName,
                false)
            {
                RawValue = 0
            };

            this.CollisionCountPerStepBase = new PerformanceCounter(
                CategoryName,
                CollisionCountPerStepBaseCounterName,
                false)
            {
                RawValue = 0
            };
        }

        #endregion

        #region Private Methods

        private void Setup()
        {
            if (PerformanceCounterCategory.Exists(CategoryName))
            {
                PerformanceCounterCategory.Delete(CategoryName);
            }

            var counters = new CounterCreationDataCollection();

            counters.Add(
                new CounterCreationData(
                    CollisionCountPerStepCounterName,
                    "The number of collision detection calls per one game engine step.",
                    PerformanceCounterType.AverageCount64));

            counters.Add(
                new CounterCreationData(
                    CollisionCountPerStepBaseCounterName,
                    string.Format("Base counter for '{0}'.", CollisionCountPerStepCounterName),
                    PerformanceCounterType.AverageBase));

            var category = PerformanceCounterCategory.Create(
                CategoryName,
                "Sharpest Beak performance counters.",
                PerformanceCounterCategoryType.Unknown,
                counters);

        }

        #endregion

        #region Public Properties

        public static PerformanceCounterHelper Instance
        {
            [DebuggerStepThrough]
            get { return s_instance; }
        }

        public PerformanceCounter CollisionCountPerStep
        {
            get;
            private set;
        }

        public PerformanceCounter CollisionCountPerStepBase
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Initializes <see cref="PerformanceCounterHelper"/>. Used to initialize
        ///     <see cref="PerformanceCounterHelper"/> at a specific time.
        /// </summary>
        public static PerformanceCounterHelper Initialize()
        {
            return Instance;
        }

        #endregion
    }
}