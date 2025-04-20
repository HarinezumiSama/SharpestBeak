using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public sealed class ConvexPolygonPrimitive : PolygonPrimitive, ICollidablePrimitive
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConvexPolygonPrimitive"/> class.
    /// </summary>
    public ConvexPolygonPrimitive(ICollection<Point2D> vertices)
        : base(AdjustPolygon(vertices))
    {
        // Nothing to do
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConvexPolygonPrimitive"/> class.
    /// </summary>
    public ConvexPolygonPrimitive(params Point2D[] vertices)
        : this((ICollection<Point2D>)vertices)
    {
        // Nothing to do
    }

    [DebuggerNonUserCode]
    Point2D ICollidablePrimitive.BasePoint => BasePoint;

    public bool HasCollision(ICollidablePrimitive other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (other is ConvexPolygonPrimitive otherCpp)
        {
            return CollisionDetector.CheckPolygonToPolygonCollision(this, otherCpp);
        }

        if (other is LinePrimitive line)
        {
            return CollisionDetector.CheckLineToPolygonCollision(line, this);
        }

        if (other is CirclePrimitive circle)
        {
            return CollisionDetector.CheckCircleToPolygonCollision(circle, this);
        }

        throw new ArgumentException($"Unexpected object type {other.GetType().GetFullName().ToUIString()}.", nameof(other));
    }

    public bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);

    /// <summary>
    ///     Checks that the polygon defined by the specified vertices is convex and adjusts the order of vertices
    ///     so that it is counter-clockwise defined.
    /// </summary>
    /// <param name="vertices">
    ///     The vertices defining the polygon.
    /// </param>
    /// <returns>
    ///     The adjusted collection of vertices.
    /// </returns>
    private static IEnumerable<Point2D> AdjustPolygon(ICollection<Point2D> vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        var edges = GetEdges(vertices.ToList());
        var convexState = GetConvexState(edges);

        return convexState switch
        {
            ConvexState.Undefined or ConvexState.Concave => throw new ArgumentException("The vertices must define a convex polygon", nameof(vertices)),
            ConvexState.ConvexCounterClockwise => vertices,
            ConvexState.ConvexClockwise => vertices.Reverse(),
            _ => throw new NotSupportedException()
        };
    }
}