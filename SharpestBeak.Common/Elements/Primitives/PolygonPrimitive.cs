using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class PolygonPrimitive : ICollidablePrimitive
    {
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
        }

        #endregion

        #region Public Properties

        public IList<Point2D> Vertices
        {
            get;
            private set;
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