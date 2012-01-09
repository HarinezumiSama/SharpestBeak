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

        private static void PrepareRotate(float radianAngle, out float cos, out float sin)
        {
            cos = radianAngle.Cos();
            sin = radianAngle.Sin();
        }

        private static PointF RotatePointInternal(PointF value, PointF center, float cos, float sin)
        {
            var newX = center.X + (value.X - center.X) * cos - (value.Y - center.Y) * sin;
            var newY = center.Y + (value.X - center.X) * sin + (value.Y - center.Y) * cos;
            return new PointF(newX, newY);
        }

        #endregion

        #region Public Methods

        public static bool IsZero(this float value, float tolerance = ZeroTolerance)
        {
            return Math.Abs(value) < tolerance;
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

            var proxyResult = oldBeakAngle.DegreeValue
                + timeDelta * GameConstants.StandardBeakAngleSpeed * beakTurnOffset;
            while (proxyResult > GameHelper.HalfRevolutionDegrees && proxyResult > -GameHelper.HalfRevolutionDegrees)
            {
                proxyResult -= GameHelper.RevolutionDegrees;
            }
            while (proxyResult <= -GameHelper.HalfRevolutionDegrees && proxyResult <= GameHelper.HalfRevolutionDegrees)
            {
                proxyResult += GameHelper.RevolutionDegrees;
            }

            if (proxyResult > GameHelper.HalfRevolutionDegrees || proxyResult <= -GameHelper.HalfRevolutionDegrees)
            {
                throw new InvalidOperationException("Computed angle was not fixed correctly.");
            }

            return GameAngle.FromDegrees(proxyResult);
        }

        public static PointF GetNewPosition(PointF oldPosition, GameAngle angle, float speed, float timeDelta)
        {
            var distance = timeDelta * speed;
            var result = oldPosition
                + new SizeF(distance * angle.RadianValue.Cos(), distance * angle.RadianValue.Sin());
            return result;
        }

        public static PointF GetNewPosition(PointF oldPosition, MoveDirection direction, float speed, float timeDelta)
        {
            if (direction == MoveDirection.None)
            {
                return oldPosition;
            }

            var directionVector = s_directionMap[direction];
            var angle = Atan2(directionVector.Y, directionVector.X);
            return GetNewPosition(oldPosition, GameAngle.FromRadians(angle), speed, timeDelta);
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

        public static PointF Rotate(this PointF value, PointF center, GameAngle angle)
        {
            float cos, sin;
            PrepareRotate(angle.RadianValue, out cos, out sin);

            return RotatePointInternal(value, center, cos, sin);
        }

        public static PointF[] Rotate(this IEnumerable<PointF> values, PointF center, GameAngle angle)
        {
            float cos, sin;
            PrepareRotate(angle.RadianValue, out cos, out sin);

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

        public static BeakTurn GetBeakTurn(GameAngle currentAngle, float targetAngle)
        {
            var difference = targetAngle - currentAngle.DegreeValue;
            return MapValueSign(difference, BeakTurn.None, BeakTurn.Clockwise, BeakTurn.CounterClockwise);
        }

        public static float GetDistance(this PointF value, PointF otherValue)
        {
            return ((otherValue.X - value.X).Sqr() + (otherValue.Y - value.Y).Sqr()).Sqrt();
        }

        #endregion
    }
}