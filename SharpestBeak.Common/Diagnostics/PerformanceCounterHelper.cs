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
                    CollisionCountPerStepCounterName,
                    PerformanceCounterType.AverageCount64));

            counters.Add(
                new CounterCreationData(
                    CollisionCountPerStepBaseCounterName,
                    CollisionCountPerStepBaseCounterName,
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
    }
}