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

        public const float RevolutionRadians = DoublePi;
        public const float HalfRevolutionRadians = Pi;
        public const float QuarterRevolutionRadians = Pi / 2f;

        public const float RevolutionDegrees = 360f;
        public const float HalfRevolutionDegrees = RevolutionDegrees / 2f;
        public const float QuarterRevolutionDegrees = RevolutionDegrees / 4f;

        #endregion

        #region Fields

        private static readonly Dictionary<MoveDirection, GameAngle?> s_directionMap =
            new Dictionary<MoveDirection, GameAngle?>
            {
                { MoveDirection.None, null },
                { MoveDirection.MoveForward,  GameAngle.FromDegrees(0f) },
                { MoveDirection.MoveBackward, GameAngle.FromDegrees(HalfRevolutionDegrees) },
                { MoveDirection.MoveLeft, GameAngle.FromDegrees(QuarterRevolutionDegrees) },
                { MoveDirection.MoveRight, GameAngle.FromDegrees(-QuarterRevolutionDegrees) }
            };

        #endregion

        #region Public Methods

        public static bool IsZero(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return Math.Abs(value) < tolerance;
        }

        public static bool IsPositive(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value >= tolerance;
        }

        public static bool IsNegative(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value <= -tolerance;
        }

        public static bool IsInRange(this float value, float min, float max, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (min > max)
            {
                throw new ArgumentOutOfRangeException(
                    "min",
                    min,
                    "Range minimum is greater than its maximum.");
            }

            #endregion

            return (value > min && value < max) || (value - min).IsZero(tolerance) || (value - max).IsZero(tolerance);
        }

        public static bool IsValidDegreeAngle(this float angle)
        {
            return angle > -GameHelper.HalfRevolutionDegrees && angle <= GameHelper.HalfRevolutionDegrees;
        }

        public static float EnsureValidDegreeAngle(this float angle)
        {
            if (!IsValidDegreeAngle(angle))
            {
                throw new ArgumentOutOfRangeException("angle", angle, "Invalid angle.");
            }

            return angle;
        }

        public static float Cos(this float value)
        {
            return (float)Math.Cos(value);
        }

        public static float Sin(this float value)
        {
            return (float)Math.Sin(value);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Sqr(this float value)
        {
            return value * value;
        }

        public static float Sqrt(this float value)
        {
            return (float)Math.Sqrt(value);
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
            var result = oldPosition
                + new Vector2D(distance * angle.RadianValue.Cos(), distance * angle.RadianValue.Sin());
            return result;
        }

        public static Point2D GetNewPosition(
            Point2D oldPosition,
            GameAngle currentAngle,
            MoveDirection direction,
            float speed,
            float timeDelta)
        {
            var relativeAngle = s_directionMap[direction];
            if (relativeAngle == null)
            {
                return oldPosition;
            }

            var actualAngle = currentAngle + relativeAngle.Value;
            var distance = timeDelta * speed;
            var result = GetNewPosition(oldPosition, actualAngle, distance);
            return result;
        }

        public static PointF Scale(this PointF value, float coefficient)
        {
            return new PointF(value.X * coefficient, value.Y * coefficient);
        }

        public static float ToRadians(this float degreeAngle)
        {
            return degreeAngle / HalfRevolutionDegrees * Pi;
        }

        public static float ToDegrees(this float radianAngle)
        {
            return radianAngle / Pi * HalfRevolutionDegrees;
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

        public static float GetDistanceSquared(this PointF value, PointF otherValue)
        {
            return ((otherValue.X - value.X).Sqr() + (otherValue.Y - value.Y).Sqr());
        }

        public static float GetDistance(this PointF value, PointF otherValue)
        {
            return GetDistanceSquared(value, otherValue).Sqrt();
        }

        #endregion
    }
}