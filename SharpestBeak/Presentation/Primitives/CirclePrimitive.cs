﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives
{
    public sealed class CirclePrimitive : BasePrimitive, ICollidablePrimitive
    {
        #region Constants and Fields

        private float _radiusSqr = float.MinValue;

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
            this.BasePoint = center;
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
                if (_radiusSqr <= 0f)
                {
                    _radiusSqr = this.Radius.Sqr();
                }

                return _radiusSqr;
            }
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

        #region Protected Methods

        protected override void OnDraw(Graphics graphics, DrawData data)
        {
            var diameter = 2f * this.Radius;
            var location = new Point2D(this.Center.X - this.Radius, this.Center.Y - this.Radius) * data.Coefficient;
            var size = new Vector2D(diameter, diameter) * data.Coefficient;
            graphics.FillEllipse(data.Brush, new RectangleF(location.ToPointF(), size.ToSizeF()));
        }

        #endregion
    }
}