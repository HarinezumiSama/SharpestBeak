using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public static class GameHelper
    {
        #region Constants

        public const float ZeroTolerance = 1E-6f;

        public const float Pi = 3.14159265358979f;
        public const float DoublePi = 6.283185307179586f;

        public const float HalfRevolutionRadians = Pi;
        public const float RevolutionRadians = DoublePi;

        public const float HalfRevolutionDegrees = 180f;
        public const float RevolutionDegrees = 360f;

        #endregion

        #region Fields

        private static readonly Dictionary<MoveDirection, PointF> s_directionMap =
            new Dictionary<MoveDirection, PointF>
            {
                { MoveDirection.None, PointF.Empty },
                { MoveDirection.MoveDown, new PointF(0, 1f) },
                { MoveDirection.MoveLeft, new PointF(-1f, 0f) },
                { MoveDirection.MoveRight, new PointF(1f, 0f) },
                { MoveDirection.MoveUp, new PointF(0f, -1f) }
            };

        #endregion

        #region Private Methods

        private static void PrepareRotate(float degreeAngle, out double cos, out double sin)
        {
            var degree = ToRadians(degreeAngle);
            cos = Math.Cos(degree);
            sin = Math.Sin(degree);
        }

        private static PointF RotatePointInternal(PointF value, PointF center, double cos, double sin)
        {
            var newX = center.X + (value.X - center.X) * cos - (value.Y - center.Y) * sin;
            var newY = center.Y + (value.X - center.X) * sin + (value.Y - center.Y) * cos;
            return new PointF((float)newX, (float)newY);
        }

        #endregion

        #region Public Methods

        public static bool IsZero(this float value, float tolerance = ZeroTolerance)
        {
            return Math.Abs(value) < tolerance;
        }

        public static float GetNewBeakAngle(float oldBeakAngle, BeakTurn beakTurn, float timeDelta)
        {
            int beakTurnOffset = (int)beakTurn;

            #region Argument Check

            if (oldBeakAngle <= -GameHelper.HalfRevolutionDegrees || oldBeakAngle > GameHelper.HalfRevolutionDegrees)
            {
                throw new ArgumentOutOfRangeException("oldBeakAngle", oldBeakAngle, "Beak angle cannot be negative.");
            }
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

            var result = (oldBeakAngle + timeDelta * GameConstants.NominalBeakAngleSpeed * beakTurnOffset);
            while (result > GameHelper.HalfRevolutionDegrees && result > -GameHelper.HalfRevolutionDegrees)
            {
                result -= GameHelper.RevolutionDegrees;
            }
            while (result <= -GameHelper.HalfRevolutionDegrees && result <= GameHelper.HalfRevolutionDegrees)
            {
                result += GameHelper.RevolutionDegrees;
            }

            if (result > GameHelper.HalfRevolutionDegrees || result <= -GameHelper.HalfRevolutionDegrees)
            {
                throw new InvalidOperationException("Computed angle was not fixed correctly.");
            }

            return result;
        }

        public static PointF GetNewPosition(PointF oldPosition, PointF directionVector, float timeDelta)
        {
            if (directionVector == PointF.Empty)
            {
                return oldPosition;
            }

            var moveAngle = Math.Atan2(directionVector.Y, directionVector.X);
            var moveStep = timeDelta * GameConstants.NominalMoveSpeed;
            var result = oldPosition
                + new SizeF((float)(moveStep * Math.Cos(moveAngle)), (float)(moveStep * Math.Sin(moveAngle)));
            return result;
        }

        public static PointF GetNewPosition(PointF oldPosition, MoveDirection direction, float timeDelta)
        {
            if (direction == MoveDirection.None)
            {
                return oldPosition;
            }

            var directionVector = s_directionMap[direction];
            return GetNewPosition(oldPosition, directionVector, timeDelta);
        }

        public static PointF Scale(this PointF value, float coefficient)
        {
            return new PointF(value.X * coefficient, value.Y * coefficient);
        }

        public static float ToRadians(float degreeAngle)
        {
            return degreeAngle / HalfRevolutionDegrees * Pi;
        }

        public static float ToDegrees(float radianAngle)
        {
            return radianAngle / Pi * HalfRevolutionDegrees;
        }

        public static PointF RotatePoint(this PointF value, PointF center, float degreeAngle)
        {
            double cos;
            double sin;
            PrepareRotate(degreeAngle, out cos, out sin);

            return RotatePointInternal(value, center, cos, sin);
        }

        public static PointF[] RotatePoints(this IEnumerable<PointF> values, PointF center, float degreeAngle)
        {
            double cos;
            double sin;
            PrepareRotate(degreeAngle, out cos, out sin);

            return values.Select(value => RotatePointInternal(value, center, cos, sin)).ToArray();
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

        public static BeakTurn GetBeakTurn(float currentAngle, float targetAngle)
        {
            var difference = targetAngle - currentAngle;
            return MapValueSign(difference, BeakTurn.None, BeakTurn.Clockwise, BeakTurn.CounterClockwise);
        }

        public static float Sqr(this float value)
        {
            return value * value;
        }

        public static float GetDistance(this PointF value, PointF otherValue)
        {
            return (float)Math.Sqrt(Sqr(otherValue.X - value.X) + Sqr(otherValue.Y - value.Y));
        }

        #endregion
    }
}