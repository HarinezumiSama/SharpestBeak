using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

public abstract class PolygonPrimitive : BasePrimitive
{
    public const int MinVertexCount = 3;

    private ConvexState? _convexState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PolygonPrimitive"/> class.
    /// </summary>
    protected PolygonPrimitive(params Point2D[] vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        Vertices = vertices.AsReadOnly();
        Edges = GetEdges(Vertices);
        Count = Vertices.Count;
        BasePoint = Vertices.First();
    }

    public ReadOnlyCollection<Point2D> Vertices { get; }

    public ReadOnlyCollection<LinePrimitive> Edges { get; }

    public int Count { get; }

    public ConvexState GetConvexState() => _convexState ??= GetConvexState(Edges);

    public bool IsConvex() => GetConvexState() is ConvexState.ConvexClockwise or ConvexState.ConvexCounterClockwise;

    protected static ReadOnlyCollection<LinePrimitive> GetEdges(IList<Point2D> vertices)
    {
        if (vertices is null)
        {
            throw new ArgumentNullException(nameof(vertices));
        }

        if (vertices.Count < MinVertexCount)
        {
            throw new ArgumentException(
                $"The number of vertices in the polygon must be at least {MinVertexCount} while it is {vertices.Count}.",
                nameof(vertices));
        }

        var count = vertices.Count;
        var resultProxy = new List<LinePrimitive>(count);
        var currentPoint = vertices[0];
        for (var nextIndex = 1; nextIndex <= count; nextIndex++)
        {
            var nextPoint = nextIndex < count ? vertices[nextIndex] : vertices[0];
            resultProxy.Add(new LinePrimitive(currentPoint, nextPoint));
            currentPoint = nextPoint;
        }

        if (resultProxy.Count != count)
        {
            throw new InvalidOperationException();
        }

        return resultProxy.AsReadOnly();
    }

    protected static ConvexState GetConvexState(IList<LinePrimitive> edges)
    {
        if (edges is null)
        {
            throw new ArgumentNullException(nameof(edges));
        }

        if (edges.Count < MinVertexCount)
        {
            throw new ArgumentException(
                $"The number of edges in the polygon must be at least {MinVertexCount} while it is {edges.Count}.",
                nameof(edges));
        }

        var sign = ConvexSign.None;
        for (var index = 0; index < edges.Count; index++)
        {
            var nextIndex = (index + 1) % edges.Count;

            var edge1 = edges[index].Direction;
            var edge2 = edges[nextIndex].Direction;

            var z = edge1 ^ edge2;
            if (z.IsPositive())
            {
                sign |= ConvexSign.Positive;
            }
            else if (z.IsNegative())
            {
                sign |= ConvexSign.Negative;
            }

            if ((sign & ConvexSign.Both) == ConvexSign.Both)
            {
                return ConvexState.Concave;
            }
        }

        return sign switch
        {
            ConvexSign.None => ConvexState.Undefined,
            ConvexSign.Positive => ConvexState.ConvexCounterClockwise,
            ConvexSign.Negative => ConvexState.ConvexClockwise,
            _ => throw new InvalidOperationException($"Unexpected convex computation state: {sign}.")
        };
    }

    [Flags]
    private enum ConvexSign
    {
        None = 0,

        Positive = 0x1,
        Negative = 0x2,

        Both = Positive | Negative
    }
}