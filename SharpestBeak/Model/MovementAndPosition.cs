using SharpestBeak.Physics;

namespace SharpestBeak.Model;

public readonly struct MovementAndPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MovementAndPosition"/> class.
    /// </summary>
    internal MovementAndPosition(Vector2D movement, Point2D position)
    {
        Movement = movement;
        Position = position;
    }

    public Vector2D Movement { get; }

    public Point2D Position { get; }
}