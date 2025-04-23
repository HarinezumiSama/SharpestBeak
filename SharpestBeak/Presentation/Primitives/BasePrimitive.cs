using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public abstract class BasePrimitive : ICollidablePrimitive
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BasePrimitive"/> class.
    /// </summary>
    internal BasePrimitive()
    {
        // Nothing to do
    }

    public Point2D BasePoint { get; protected set; }

    public abstract bool HasCollision(ICollidable other);

    public abstract bool HasCollision(ICollidablePrimitive other);
}