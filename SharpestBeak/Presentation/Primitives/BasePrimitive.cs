using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public abstract class BasePrimitive
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BasePrimitive"/> class.
    /// </summary>
    internal BasePrimitive()
    {
        // Nothing to do
    }

    public Point2D BasePoint
    {
        get;
        protected set;
    }
}