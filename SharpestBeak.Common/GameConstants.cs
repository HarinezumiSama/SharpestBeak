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

            public static readonly float BodyCircleRadius = LargeCellSize / 4f;

            /// <summary>
            ///     Length of beak overhang, counting from the body circle.
            /// </summary>
            public static readonly float BeakOverhang = LargeCellSize / 8f;

            public static readonly float BeakOffset = BodyCircleRadius + BeakOverhang;

            public static readonly float BeakRayOffset = BodyCircleRadius / 2f;

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        public static readonly float LargeCellSize = 100f;

        public static readonly TimeSpan LogicPollFrequency = TimeSpan.FromMilliseconds(20);

        public static readonly float NominalMoveSpeed = LargeCellSize;  // Per second

        /// <summary>
        ///     Nominal beak angle speed, degrees per second.
        /// </summary>
        public static readonly float NominalBeakAngleSpeed = 90f;  // Per second

        public static readonly int MinNominalCellCount = 5;

        public static readonly float FullRevolutionAngle = GameHelper.RevolutionDegrees;

        #endregion
    }
}