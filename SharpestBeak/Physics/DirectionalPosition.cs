using System;

namespace SharpestBeak.Physics;

public readonly struct DirectionalPosition : IDirectionalPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DirectionalPosition"/> structure.
    /// </summary>
    public DirectionalPosition(Point2D position, GameAngle angle)
    {
        Position = position;
        Angle = angle;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DirectionalPosition"/> class.
    /// </summary>
    public DirectionalPosition(IDirectionalPosition directionalPosition)
    {
        if (directionalPosition is null)
        {
            throw new ArgumentNullException(nameof(directionalPosition));
        }

        Position = directionalPosition.Position;
        Angle = directionalPosition.Angle;
    }

    public Point2D Position { get; }

    public GameAngle Angle { get; }

    public override string ToString() => $"[ {Position}; {Angle} ]";
}