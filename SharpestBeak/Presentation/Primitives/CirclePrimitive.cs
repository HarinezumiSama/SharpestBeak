using System;
using System.Diagnostics;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public sealed class CirclePrimitive : BasePrimitive
{
    private float _radiusSqr = float.MinValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CirclePrimitive"/> class.
    /// </summary>
    public CirclePrimitive(Point2D center, float radius)
    {
        if (radius.IsNegativeOrZero())
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Circle radius must be positive.");
        }

        Center = center;
        Radius = radius;
        BasePoint = center;
    }

    public Point2D Center { get; }

    public float Radius { get; }

    [DebuggerNonUserCode]
    public float RadiusSquared
    {
        get
        {
            if (_radiusSqr <= 0f)
            {
                _radiusSqr = Radius.Sqr();
            }

            return _radiusSqr;
        }
    }

    public override bool HasCollision(ICollidablePrimitive other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (other is CirclePrimitive otherCircle)
        {
            return CollisionDetector.CheckCircleToCircleCollision(this, otherCircle);
        }

        if (other is LinePrimitive line)
        {
            return CollisionDetector.CheckLineToCircleCollision(line, this);
        }

        if (other is ConvexPolygonPrimitive polygon)
        {
            return CollisionDetector.CheckCircleToPolygonCollision(this, polygon);
        }

        throw new ArgumentException($"Unexpected object type {other.GetType().GetFullName().ToUIString()}.", nameof(other));
    }

    public override bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);
}