using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives
{
    public sealed class ConvexPolygonPrimitive : PolygonPrimitive, ICollidablePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConvexPolygonPrimitive"/> class.
        /// </summary>
        public ConvexPolygonPrimitive(IEnumerable<Point2D> vertices)
            : base(AdjustPolygon(vertices))
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConvexPolygonPrimitive"/> class.
        /// </summary>
        public ConvexPolygonPrimitive(params Point2D[] vertices)
            : this((IEnumerable<Point2D>)vertices)
        {
            // Nothing to do
        }

        #endregion

        #region ICollidablePrimitive Members

        Point2D ICollidablePrimitive.BasePoint
        {
            [DebuggerNonUserCode]
            get
            {
                return this.BasePoint;
            }
        }

        public bool HasCollision(ICollidablePrimitive other)
        {
            #region Argument Check

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            var otherCpp = other as ConvexPolygonPrimitive;
            if (otherCpp != null)
            {
                return CollisionDetector.CheckPolygonToPolygonCollision(this, otherCpp);
            }

            var line = other as LinePrimitive;
            if (line != null)
            {
                return CollisionDetector.CheckLineToPolygonCollision(line, this);
            }

            var circle = other as CirclePrimitive;
            if (circle != null)
            {
                return CollisionDetector.CheckCircleToPolygonCollision(circle, this);
            }

            throw new NotSupportedException();
        }

        #endregion

        #region ICollidable Members

        public bool HasCollision(ICollidable other)
        {
            return CollisionDetector.CheckPrimitiveCollision(this, other);
        }

        #endregion

        #region Private Methods

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
        private static IEnumerable<Point2D> AdjustPolygon(IEnumerable<Point2D> vertices)
        {
            #region Argument Check

            if (vertices == null)
            {
                throw new ArgumentNullException("vertices");
            }

            #endregion

            var edges = GetEdges(vertices.ToList());
            var convexState = GetConvexState(edges);

            switch (convexState)
            {
                case ConvexState.Undefined:
                case ConvexState.Concave:
                    throw new ArgumentException("The vertices must define a convex polygon", "vertices");

                case ConvexState.ConvexCounterClockwise:
                    return vertices;

                case ConvexState.ConvexClockwise:
                    return vertices.Reverse();

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}