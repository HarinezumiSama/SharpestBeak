using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Physics;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
// ReSharper disable once CheckNamespace
namespace SharpestBeak
{
    public static class GameHelper
    {
        #region Constants and Fields

        private static readonly Vector2D HalfNominalCellOffset =
            new Vector2D(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);

        private static readonly ReadOnlyCollection<MoveDirection> BasicActiveMoveDirectionsField =
            new[]
            {
                MoveDirection.MoveForward,
                MoveDirection.MoveBackward,
                MoveDirection.StrafeLeft,
                MoveDirection.StrafeRight
            }
                .AsReadOnly();

        private static readonly ReadOnlyCollection<MoveDirection> BasicMoveDirectionsField =
            BasicActiveMoveDirectionsField.Concat(MoveDirection.None.AsCollection()).ToArray().AsReadOnly();

        private static readonly ReadOnlyCollection<FireMode> FireModesField =
            Helper.GetEnumValues<FireMode>().AsReadOnly();

        #endregion

        #region Public Properties

        public static ReadOnlyCollection<MoveDirection> BasicMoveDirections
        {
            [DebuggerStepThrough]
            get
            {
                return BasicMoveDirectionsField;
            }
        }

        public static ReadOnlyCollection<MoveDirection> BasicActiveMoveDirections
        {
            [DebuggerStepThrough]
            get
            {
                return BasicActiveMoveDirectionsField;
            }
        }

        public static ReadOnlyCollection<FireMode> FireModes
        {
            [DebuggerStepThrough]
            get
            {
                return FireModesField;
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
            return new Point2D(nominalPoint) * GameConstants.NominalCellSize + HalfNominalCellOffset;
        }

        public static GameAngle GetBeakMovement(BeakTurn beakTurn)
        {
            if (beakTurn.Value.IsZero())
            {
                return GameAngle.Zero;
            }

            var degreeAngle = GameConstants.ChickenUnit.DefaultAngularStep * beakTurn.Value;
            return GameAngle.FromDegrees(degreeAngle);
        }

        public static GameAngle GetNewBeakAngle(GameAngle oldBeakAngle, BeakTurn beakTurn)
        {
            var beakMovement = GetBeakMovement(beakTurn);

            var result = oldBeakAngle + beakMovement;
            return result;
        }

        public static Tuple<GameAngle, GameAngle> GetBeakMovementAndNewAngle(
            GameAngle oldBeakAngle,
            BeakTurn beakTurn)
        {
            var beakMovement = GetBeakMovement(beakTurn);
            var newAngle = oldBeakAngle + beakMovement;
            return Tuple.Create(beakMovement, newAngle);
        }

        public static Vector2D GetMovement(GameAngle angle, float speed)
        {
            var rad = angle.RadianValue;

            var result = new Vector2D(speed * rad.Cos(), speed * rad.Sin());
            return result;
        }

        public static Vector2D GetMovement(
            GameAngle currentAngle,
            MoveDirection direction,
            float speed)
        {
            if (direction.IsNone)
            {
                return Vector2D.Zero;
            }

            var relativeAngle = Vector2D.UnitY.GetAngle(direction.NormalizedDirection);

            var actualAngle = currentAngle + relativeAngle;
            var actualSpeed = Math.Min(direction.NormalizedDirection.GetLength(), 1f) * speed;

            var result = GetMovement(actualAngle, actualSpeed);
            return result;
        }

        public static Point2D GetNewPosition(
            Point2D oldPosition,
            GameAngle currentAngle,
            MoveDirection direction,
            float speed)
        {
            var movement = GetMovement(currentAngle, direction, speed);

            var result = GetNewPositionInternal(oldPosition, movement);
            return result;
        }

        public static Tuple<Vector2D, Point2D> GetMovementAndNewPosition(
            Point2D oldPosition,
            GameAngle currentAngle,
            MoveDirection direction,
            float speed)
        {
            var movement = GetMovement(currentAngle, direction, speed);
            var newPosition = GetNewPositionInternal(oldPosition, movement);

            return Tuple.Create(movement, newPosition);
        }

        public static Point2D GetNewPosition(Point2D oldPosition, GameAngle angle, float speed)
        {
            var rad = angle.RadianValue;
            var result = oldPosition + new Vector2D(speed * rad.Cos(), speed * rad.Sin());
            return result;
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
            if (position.GetDistanceSquared(targetPoint).IsZero())
            {
                return MoveDirection.None;
            }

            //// TODO [vmcl] Use computed MoveDirection instead of one of predefined ones

            var resultProxy = Tuple.Create(MoveDirection.None, float.MaxValue);
            foreach (var item in BasicMoveDirectionsField)
            {
                var potentialMovePoint = GetNewPosition(
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
            var result = GetBeakTurn(beakAngle, targetAngle);
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

        #region Private Methods

        private static float GetDistanceToLineInternal(Vector2D pointDirection, Vector2D lineDirection)
        {
            var projection = pointDirection.ProjectScalar(lineDirection);
            var result = (pointDirection.GetLengthSquared() - projection.Sqr()).Sqrt();
            return result;
        }

        private static Point2D GetNewPositionInternal(Point2D oldPosition, Vector2D movement)
        {
            var result = movement == Vector2D.Zero ? oldPosition : oldPosition + movement;
            return result;
        }

        #endregion
    }
}