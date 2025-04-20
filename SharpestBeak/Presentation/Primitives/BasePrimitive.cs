using System;
using System.Drawing;
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

    //// TODO: [VM] To be removed
    public void Draw(Graphics graphics, DrawData data)
    {
        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        OnDraw(graphics, data);
    }

    //// TODO: [VM] To be removed
    protected abstract void OnDraw(Graphics graphics, DrawData data);
}