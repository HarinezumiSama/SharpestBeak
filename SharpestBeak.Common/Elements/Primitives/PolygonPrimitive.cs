using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class PolygonPrimitive : ICollidablePrimitive
    {
        #region Fields

        private ConvexState? m_convexState;

        #endregion

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

        private const int c_minVertexCount = 3;

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
            this.Count = this.Vertices.Count;

            #region Argument Check

            if (this.Count < c_minVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of vertices in the polygon must be at least {0} while is {1}.",
                        c_minVertexCount,
                        this.Count),
                    "vertices");
            }

            #endregion

            this.Edges = GetEdges(this.Vertices);
        }

        private IList<Vector2D> GetEdges(IList<Point2D> vertices)
        {
            var count = vertices.Count;
            var resultProxy = new List<Vector2D>(count);
            var currentPoint = this.Vertices[0];
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

        #endregion

        #region Private Methods

        private ConvexState GetConvexStateInternal()
        {
            ConvexSign sign = ConvexSign.None;
            for (int index = 0; index < this.Count; index++)
            {
                var nextIndex = (index + 1) % this.Count;

                var edge1 = this.Edges[index];
                var edge2 = this.Edges[nextIndex];

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

            throw new NotImplementedException();
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
                m_convexState = GetConvexStateInternal();
            }
            return m_convexState.Value;
        }

        public bool IsConvex()
        {
            var state = GetConvexState();
            return state == ConvexState.ConvexClockwise || state == ConvexState.ConvexCounterClockwise;
        }

        #endregion

        #region ICollidablePrimitive Members

        public bool HasCollision(ICollidablePrimitive other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollidable Members

        public bool HasCollision(ICollidable other)
        {
            return CollisionDetector.CheckPrimitiveCollision(this, other);
        }

        #endregion
    }
}