using System;
using System.Diagnostics;
using SharpestBeak.Physics;

namespace SharpestBeak.Model;

/// <summary>
///     Specifies the move direction.
/// </summary>
public sealed class MoveDirection
{
    public static readonly MoveDirection None = new(Vector2D.Zero, 0f);

    public static readonly MoveDirection MoveForward = new(
        Vector2D.UnitY,
        GameConstants.NormalizedScalarSpeedRange.Max);

    public static readonly MoveDirection MoveBackward = new(
        -Vector2D.UnitY,
        GameConstants.NormalizedScalarSpeedRange.Max);

    public static readonly MoveDirection StrafeLeft = new(
        -Vector2D.UnitX,
        GameConstants.NormalizedScalarSpeedRange.Max);

    public static readonly MoveDirection StrafeRight = new(
        Vector2D.UnitX,
        GameConstants.NormalizedScalarSpeedRange.Max);

    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveDirection"/> class.
    /// </summary>
    public MoveDirection(Vector2D direction, float normalizedScalarSpeed)
    {
        if (!normalizedScalarSpeed.IsInRange(GameConstants.NormalizedScalarSpeedRange))
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalizedScalarSpeed),
                normalizedScalarSpeed,
                $"The value is out of the valid range {GameConstants.NormalizedScalarSpeedRange}.");
        }

        NormalizedDirection = GetNormalizedDirection(direction, normalizedScalarSpeed);
    }

    public Vector2D NormalizedDirection { get; }

    [DebuggerNonUserCode]
    public bool IsNone => NormalizedDirection == Vector2D.Zero;

    public override string ToString() => $"{GetType().Name}: NormalizedDirection = {NormalizedDirection}";

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
}