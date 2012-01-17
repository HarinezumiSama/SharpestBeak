using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class LinePrimitive : BasePrimitive, ICollidablePrimitive
    {
        #region Fields

        private Vector2D? m_direction;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinePrimitive"/> class.
        /// </summary>
        public LinePrimitive(Point2D start, Point2D end)
        {
            #region Argument Check

            if (start.GetDistance(end).IsZero())
            {
                throw new ArgumentException(
                    string.Format("Line endpoints cannot be the same ({0} and {1}).", start, end),
                    "end");
            }

            #endregion

            this.Start = start;
            this.End = end;
            base.BasePoint = start;
        }

        #endregion

        #region Public Properties

        public Point2D Start
        {
            get;
            private set;
        }

        public Point2D End
        {
            get;
            private set;
        }

        public Vector2D Direction
        {
            [DebuggerNonUserCode]
            get
            {
                if (!m_direction.HasValue)
                {
                    m_direction = this.End - this.Start;
                }
                return m_direction.Value;
            }
        }

        #endregion

        #region Public Methods

        public LineSide GetLineSide(Point2D point)
        {
            return CollisionDetector.GetLineSide(this, point);
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

            var otherLine = other as LinePrimitive;
            if (otherLine != null)
            {
                return CollisionDetector.CheckLineToLineCollision(this, otherLine);
            }

            var circle = other as CirclePrimitive;
            if (circle != null)
            {
                return CollisionDetector.CheckLineToCircleCollision(this, circle);
            }

            var polygon = other as ConvexPolygonPrimitive;
            if (polygon != null)
            {
                return CollisionDetector.CheckLineToPolygonCollision(this, polygon);
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