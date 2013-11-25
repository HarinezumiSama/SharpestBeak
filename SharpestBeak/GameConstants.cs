using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpestBeak
{
    public static class GameConstants
    {
        #region Constants and Fields

        public static readonly float ShotToChickenRectilinearSpeedRatio = 4f;

        public static readonly float NominalCellSize = 100f;

        public static readonly ValueRange<int> NominalCellCountRange = new ValueRange<int>(5, 128);

        public static readonly TimeSpan LogicPollFrequency = TimeSpan.FromMilliseconds(LogicPollFrequencyMsec);

        public static readonly float StepTimeDelta = (float)LogicPollFrequency.TotalSeconds;

        public static readonly float FullRevolutionAngle = MathHelper.RevolutionDegrees;

        public static readonly ValueRange<int> TeamPlayerUnitCountRange = new ValueRange<int>(1, int.MaxValue);

        public static readonly ValueRange<float> NormalizedScalarSpeedRange = new ValueRange<float>(0f, 1f);

        internal static readonly float SlowDownRatio = GetSlowDownRatio();

        private const float MinSlowDownRatio = 1f / 16f;
        private const float MaxSlowDownRatio = 16f;

        private const double LogicPollFrequencyMsec = 20d;

        #endregion

        #region Private Methods

        [DebuggerNonUserCode]
        private static float GetSlowDownRatio()
        {
            var result = Math.Min(
                MaxSlowDownRatio,
                Math.Max(MinSlowDownRatio, Properties.Settings.Default.SlowDownRatio));
            return result;
        }

        #endregion

        #region Nested Types

        #region ChickenUnit Class

        public static class ChickenUnit
        {
            #region Constants and Fields

            /// <summary>
            ///     Distance from body circle center to beak tip.
            /// </summary>
            public static readonly float BeakOffset = (NominalCellSize / 2f) * 7f / 8f;

            /// <summary>
            ///     Length of beak overhang, counting from the body circle.
            /// </summary>
            public static readonly float BeakOverhang = BeakOffset / 2f;

            public static readonly float BodyCircleRadius = BeakOffset - BeakOverhang;

            public static readonly float BeakRayOffset = BodyCircleRadius * 0.75f;

            /// <summary>
            ///     Standard rectilinear speed of a chicken unit, in units per second.
            /// </summary>
            public static readonly float DefaultRectilinearSpeed =
                NominalCellSize * InternalSpeedCoefficient / SlowDownRatio;

            /// <summary>
            ///     Standard beak angle speed of a chicken unit, in degrees per second.
            /// </summary>
            public static readonly float DefaultAngularSpeed =
                MathHelper.RevolutionDegrees * InternalSpeedCoefficient / 4f / SlowDownRatio;

            /// <summary>
            ///     The value, in degrees, specifying the angle of view of a chicken unit to left and to right
            ///     (the whole view angle is double this value).
            /// </summary>
            public static readonly float ViewAngle = MathHelper.RevolutionDegrees / 6f;

            public static readonly float DefaultRectilinearStepDistance = DefaultRectilinearSpeed * StepTimeDelta;

            public static readonly float DefaultAngularStep = DefaultAngularSpeed * StepTimeDelta;

            private const float InternalSpeedCoefficient = 1f;

            #endregion
        }

        #endregion

        #region ShotUnit Class

        public static class ShotUnit
        {
            #region Constants and Fields

            public static readonly double MaximumFrequencyMsec = 1000d * SlowDownRatio;

            public static readonly float Radius = NominalCellSize / 10f;

            /// <summary>
            ///     Standard rectilinear speed of a shot unit, in units per second.
            /// </summary>
            public static readonly float DefaultRectilinearSpeed =
                ChickenUnit.DefaultRectilinearSpeed * ShotToChickenRectilinearSpeedRatio;

            public static readonly int EngineStepsBetweenShots = Math.Max(
                1,
                (int)(MaximumFrequencyMsec / LogicPollFrequencyMsec));

            public static readonly float DefaultRectilinearStepDistance = DefaultRectilinearSpeed * StepTimeDelta;

            #endregion
        }

        #endregion

        #endregion
    }
}