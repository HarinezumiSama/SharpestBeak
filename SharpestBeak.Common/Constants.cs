using System;
using System.Collections.Generic;

namespace SharpestBeak.Common
{
    public static class Constants
    {
        #region Nested Types

        #region ChickenUnit Class

        public static class ChickenUnit
        {
            #region Fields

            public static readonly float BodyCircleRadius = NominalCellSize / 4f;

            /// <summary>
            ///     Length of beak overhang, counting from the body circle.
            /// </summary>
            public static readonly float BeakOverhang = NominalCellSize / 8f;

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        public static readonly float NominalCellSize = 100f;

        public static readonly TimeSpan LogicPollFrequency = TimeSpan.FromMilliseconds(20);

        public static readonly float NominalMoveSpeed = NominalCellSize;  // Per second

        /// <summary>
        ///     Nominal beak angle speed, degrees per second.
        /// </summary>
        public static readonly float NominalBeakAngleSpeed = 90f;  // Per second

        public static readonly int MinNominalCellCount = 5;

        public static readonly float FullRotationAngle = 360f;  // In degrees

        #endregion
    }
}