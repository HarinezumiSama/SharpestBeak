using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public static class GameHelper
    {
        #region Fields

        private static readonly Dictionary<MoveDirection, GameAngle?> s_directionMap =
            new Dictionary<MoveDirection, GameAngle?>
            {
                { MoveDirection.None, null },
                { MoveDirection.MoveForward,  GameAngle.FromDegrees(0f) },
                { MoveDirection.MoveBackward, GameAngle.FromDegrees(MathHelper.HalfRevolutionDegrees) },
                { MoveDirection.StrafeLeft, GameAngle.FromDegrees(MathHelper.QuarterRevolutionDegrees) },
                { MoveDirection.StrafeRight, GameAngle.FromDegrees(-MathHelper.QuarterRevolutionDegrees) }
            };

        private static readonly Vector2D s_halfNominalCellOffset =
            new Vector2D(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the real board coordinates of the center of the nominal cell with the specified coordinates.
        /// </summary>
        /// <param name="nominalPoint">
        ///     The coordinates of the nominal cell to convert.
        /// </param>
        /// <returns>
        ///     The real board coordinates.
        /// </returns>
        public static Point2D NominalToReal(Point nominalPoint)
        {
            return new Point2D(nominalPoint) * GameConstants.NominalCellSize + s_halfNominalCellOffset;
        }

        public static GameAngle GetNewBeakAngle(GameAngle oldBeakAngle, BeakTurn beakTurn, float timeDelta)
        {
            int beakTurnOffset = (int)beakTurn;

            #region Argument Check

            if (Math.Abs(beakTurnOffset) > 1)
            {
                throw new ArgumentOutOfRangeException("beakTurn", beakTurn, "Invalid beak turn.");
            }
            if (timeDelta < 0)
            {
                throw new ArgumentOutOfRangeException("timeDelta", timeDelta, "Time delta cannot be negative.");
            }

            #endregion

            if (beakTurnOffset == 0)
            {
                return oldBeakAngle;
            }

            var proxyResult = GameAngle.NormalizeDegreeAngle(oldBeakAngle.DegreeValue
                + timeDelta * GameConstants.ChickenUnit.DefaultAngularSpeed * beakTurnOffset);
            return GameAngle.FromDegrees(proxyResult);
        }

        public static Point2D GetNewPosition(Point2D oldPosition, GameAngle angle, float distance)
        {
            var rad = angle.RadianValue;
            var result = oldPosition + new Vector2D(distance * rad.Cos(), distance * rad.Sin());
            return result;
        }

        public static Point2D GetNewPosition(
            Point2D oldPosition,
            GameAngle currentAngle,
            MoveDirection direction,
            float distance)
        {
            var relativeAngle = s_directionMap[direction];
            if (relativeAngle == null)
            {
                return oldPosition;
            }

            var actualAngle = currentAngle + relativeAngle.Value;

            var result = GetNewPosition(oldPosition, actualAngle, distance);
            return result;
        }

        public static Point2D GetBeakTipPosition(Point2D position, GameAngle beakAngle)
        {
            return position
                .OffsetX(GameConstants.ChickenUnit.BeakOffset)
                .Rotate(position, beakAngle);
        }

        public static Vector2D GetChickenViewDirection(Point2D position, GameAngle beakAngle)
        {
            var beakTipPosition = GetBeakTipPosition(position, beakAngle);
            return beakTipPosition - position;
        }

        public static PointF Scale(this PointF value, float coefficient)
        {
            return new PointF(value.X * coefficient, value.Y * coefficient);
        }

        public static Point2D[] Rotate(this IEnumerable<Point2D> values, Point2D center, GameAngle angle)
        {
            return Point2D.Rotate(values, center, angle);
        }

        public static PointF[] ToPointF(this IEnumerable<Point2D> values)
        {
            #region Argument Check

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            #endregion

            return values.Select(item => item.ToPointF()).ToArray();
        }

        public static SizeF ToSizeF(this PointF value)
        {
            return new SizeF(value);
        }

        public static T MapValueSign<T>(this float value, T zero, T negative, T positive)
        {
            if (value.IsZero())
            {
                return zero;
            }

            return value > 0 ? positive : negative;
        }

        public static BeakTurn GetBeakTurn(GameAngle currentAngle, GameAngle targetAngle)
        {
            var difference = (targetAngle - currentAngle).DegreeValue;
            return MapValueSign(difference, BeakTurn.None, BeakTurn.Clockwise, BeakTurn.CounterClockwise);
        }

        #endregion
    }
}