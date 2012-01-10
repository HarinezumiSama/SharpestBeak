using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class PolygonPrimitive : ICollidablePrimitive
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
            this.VertexCount = this.Vertices.Count;

            #region Argument Check

            if (this.VertexCount < c_minVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of vertices in the polygon must be at least {0} while is {1}.",
                        c_minVertexCount,
                        this.VertexCount),
                    "vertices");
            }

            #endregion
        }

        #endregion

        #region Public Properties

        public IList<Point2D> Vertices
        {
            get;
            private set;
        }

        public int VertexCount
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool IsConvex()
        {
            ConvexSign sign = ConvexSign.None;

            for (int index = 0; index < this.VertexCount; index++)
            {
                var index1 = (index + 1) % this.VertexCount;
                var index2 = (index + 2) % this.VertexCount;

                var edge1 = this.Vertices[index1] - this.Vertices[index];
                var edge2 = this.Vertices[index2] - this.Vertices[index1];

                var z = edge1.X * edge2.Y - edge1.Y * edge2.X;
                if (z.IsPositive())
                {
                    sign |= ConvexSign.Positive;
                }
                else if (z.IsNegative())
                {
                    sign |= ConvexSign.Negative;
                }

                if (sign == ConvexSign.Both)
                {
                    return false;
                }
            }

            return sign == ConvexSign.Positive || sign == ConvexSign.Negative;
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