using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Elements;

public abstract class BaseElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseElement"/> class.
    /// </summary>
    internal BaseElement(Point2D position) => Position = position;

    public Point2D Position { get; }
}