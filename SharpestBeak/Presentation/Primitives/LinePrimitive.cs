using System;
using System.Diagnostics;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public sealed class LinePrimitive : BasePrimitive
{
    private Vector2D? _direction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LinePrimitive"/> class.
    /// </summary>
    public LinePrimitive(Point2D start, Point2D end)
    {
        if (start.GetDistance(end).IsZero())
        {
            throw new ArgumentException($"Line endpoints cannot be the same ({start} and {end}).", nameof(end));
        }

        Start = start;
        End = end;
        BasePoint = start;
    }

    public Point2D Start { get; }

    public Point2D End { get; }

    [DebuggerNonUserCode]
    public Vector2D Direction => _direction ??= End - Start;

    public LineSide GetLineSide(Point2D point) => CollisionDetector.GetLineSide(this, point);

    public override bool HasCollision(ICollidablePrimitive other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (other is LinePrimitive otherLine)
        {
            return CollisionDetector.CheckLineToLineCollision(this, otherLine);
        }

        if (other is CirclePrimitive circle)
        {
            return CollisionDetector.CheckLineToCircleCollision(this, circle);
        }

        if (other is ConvexPolygonPrimitive polygon)
        {
            return CollisionDetector.CheckLineToPolygonCollision(this, polygon);
        }

        throw new ArgumentException($"Unexpected object type {other.GetType().GetFullName().ToUIString()}.", nameof(other));
    }

    public override bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);
}