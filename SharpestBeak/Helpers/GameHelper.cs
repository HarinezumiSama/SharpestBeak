using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.Model;
using SharpestBeak.Physics;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
namespace SharpestBeak
{
    public static class GameHelper
    {
        #region Fields

        private static readonly Dictionary<MoveDirection, GameAngle?> s_directionToAngleMap =
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

        private static readonly IList<MoveDirection> s_moveDirections =
            Helper.GetEnumValues<MoveDirection>().AsReadOnly();
        private static readonly IList<MoveDirection> s_activeMoveDirections = GetActiveMoveDirections();

        private static readonly IList<FireMode> s_fireModes = Helper.GetEnumValues<FireMode>().AsReadOnly();

        #endregion

        #region Private Methods

        private static IList<MoveDirection> GetActiveMoveDirections()
        {
            var resultProxy = new HashSet<MoveDirection>(Helper.GetEnumValues<MoveDirection>());
            resultProxy.Remove(MoveDirection.None);
            return resultProxy.ToList().AsReadOnly();
        }

        private static float GetDistanceToLineInternal(Vector2D pointDirection, Vector2D lineDirection)
        {
            var projection = pointDirection.ProjectScalar(lineDirection);
            var result = (pointDirection.GetLengthSquared() - projection.Sqr()).Sqrt();
            return result;
        }

        #endregion

        #region Public Properties

        public static IList<MoveDirection> MoveDirections
        {
            [DebuggerStepThrough]
            get
            {
                return s_moveDirections;
            }
        }

        public static IList<MoveDirection> ActiveMoveDirections
        {
            [DebuggerStepThrough]
            get
            {
                return s_activeMoveDirections;
            }
        }

        public static IList<FireMode> FireModes
        {
            [DebuggerStepThrough]
            get
            {
                return s_fireModes;
            }
        }

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

        public static GameAngle GetNewBeakAngle(GameAngle oldBeakAngle, BeakTurn beakTurn)
        {
            if (beakTurn.Value.IsZero())
            {
                return oldBeakAngle;
            }

            var proxyResult = GameAngle.NormalizeDegreeAngle(oldBeakAngle.DegreeValue
                + GameConstants.StepTimeDelta * GameConstants.ChickenUnit.DefaultAngularSpeed * beakTurn.Value);
            return GameAngle.FromDegrees(proxyResult);
        }

        public static Point2D GetNewPosition(Point2D oldPosition, GameAngle angle, float distance)
        {
            var rad = angle.RadianValue;
            var result = oldPosition + new Vector2D(distance * rad.Cos(), distance * rad.Sin());
            return result;
        }

        public static Point2D GetNewPosition(IDirectionalPosition directionalPosition, float distance)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetNewPosition(directionalPosition.Position, directionalPosition.Angle, distance);
        }

        public static Point2D GetNewPosition(
            Point2D oldPosition,
            GameAngle currentAngle,
            MoveDirection direction,
            float distance)
        {
            var relativeAngle = s_directionToAngleMap[direction];
            if (relativeAngle == null)
            {
                return oldPosition;
            }

            var actualAngle = currentAngle + relativeAngle.Value;

            var result = GetNewPosition(oldPosition, actualAngle, distance);
            return result;
        }

        public static Point2D GetNewPosition(
            IDirectionalPosition directionalPosition,
            MoveDirection direction,
            float distance)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetNewPosition(directionalPosition.Position, directionalPosition.Angle, direction, distance);
        }

        public static Point2D GetBeakTipPosition(Point2D position, GameAngle beakAngle)
        {
            return position
                .OffsetX(GameConstants.ChickenUnit.BeakOffset)
                .Rotate(position, beakAngle);
        }

        public static Point2D GetBeakTipPosition(IDirectionalPosition directionalPosition)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetBeakTipPosition(directionalPosition.Position, directionalPosition.Angle);
        }

        public static Vector2D GetChickenViewDirection(Point2D position, GameAngle beakAngle)
        {
            var beakTipPosition = GetBeakTipPosition(position, beakAngle);
            return beakTipPosition - position;
        }

        public static Vector2D GetChickenViewDirection(IDirectionalPosition directionalPosition)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetChickenViewDirection(directionalPosition.Position, directionalPosition.Angle);
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

        public static float GetDistanceToLine(Point2D point, Point2D linePoint, GameAngle lineAngle)
        {
            return GetDistanceToLineInternal(lineAngle.ToUnitVector(), point - linePoint);
        }

        public static float GetDistanceToLine(Point2D point, Point2D linePoint1, Point2D linePoint2)
        {
            return GetDistanceToLineInternal(point - linePoint1, linePoint2 - linePoint1);
        }

        public static T MapValueSign<T>(this float value, T zero, T negative, T positive)
        {
            if (value.IsZero())
            {
                return zero;
            }

            return value > 0 ? positive : negative;
        }

        public static float GetBeakTurn(GameAngle currentAngle, GameAngle targetAngle)
        {
            var difference = (targetAngle - currentAngle).DegreeValue;
            return difference / GameConstants.ChickenUnit.DefaultAngularStep;
        }

        public static BeakTurn NormalizeBeakTurn(this float absoluteBeakTurn)
        {
            var normalizedTurn = absoluteBeakTurn.ReduceToRange(BeakTurn.ValueRange);
            return new BeakTurn(normalizedTurn);
        }

        public static BeakTurn GetBeakTurnNormalized(GameAngle currentAngle, GameAngle targetAngle)
        {
            var absoluteBeakTurn = GetBeakTurn(currentAngle, targetAngle);
            return NormalizeBeakTurn(absoluteBeakTurn);
        }

        public static MoveDirection GetBestMoveDirection(Point2D position, GameAngle beakAngle, Point2D targetPoint)
        {
            var resultProxy = Tuple.Create(MoveDirection.None, float.MaxValue);
            foreach (var item in s_moveDirections)
            {
                var potentialMovePoint = GameHelper.GetNewPosition(
                    position,
                    beakAngle,
                    item,
                    GameConstants.ChickenUnit.DefaultRectilinearStepDistance);
                var distanceSquared = targetPoint.GetDistanceSquared(potentialMovePoint);
                if (distanceSquared < resultProxy.Item2)
                {
                    resultProxy = Tuple.Create(item, distanceSquared);
                }
            }

            return resultProxy.Item1;
        }

        public static MoveDirection GetBestMoveDirection(
            IDirectionalPosition directionalPosition,
            Point2D targetPoint)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetBestMoveDirection(directionalPosition.Position, directionalPosition.Angle, targetPoint);
        }

        public static float GetBestBeakTurn(Point2D position, GameAngle beakAngle, Point2D targetPoint)
        {
            var targetOffset = targetPoint - position;
            var targetAngle = GameAngle.FromRadians((float)Math.Atan2(targetOffset.Y, targetOffset.X));
            var result = GameHelper.GetBeakTurn(beakAngle, targetAngle);
            return result;
        }

        public static float GetBestBeakTurn(IDirectionalPosition directionalPosition, Point2D targetPoint)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetBestBeakTurn(directionalPosition.Position, directionalPosition.Angle, targetPoint);
        }

        public static BeakTurn GetBestBeakTurnNormalized(Point2D position, GameAngle beakAngle, Point2D targetPoint)
        {
            var absoluteBeakTurn = GetBestBeakTurn(position, beakAngle, targetPoint);
            var result = NormalizeBeakTurn(absoluteBeakTurn);
            return result;
        }

        public static BeakTurn GetBestBeakTurnNormalized(
            IDirectionalPosition directionalPosition,
            Point2D targetPoint)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            return GetBestBeakTurnNormalized(directionalPosition.Position, directionalPosition.Angle, targetPoint);
        }

        #endregion
    }
}