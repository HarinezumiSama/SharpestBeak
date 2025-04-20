using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Omnifactotum;
using SharpestBeak.Model;
using SharpestBeak.Physics;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
// ReSharper disable once CheckNamespace
namespace SharpestBeak;

public static class GameHelper
{
    private static readonly Vector2D HalfNominalCellOffset = new(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);

    private static readonly ReadOnlyCollection<MoveDirection> BasicActiveMoveDirectionsField =
        new[] { MoveDirection.MoveForward, MoveDirection.MoveBackward, MoveDirection.StrafeLeft, MoveDirection.StrafeRight }.AsReadOnly();

    private static readonly ReadOnlyCollection<MoveDirection> BasicMoveDirectionsField =
        BasicActiveMoveDirectionsField.Concat(MoveDirection.None.AsCollection()).ToArray().AsReadOnly();

    private static readonly ReadOnlyCollection<FireMode> FireModesField = EnumFactotum.GetAllValues<FireMode>().AsReadOnly();

    public static ReadOnlyCollection<MoveDirection> BasicMoveDirections
    {
        [DebuggerStepThrough]
        get => BasicMoveDirectionsField;
    }

    public static ReadOnlyCollection<MoveDirection> BasicActiveMoveDirections
    {
        [DebuggerStepThrough]
        get => BasicActiveMoveDirectionsField;
    }

    public static ReadOnlyCollection<FireMode> FireModes
    {
        [DebuggerStepThrough]
        get => FireModesField;
    }

    /// <summary>
    ///     Gets the real board coordinates of the center of the nominal cell with the specified coordinates.
    /// </summary>
    /// <param name="nominalPoint">
    ///     The coordinates of the nominal cell to convert.
    /// </param>
    /// <returns>
    ///     The real board coordinates.
    /// </returns>
    public static Point2D NominalToReal(Point nominalPoint) => new Point2D(nominalPoint) * GameConstants.NominalCellSize + HalfNominalCellOffset;

    public static GameAngle GetBeakMovement(BeakTurn beakTurn)
    {
        if (beakTurn.Value.IsZero())
        {
            return GameAngle.Zero;
        }

        var degreeAngle = GameConstants.ChickenUnit.DefaultAngularSpeed * beakTurn.Value;
        return GameAngle.FromDegrees(degreeAngle);
    }

    public static GameAngle GetNewBeakAngle(GameAngle oldBeakAngle, BeakTurn beakTurn)
    {
        var beakMovement = GetBeakMovement(beakTurn);

        var result = oldBeakAngle + beakMovement;
        return result;
    }

    public static AngleMovementAndPosition GetBeakMovementAndNewAngle(
        GameAngle oldBeakAngle,
        BeakTurn beakTurn)
    {
        var beakMovement = GetBeakMovement(beakTurn);
        var newAngle = oldBeakAngle + beakMovement;
        return new AngleMovementAndPosition(beakMovement, newAngle);
    }

    public static Vector2D GetMovement(GameAngle angle, float speed) => new(speed * angle.RadianValue.Cos(), speed * angle.RadianValue.Sin());

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

    public static MovementAndPosition GetMovementAndNewPosition(
        Point2D oldPosition,
        GameAngle currentAngle,
        MoveDirection direction,
        float speed)
    {
        var movement = GetMovement(currentAngle, direction, speed);
        var newPosition = GetNewPositionInternal(oldPosition, movement);

        return new MovementAndPosition(movement, newPosition);
    }

    public static Point2D GetNewPosition(Point2D oldPosition, GameAngle angle, float speed)
        => oldPosition + new Vector2D(speed * angle.RadianValue.Cos(), speed * angle.RadianValue.Sin());

    public static Point2D GetBeakTipPosition(Point2D position, GameAngle beakAngle)
        => position
            .OffsetX(GameConstants.ChickenUnit.BeakOffset)
            .Rotate(position, beakAngle);

    public static Point2D GetBeakTipPosition(IDirectionalPosition directionalPosition)
    {
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

        return GetBeakTipPosition(directionalPosition.Position, directionalPosition.Angle);
    }

    public static Vector2D GetChickenViewDirection(Point2D position, GameAngle beakAngle)
    {
        var beakTipPosition = GetBeakTipPosition(position, beakAngle);
        return beakTipPosition - position;
    }

    public static Vector2D GetChickenViewDirection(IDirectionalPosition directionalPosition)
    {
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

        return GetChickenViewDirection(directionalPosition.Position, directionalPosition.Angle);
    }

    public static Point2D[] Rotate(this IEnumerable<Point2D> values, Point2D center, GameAngle angle) => Point2D.Rotate(values, center, angle);

    public static PointF[] ToPointF(this IEnumerable<Point2D> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return values.Select(item => item.ToPointF()).ToArray();
    }

    public static SizeF ToSizeF(this PointF value) => new(value);

    public static float GetDistanceToLine(Point2D point, Point2D linePoint, GameAngle lineAngle)
        => GetDistanceToLineInternal(lineAngle.ToUnitVector(), point - linePoint);

    public static float GetDistanceToLine(Point2D point, Point2D linePoint1, Point2D linePoint2)
        => GetDistanceToLineInternal(point - linePoint1, linePoint2 - linePoint1);

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
        return difference / GameConstants.ChickenUnit.DefaultAngularSpeed;
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
                GameConstants.ChickenUnit.DefaultRectilinearSpeed);

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
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

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
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

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
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

        return GetBestBeakTurnNormalized(directionalPosition.Position, directionalPosition.Angle, targetPoint);
    }

    private static float GetDistanceToLineInternal(Vector2D pointDirection, Vector2D lineDirection)
    {
        var projection = pointDirection.ProjectScalar(lineDirection);
        return (pointDirection.GetLengthSquared() - projection.Sqr()).Sqrt();
    }

    private static Point2D GetNewPositionInternal(Point2D oldPosition, Vector2D movement) => movement == Vector2D.Zero ? oldPosition : oldPosition + movement;
}