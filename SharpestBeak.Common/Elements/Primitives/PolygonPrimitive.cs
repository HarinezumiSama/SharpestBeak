using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public class PolygonPrimitive
    {
        #region Nested Types

        #region ConvexSign Enumeration

        [Flags]
        private enum ConvexSign
        {
            None = 0,

            Positive = 0x1,
            Negative = 0x2,

            Both = Positive | Negative
        }

        #endregion

        #endregion

        #region Constants

        public const int MinVertexCount = 3;

        #endregion

        #region Fields

        private ConvexState? m_convexState;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonPrimitive"/> class.
        /// </summary>
        public PolygonPrimitive(IEnumerable<Point2D> vertices)
        {
            #region Argument Check

            if (vertices == null)
            {
                throw new ArgumentNullException("vertices");
            }

            #endregion

            this.Vertices = vertices.ToList().AsReadOnly();
            this.Edges = GetEdges(this.Vertices);
            this.Count = this.Vertices.Count;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonPrimitive"/> class.
        /// </summary>
        public PolygonPrimitive(params Point2D[] vertices)
            : this((IEnumerable<Point2D>)vertices)
        {
            // Nothing to do
        }

        #endregion

        #region Protected Methods

        protected static IList<Vector2D> GetEdges(IList<Point2D> vertices)
        {
            #region Argument Check

            if (vertices == null)
            {
                throw new ArgumentNullException("vertices");
            }
            if (vertices.Count < MinVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of vertices in the polygon must be at least {0} while it is {1}.",
                        MinVertexCount,
                        vertices.Count),
                    "vertices");
            }

            #endregion

            var count = vertices.Count;
            var resultProxy = new List<Vector2D>(count);
            var currentPoint = vertices[0];
            for (int nextIndex = 1; nextIndex <= count; nextIndex++)
            {
                var nextPoint = nextIndex < count ? vertices[nextIndex] : vertices[0];
                resultProxy.Add(nextPoint - currentPoint);
                currentPoint = nextPoint;
            }
            if (resultProxy.Count != count)
            {
                throw new InvalidOperationException();
            }
            return resultProxy.AsReadOnly();
        }

        protected static ConvexState GetConvexState(IList<Vector2D> edges)
        {
            #region Argument Check

            if (edges == null)
            {
                throw new ArgumentNullException("edges");
            }
            if (edges.Count < MinVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of edges in the polygon must be at least {0} while it is {1}.",
                        MinVertexCount,
                        edges.Count),
                    "edges");
            }

            #endregion

            ConvexSign sign = ConvexSign.None;
            for (int index = 0; index < edges.Count; index++)
            {
                var nextIndex = (index + 1) % edges.Count;

                var edge1 = edges[index];
                var edge2 = edges[nextIndex];

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

            switch (sign)
            {
                case ConvexSign.None:
                    return ConvexState.Undefined;
                case ConvexSign.Positive:
                    return ConvexState.ConvexCounterClockwise;
                case ConvexSign.Negative:
                    return ConvexState.ConvexClockwise;
            }

            throw new InvalidOperationException(
                string.Format("Unexpected convex computation state: {0}.", sign));
        }

        #endregion

        #region Public Properties

        public IList<Point2D> Vertices
        {
            get;
            private set;
        }

        public IList<Vector2D> Edges
        {
            get;
            private set;
        }

        public int Count
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public ConvexState GetConvexState()
        {
            if (!m_convexState.HasValue)
            {
                m_convexState = GetConvexState(this.Edges);
            }
            return m_convexState.Value;
        }

        public bool IsConvex()
        {
            var state = GetConvexState();
            return state == ConvexState.ConvexClockwise || state == ConvexState.ConvexCounterClockwise;
        }

        #endregion
    }
}