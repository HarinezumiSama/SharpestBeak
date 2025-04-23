using System;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public sealed class ConvexPolygonPrimitive : PolygonPrimitive
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConvexPolygonPrimitive"/> class.
    /// </summary>
    public ConvexPolygonPrimitive(params Point2D[] vertices)
        : base(AdjustPolygon(vertices))
    {
        // Nothing to do
    }

    public override bool HasCollision(ICollidablePrimitive other)
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

    public override bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);

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
    private static Point2D[] AdjustPolygon(Point2D[] vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        var edges = GetEdges(vertices);
        var convexState = GetConvexState(edges);

        return convexState switch
        {
            ConvexState.Undefined or ConvexState.Concave => throw new ArgumentException("The vertices must define a convex polygon", nameof(vertices)),
            ConvexState.ConvexCounterClockwise => vertices,
            ConvexState.ConvexClockwise => vertices.Reverse().ToArray(),
            _ => throw convexState.CreateEnumValueNotSupportedException()
        };
    }
}