using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class CirclePrimitive : ICollidablePrimitive
    {
        #region Fields

        private float m_radiusSqr = float.MinValue;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CirclePrimitive"/> class.
        /// </summary>
        public CirclePrimitive(Point2D center, float radius)
        {
            #region Argument Check

            if (radius.IsNegativeOrZero())
            {
                throw new ArgumentOutOfRangeException("radius", radius, "Circle radius must be positive.");
            }

            #endregion

            this.Center = center;
            this.Radius = radius;
        }

        #endregion

        #region Public Properties

        public Point2D Center
        {
            get;
            private set;
        }

        public float Radius
        {
            get;
            private set;
        }

        public float RadiusSquared
        {
            [DebuggerNonUserCode]
            get
            {
                if (m_radiusSqr <= 0f)
                {
                    m_radiusSqr = this.Radius.Sqr();
                }
                return m_radiusSqr;
            }
        }

        #endregion

        #region ICollidablePrimitive Members

        public bool HasCollision(ICollidablePrimitive other)
        {
            #region Argument Check

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            var otherCircle = other as CirclePrimitive;
            if (otherCircle != null)
            {
                return CollisionDetector.CheckCircleToCircleCollision(this, otherCircle);
            }

            var line = other as LinePrimitive;
            if (line != null)
            {
                return CollisionDetector.CheckLineToCircleCollision(line, this);
            }

            var polygon = other as ConvexPolygonPrimitive;
            if (polygon != null)
            {
                return CollisionDetector.CheckCircleToPolygonCollision(this, polygon);
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
    }
}