using System;
using System.Diagnostics;

namespace SharpestBeak;

public static class GameConstants
{
    public static readonly float ShotToChickenRectilinearSpeedRatio = 4f;
    public static readonly float NominalCellSize = 100f;

    public static readonly ValueRange<int> NominalCellCountRange = new(5, 128);
    public static readonly ValueRange<int> TeamPlayerUnitCountRange = new(1, int.MaxValue);

    internal static readonly TimeSpan LogicPollFrequency = TimeSpan.FromMilliseconds(20d * GetSlowDownRatio());
    internal static readonly ValueRange<float> NormalizedScalarSpeedRange = new(0f, 1f);

    private const float MinSlowDownRatio = 1f / 16f;
    private const float MaxSlowDownRatio = 16f;

    [DebuggerNonUserCode]
    private static float GetSlowDownRatio()
    {
        var result = Math.Min(
            MaxSlowDownRatio,
            Math.Max(MinSlowDownRatio, Properties.Settings.Default.SlowDownRatio));

        return result;
    }

    public static class ChickenUnit
    {
        /// <summary>
        ///     Distance from body circle center to beak tip.
        /// </summary>
        public static readonly float BeakOffset = NominalCellSize / 2f * 7f / 8f;

        /// <summary>
        ///     Length of beak overhang, counting from the body circle.
        /// </summary>
        public static readonly float BeakOverhang = BeakOffset / 2f;

        public static readonly float BodyCircleRadius = BeakOffset - BeakOverhang;

        public static readonly float BeakRayOffset = BodyCircleRadius * 0.75f;

        /// <summary>
        ///     Standard rectilinear speed of a chicken unit, in units per second.
        /// </summary>
        public static readonly float DefaultRectilinearSpeed = NominalCellSize / 50f;

        /// <summary>
        ///     Standard beak angle speed of a chicken unit, in degrees per second.
        /// </summary>
        public static readonly float DefaultAngularSpeed = MathHelper.RevolutionDegrees / 50f;

        /// <summary>
        ///     The value, in degrees, specifying the angle of view of a chicken unit to left and to right
        ///     (the whole view angle is double this value).
        /// </summary>
        public static readonly float ViewAngle = MathHelper.RevolutionDegrees / 6f;
    }

    public static class ShotUnit
    {
        public static readonly float Radius = NominalCellSize / 10f;

        /// <summary>
        ///     Standard rectilinear speed of a shot unit, in units per second.
        /// </summary>
        public static readonly float DefaultRectilinearSpeed = ChickenUnit.DefaultRectilinearSpeed * ShotToChickenRectilinearSpeedRatio;

        public static readonly int EngineStepsBetweenShots = 50;
    }
}