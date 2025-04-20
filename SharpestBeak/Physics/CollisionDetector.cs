using System;
using System.Linq;
using SharpestBeak.Diagnostics;
using SharpestBeak.Presentation;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Physics;

public static class CollisionDetector
{
    private static readonly ValueRange<float> LineCollisionRange = new(0f, 1f);

    public static bool CheckCollision(ICollidable first, ICollidable second)
    {
        if (first is null)
        {
            throw new ArgumentNullException(nameof(first));
        }

        if (second is null)
        {
            throw new ArgumentNullException(nameof(second));
        }

        var result = first.HasCollision(second);
        if (SettingsCache.Instance.UsePerformanceCounters)
        {
            PerformanceCounterHelper.Instance.CollisionCountPerStep.Increment();
        }

        return result;
    }

    public static LineSide GetLineSide(Point2D lineStart, Point2D lineEnd, Point2D point)
    {
        var lineVector = lineEnd - lineStart;
        var pointVector = point - lineStart;
        var product = lineVector ^ pointVector;
        return (LineSide)Math.Sign(product.IsZero() ? 0f : product);
    }

    public static LineSide GetLineSide(LinePrimitive line, Point2D point)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        return GetLineSide(line.Start, line.End, point);
    }

    // Is the point inside or on the circle?
    public static bool IsPointInCircle(Point2D point, CirclePrimitive circle)
    {
        if (circle is null)
        {
            throw new ArgumentNullException(nameof(circle));
        }

        // If a point is on a circle, it is considered to be in this circle
        return (point.GetDistanceSquared(circle.Center) - circle.RadiusSquared).IsNegativeOrZero();
    }

    public static bool IsPointInPolygon(Point2D point, ConvexPolygonPrimitive polygon)
    {
        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        // If a point is on a polygon's line, it is considered to be in this polygon
        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < polygon.Edges.Count; index++)
        {
            var edge = polygon.Edges[index];
            if (GetLineSide(edge, point) == LineSide.Right)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool CheckPrimitiveCollision(ICollidablePrimitive primitive, ICollidable other)
    {
        if (primitive is null)
        {
            throw new ArgumentNullException(nameof(primitive));
        }

        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (other is ICollidablePrimitive otherPrimitive)
        {
            return primitive.HasCollision(otherPrimitive);
        }

        if (other is ICollidableElement otherElement)
        {
            return CheckElementCollision(otherElement, primitive);
        }

        throw new NotSupportedException();
    }

    internal static bool CheckElementCollision(ICollidableElement element, ICollidable other)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        if (element.HasRoughPrimitives)
        {
            var canCollide = false;
            var roughPrimitives = element.GetRoughPrimitives().EnsureNotNull();
            if (roughPrimitives.Count <= 0)
            {
                throw new InvalidOperationException($"The element {{{element}}} has empty rough primitives collection.");
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < roughPrimitives.Count; index++)
            {
                var roughPrimitive = roughPrimitives[index];
                if (roughPrimitive.HasCollision(other))
                {
                    canCollide = true;
                    break;
                }
            }

            if (!canCollide)
            {
                return false;
            }
        }

        var primitives = element.GetPrimitives().EnsureNotNull();
        if (primitives.Count <= 0)
        {
            throw new InvalidOperationException($"The element {{{element}}} has empty primitives collection.");
        }

        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < primitives.Count; index++)
        {
            var primitive = primitives[index];
            if (CheckPrimitiveCollision(primitive, other))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool CheckLineToLineCollision(LinePrimitive line1, LinePrimitive line2)
    {
        if (line1 is null)
        {
            throw new ArgumentNullException(nameof(line1));
        }

        if (line2 is null)
        {
            throw new ArgumentNullException(nameof(line2));
        }

        var dir1 = line1.Direction;
        var dir2 = line2.Direction;

        var denominator = dir2.Y * dir1.X - dir2.X * dir1.Y;
        var numeratorA = dir2.X * (line1.Start.Y - line2.Start.Y) - dir2.Y * (line1.Start.X - line2.Start.X);
        var numeratorB = dir1.X * (line1.Start.Y - line2.Start.Y) - dir1.Y * (line1.Start.X - line2.Start.X);

        if (denominator.IsZero())
        {
            if (numeratorA.IsZero() || numeratorB.IsZero())
            {
                // Both lines are same line (with respect to zero tolerance)
                return true;
            }

            // Lines are parallel
            return false;
        }

        var a = numeratorA / denominator;
        var b = numeratorB / denominator;

        return a.IsInRange(LineCollisionRange) && b.IsInRange(LineCollisionRange);
    }

    internal static bool CheckCircleToCircleCollision(CirclePrimitive circle1, CirclePrimitive circle2)
    {
        if (circle1 is null)
        {
            throw new ArgumentNullException(nameof(circle1));
        }

        if (circle2 is null)
        {
            throw new ArgumentNullException(nameof(circle2));
        }

        var centerDistanceSqr = circle1.Center.GetDistanceSquared(circle2.Center);
        var radiusSumSqr = (circle1.Radius + circle2.Radius).Sqr();
        var difference = radiusSumSqr - centerDistanceSqr;

        return difference > 0f || difference.IsZero();
    }

    internal static bool CheckLineToCircleCollision(LinePrimitive line, CirclePrimitive circle)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        if (circle is null)
        {
            throw new ArgumentNullException(nameof(circle));
        }

        // It's enough to check only one of the line's points
        if (IsPointInCircle(line.Start, circle))
        {
            return true;
        }

        var circleDirection = line.Start - circle.Center;

        var a = line.Direction * line.Direction;
        var b = 2 * (circleDirection * line.Direction);
        var c = circleDirection * circleDirection - circle.RadiusSquared;

        var d = b.Sqr() - 4 * a * c;
        return d.IsPositiveOrZero();
    }

    internal static bool CheckLineToPolygonCollision(LinePrimitive line, ConvexPolygonPrimitive polygon)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        if (IsPointInPolygon(line.Start, polygon) || IsPointInPolygon(line.End, polygon))
        {
            return true;
        }

        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < polygon.Edges.Count; index++)
        {
            var edge = polygon.Edges[index];
            if (CheckLineToLineCollision(line, edge))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool CheckCircleToPolygonCollision(CirclePrimitive circle, ConvexPolygonPrimitive polygon)
    {
        if (circle is null)
        {
            throw new ArgumentNullException(nameof(circle));
        }

        if (polygon is null)
        {
            throw new ArgumentNullException(nameof(polygon));
        }

        return IsPointInPolygon(circle.Center, polygon)
            || polygon.Edges.Any(edge => CheckLineToCircleCollision(edge, circle));
    }

    internal static bool CheckPolygonToPolygonCollision(
        ConvexPolygonPrimitive polygon1,
        ConvexPolygonPrimitive polygon2)
    {
        if (polygon1 is null)
        {
            throw new ArgumentNullException(nameof(polygon1));
        }

        if (polygon2 is null)
        {
            throw new ArgumentNullException(nameof(polygon2));
        }

        return !HasSeparatingAxis(polygon1, polygon2) && !HasSeparatingAxis(polygon2, polygon1);
    }

    private static bool HasSeparatingAxis(PolygonPrimitive polygon, PolygonPrimitive otherPolygon)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < polygon.Edges.Count; index++)
        {
            var direction = polygon.Edges[index].Direction.GetNormal();

            var projections = polygon.Vertices.Select(p => p.ProjectScalar(direction)).ToArray();
            var otherProjections = otherPolygon.Vertices.Select(p => p.ProjectScalar(direction)).ToArray();

            projections.MinMax(out var min, out var max);

            otherProjections.MinMax(out var otherMin, out var otherMax);

            if (min > otherMax || max < otherMin)
            {
                return true;
            }
        }

        return false;
    }
}