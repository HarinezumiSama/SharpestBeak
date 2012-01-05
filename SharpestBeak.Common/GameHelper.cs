using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public static class GameHelper
    {
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

        public static float GetNewBeakAngle(float oldBeakAngle, BeakTurn beakTurn, float timeDelta)
        {
            int beakTurnOffset = (int)beakTurn;

            #region Argument Check

            if (oldBeakAngle < 0)
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

            var result = (oldBeakAngle + timeDelta * Constants.NominalBeakAngleSpeed * beakTurnOffset)
                % Constants.FullRotationAngle;
            if (result < 0f)
            {
                result += Constants.FullRotationAngle;
            }
            else if (result >= Constants.FullRotationAngle)
            {
                result -= Constants.FullRotationAngle;
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
            var moveStep = timeDelta * Constants.NominalMoveSpeed;
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
            var result = (float)(degreeAngle / 180d * Math.PI);
            return result;
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

        #endregion
    }
}