using System;
using System.Collections.Generic;

namespace SharpestBeak.Common
{
    public static class GameConstants
    {
        #region Nested Types

        #region ChickenUnit Class

        public static class ChickenUnit
        {
            #region Fields

            public static readonly float BodyCircleRadius = GameConstants.NominalCellSize / 4f;

            /// <summary>
            ///     Length of beak overhang, counting from the body circle.
            /// </summary>
            public static readonly float BeakOverhang = BodyCircleRadius / 2f;

            /// <summary>
            ///     Distance from body circle center to beak tip.
            /// </summary>
            public static readonly float BeakOffset = BodyCircleRadius + BeakOverhang;

            public static readonly float BeakRayOffset = BodyCircleRadius / 2f;

            /// <summary>
            ///     Standard rectilinear speed of a chicken unit, in units per second.
            /// </summary>
            public static readonly float DefaultRectilinearSpeed = GameConstants.NominalCellSize;

            /// <summary>
            ///     Standard beak angle speed of a chicken unit, in degrees per second.
            /// </summary>
            public static readonly float DefaultAngularSpeed = GameHelper.RevolutionDegrees / 4f;

            #endregion
        }

        #endregion

        #region ShotUnit Class

        public static class ShotUnit
        {
            #region Fields

            public static readonly float Radius = GameConstants.NominalCellSize / 20f;

            /// <summary>
            ///     Standard rectilinear speed of a shot unit, in units per second.
            /// </summary>
            public static readonly float DefaultSpeed = 2f * GameConstants.ChickenUnit.DefaultRectilinearSpeed;

            public static readonly TimeSpan MaximumFrequency = TimeSpan.FromMilliseconds(500d);

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        public static readonly float NominalCellSize = 100f;

        public static readonly int MinNominalCellCount = 5;

        public static readonly TimeSpan LogicPollFrequency = TimeSpan.FromMilliseconds(20d);

        public static readonly float FullRevolutionAngle = GameHelper.RevolutionDegrees;

        #endregion
    }
}