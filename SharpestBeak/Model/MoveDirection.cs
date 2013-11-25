using System;
using System.Diagnostics;
using System.Globalization;
using SharpestBeak.Physics;

namespace SharpestBeak.Model
{
    /// <summary>
    ///     Specifies the move direction.
    /// </summary>
    public sealed class MoveDirection
    {
        #region Constants and Fields

        public static readonly MoveDirection None = new MoveDirection(Vector2D.Zero, 0f);

        public static readonly MoveDirection MoveForward = new MoveDirection(
            Vector2D.UnitY,
            GameConstants.NormalizedScalarSpeedRange.Max);

        public static readonly MoveDirection MoveBackward = new MoveDirection(
            -Vector2D.UnitY,
            GameConstants.NormalizedScalarSpeedRange.Max);

        public static readonly MoveDirection StrafeLeft = new MoveDirection(
            -Vector2D.UnitX,
            GameConstants.NormalizedScalarSpeedRange.Max);

        public static readonly MoveDirection StrafeRight = new MoveDirection(
            Vector2D.UnitX,
            GameConstants.NormalizedScalarSpeedRange.Max);

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MoveDirection"/> class.
        /// </summary>
        public MoveDirection(Vector2D direction, float normalizedScalarSpeed)
        {
            #region Argument Check

            if (!normalizedScalarSpeed.IsInRange(GameConstants.NormalizedScalarSpeedRange))
            {
                throw new ArgumentOutOfRangeException(
                    "normalizedScalarSpeed",
                    normalizedScalarSpeed,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        @"The value is out of the valid range {0}.",
                        GameConstants.NormalizedScalarSpeedRange));
            }

            #endregion

            this.NormalizedDirection = GetNormalizedDirection(direction, normalizedScalarSpeed);
        }

        #endregion

        #region Public Properties

        public Vector2D NormalizedDirection
        {
            get;
            private set;
        }

        public bool IsNone
        {
            [DebuggerNonUserCode]
            get
            {
                return this.NormalizedDirection == Vector2D.Zero;
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}: NormalizedDirection = {1}",
                GetType().Name,
                this.NormalizedDirection);
        }

        #endregion

        #region Private Methods

        private static Vector2D GetNormalizedDirection(Vector2D direction, float normalizedScalarSpeed)
        {
            if (normalizedScalarSpeed.IsZero())
            {
                return Vector2D.Zero;
            }

            var length = direction.GetLength();
            if (length.IsZero())
            {
                return Vector2D.Zero;
            }

            return direction / length * normalizedScalarSpeed;
        }

        #endregion
    }
}