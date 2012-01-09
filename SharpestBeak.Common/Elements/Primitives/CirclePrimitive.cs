using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class CirclePrimitive : ICollidablePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CirclePrimitive"/> class.
        /// </summary>
        public CirclePrimitive(PointF center, float radius)
        {
            #region Argument Check

            if (radius < 0f || radius.IsZero())
            {
                throw new ArgumentOutOfRangeException("radius", radius, "Circle radius must be positive.");
            }

            #endregion

            this.Center = center;
            this.Radius = radius;
        }

        #endregion

        #region Public Properties

        public PointF Center
        {
            get;
            private set;
        }

        public float Radius
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
            throw new NotImplementedException();
        }

        #endregion
    }
}