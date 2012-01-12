﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using SharpestBeak.Common.Diagnostics;
using SharpestBeak.Common.Elements.Primitives;

namespace SharpestBeak.Common
{
    public static class CollisionDetector
    {
        #region Private Methods

        private static bool HasSeparatingAxis(
            ConvexPolygonPrimitive polygon,
            ConvexPolygonPrimitive otherPolygon)
        {
            for (int index = 0; index < polygon.Edges.Count; index++)
            {
                var direction = polygon.Edges[index].GetNormal();

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
                return CollisionDetector.CheckElementCollision(otherElement, primitive);
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
                var roughPrimitives = element.GetRoughPrimitives();
                foreach (var roughPrimitive in roughPrimitives)
                {
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

            var primitives = element.GetPrimitives();
            foreach (var primitive in primitives)
            {
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

            var dir1 = line1.Second - line1.First;
            var dir2 = line2.Second - line2.First;

            var denominator = dir2.Y * dir1.X - dir2.X * dir1.Y;
            var numeratorA = dir2.X * (line1.First.Y - line2.First.Y) - dir2.Y * (line1.First.X - line2.First.X);
            var numeratorB = dir1.X * (line1.First.Y - line2.First.Y) - dir1.Y * (line1.First.X - line2.First.X);

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

            return a.IsInRange(0f, 1f) && b.IsInRange(0f, 1f);
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
            PerformanceCounterHelper.Instance.CollisionCountPerStep.Increment();
            return result;
        }

        #endregion
    }
}