using SharpestBeak.Physics;

namespace SharpestBeak.Model;

public readonly struct AngleMovementAndPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AngleMovementAndPosition"/> class.
    /// </summary>
    internal AngleMovementAndPosition(GameAngle movement, GameAngle position)
    {
        Movement = movement;
        Position = position;
    }

    public GameAngle Movement { get; }

    public GameAngle Position { get; }
}