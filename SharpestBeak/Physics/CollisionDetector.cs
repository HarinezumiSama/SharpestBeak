﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Diagnostics;
using SharpestBeak.Presentation;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Physics
{
    public static class CollisionDetector
    {
        #region Constants and Fields

        private static readonly ValueRange<float> LineCollisionRange = new ValueRange<float>(0f, 1f);

        #endregion

        #region Public Methods

        public static bool CheckCollision(ICollidable first, ICollidable second)
        {
            #region Argument Check

            if (first == null)
            {
                throw new ArgumentNullException("first");
            }

            if (second == null)
            {
                throw new ArgumentNullException("second");
            }

            #endregion

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
            #region Argument Check

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            #endregion

            return GetLineSide(line.Start, line.End, point);
        }

        // Is the point inside or on the circle?
        public static bool IsPointInCircle(Point2D point, CirclePrimitive circle)
        {
            #region Argument Check

            if (circle == null)
            {
                throw new ArgumentNullException("circle");
            }

            #endregion

            // If a point is on a circle, it is considered to be in this circle
            return (point.GetDistanceSquared(circle.Center) - circle.RadiusSquared).IsNegativeOrZero();
        }

        public static bool IsPointInPolygon(Point2D point, ConvexPolygonPrimitive polygon)
        {
            #region Argument Check

            if (polygon == null)
            {
                throw new ArgumentNullException("polygon");
            }

            #endregion

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

        #endregion

        #region Internal Methods

        internal static bool CheckPrimitiveCollision(ICollidablePrimitive primitive, ICollidable other)
        {
            #region Argument Check

            if (primitive == null)
            {
                throw new ArgumentNullException("primitive");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            var otherPrimitive = other as ICollidablePrimitive;
            if (otherPrimitive != null)
            {
                return primitive.HasCollision(otherPrimitive);
            }

            var otherElement = other as ICollidableElement;
            if (otherElement != null)
            {
                return CheckElementCollision(otherElement, primitive);
            }

            throw new NotSupportedException();
        }

        internal static bool CheckElementCollision(ICollidableElement element, ICollidable other)
        {
            #region Argument Check

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            if (element.HasRoughPrimitives)
            {
                var canCollide = false;
                var roughPrimitives = element.GetRoughPrimitives().EnsureNotNull();
                if (roughPrimitives.Count <= 0)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The element {{{0}}} has empty rough primitives collection.",
                            element));
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
                throw new InvalidOperationException(
                    string.Format(
                        "The element {{{0}}} has empty primitives collection.",
                        element));
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
            #region Argument Check

            if (line1 == null)
            {
                throw new ArgumentNullException("line1");
            }

            if (line2 == null)
            {
                throw new ArgumentNullException("line2");
            }

            #endregion

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
            #region Argument Check

            if (circle1 == null)
            {
                throw new ArgumentNullException("circle1");
            }

            if (circle2 == null)
            {
                throw new ArgumentNullException("circle2");
            }

            #endregion

            var centerDistanceSqr = circle1.Center.GetDistanceSquared(circle2.Center);
            var radiusSumSqr = (circle1.Radius + circle2.Radius).Sqr();
            var difference = radiusSumSqr - centerDistanceSqr;

            return difference > 0f || difference.IsZero();
        }

        internal static bool CheckLineToCircleCollision(LinePrimitive line, CirclePrimitive circle)
        {
            #region Argument Check

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            if (circle == null)
            {
                throw new ArgumentNullException("circle");
            }

            #endregion

            // It's enough to check only one of the line's points
            if (IsPointInCircle(line.Start, circle))
            {
                return true;
            }

            var circleDirection = line.Start - circle.Center;

            var a = line.Direction * line.Direction;
            var b = 2 * (circleDirection * line.Direction);
            var c = (circleDirection * circleDirection) - circle.RadiusSquared;

            var d = b.Sqr() - 4 * a * c;
            return d.IsPositiveOrZero();
        }

        internal static bool CheckLineToPolygonCollision(LinePrimitive line, ConvexPolygonPrimitive polygon)
        {
            #region Argument Check

            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            if (polygon == null)
            {
                throw new ArgumentNullException("polygon");
            }

            #endregion

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
            #region Argument Check

            if (circle == null)
            {
                throw new ArgumentNullException("circle");
            }

            if (polygon == null)
            {
                throw new ArgumentNullException("polygon");
            }

            #endregion

            return IsPointInPolygon(circle.Center, polygon)
                || polygon.Edges.Any(edge => CheckLineToCircleCollision(edge, circle));
        }

        internal static bool CheckPolygonToPolygonCollision(
            ConvexPolygonPrimitive polygon1,
            ConvexPolygonPrimitive polygon2)
        {
            #region Argument Check

            if (polygon1 == null)
            {
                throw new ArgumentNullException("polygon1");
            }

            if (polygon2 == null)
            {
                throw new ArgumentNullException("polygon2");
            }

            #endregion

            return !HasSeparatingAxis(polygon1, polygon2) && !HasSeparatingAxis(polygon2, polygon1);
        }

        #endregion

        #region Private Methods

        private static bool HasSeparatingAxis(PolygonPrimitive polygon, PolygonPrimitive otherPolygon)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < polygon.Edges.Count; index++)
            {
                var direction = polygon.Edges[index].Direction.GetNormal();

                var projections = polygon.Vertices.Select(p => p.ProjectScalar(direction)).ToArray();
                var otherProjections = otherPolygon.Vertices.Select(p => p.ProjectScalar(direction)).ToArray();

                float min, max;
                projections.MinMax(out min, out max);

                float otherMin, otherMax;
                otherProjections.MinMax(out otherMin, out otherMax);

                if (min > otherMax || max < otherMin)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}