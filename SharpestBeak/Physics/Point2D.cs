using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Physics
{
    [Serializable]
    public struct Point2D : IEquatable<Point2D>
    {
        #region Fields

        public static readonly Point2D Zero = new Point2D();

        private readonly float m_x;
        private readonly float m_y;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point2D"/> class.
        /// </summary>
        public Point2D(float x, float y)
        {
            m_x = x;
            m_y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point2D"/> class.
        /// </summary>
        public Point2D(Point point)
            : this((float)point.X, (float)point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point2D"/> class.
        /// </summary>
        public Point2D(PointF point)
            : this(point.X, point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point2D"/> class.
        /// </summary>
        public Point2D(SizeF size)
            : this(size.Width, size.Height)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point2D"/> class.
        /// </summary>
        public Point2D(Vector2D value)
            : this(value.X, value.Y)
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private static Point2D RotatePointInternal(Point2D value, Point2D center, float cos, float sin)
        {
            var newX = center.X + (value.X - center.X) * cos - (value.Y - center.Y) * sin;
            var newY = center.Y + (value.X - center.X) * sin + (value.Y - center.Y) * cos;
            return new Point2D(newX, newY);
        }

        #endregion

        #region Public Properties

        public float X
        {
            [DebuggerStepThrough]
            get
            {
                return m_x;
            }
        }

        public float Y
        {
            [DebuggerStepThrough]
            get
            {
                return m_y;
            }
        }

        #endregion

        #region Public Methods

        public static Point2D[] Rotate(IEnumerable<Point2D> values, Point2D center, GameAngle angle)
        {
            #region Argument Check

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            #endregion

            float cos, sin;
            angle.CosSin(out cos, out sin);

            return values.Select(value => RotatePointInternal(value, center, cos, sin)).ToArray();
        }

        public override int GetHashCode()
        {
            return m_x.GetHashCode() ^ m_y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Point2D && Equals((Point2D)obj);
        }

        public override string ToString()
        {
            return string.Format("{{{0}: X = {1}, Y = {2}}}", GetType().Name, m_x, m_y);
        }

        public PointF ToPointF()
        {
            return new PointF(m_x, m_y);
        }

        public SizeF ToSizeF()
        {
            return new SizeF(m_x, m_y);
        }

        public Vector2D ToVector2D()
        {
            return new Vector2D(this);
        }

        public Point2D OffsetX(float offset)
        {
            return new Point2D(m_x + offset, m_y);
        }

        public Point2D OffsetY(float offset)
        {
            return new Point2D(m_x, m_y + offset);
        }

        public Point2D Negate()
        {
            return new Point2D(-m_x, -m_y);
        }

        public float GetDistanceSquared(Point2D otherValue)
        {
            return (otherValue.ToVector2D() - this.ToVector2D()).GetLengthSquared();
        }

        public float GetDistance(Point2D otherValue)
        {
            return GetDistanceSquared(otherValue).Sqrt();
        }

        public Point2D Rotate(Point2D center, GameAngle angle)
        {
            float cos, sin;
            angle.CosSin(out cos, out sin);

            return RotatePointInternal(this, center, cos, sin);
        }

        public Point2D Project(Vector2D target)
        {
            var coefficient = (this.ToVector2D() * target) / target.GetLengthSquared();
            return target.ToPoint2D() * coefficient;
        }

        public float ProjectScalar(Vector2D target)
        {
            return (this.ToVector2D() * target) / target.GetLength();
        }

        #endregion

        #region Operators

        public static bool operator ==(Point2D left, Point2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point2D left, Point2D right)
        {
            return !left.Equals(right);
        }

        public static Point2D operator +(Point2D value)
        {
            return value;
        }

        public static Point2D operator -(Point2D value)
        {
            return value.Negate();
        }

        public static Point2D operator +(Vector2D left, Point2D right)
        {
            return new Point2D(left.X + right.X, left.Y + right.Y);
        }

        public static Point2D operator +(Point2D left, Vector2D right)
        {
            return new Point2D(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2D operator -(Point2D left, Point2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        public static Point2D operator -(Vector2D left, Point2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }

        public static Point2D operator -(Point2D left, Vector2D right)
        {
            return new Point2D(left.X - right.X, left.Y - right.Y);
        }

        public static Point2D operator *(Point2D left, float right)
        {
            return new Point2D(left.X * right, left.Y * right);
        }

        public static Point2D operator *(float left, Point2D right)
        {
            return new Point2D(left * right.X, left * right.Y);
        }

        public static Point2D operator /(Point2D left, float right)
        {
            return new Point2D(left.X / right, left.Y / right);
        }

        public static implicit operator Point2D(Point value)
        {
            return new Point2D(value);
        }

        public static implicit operator Point2D(PointF value)
        {
            return new Point2D(value);
        }

        public static implicit operator Point2D(SizeF value)
        {
            return new Point2D(value);
        }

        public static explicit operator Point2D(Vector2D value)
        {
            return new Point2D(value);
        }

        public static explicit operator PointF(Point2D value)
        {
            return value.ToPointF();
        }

        public static explicit operator SizeF(Point2D value)
        {
            return value.ToSizeF();
        }

        #endregion

        #region IEquatable<Point2D> Members

        public bool Equals(Point2D other)
        {
            return m_x == other.X && m_y == other.Y;
        }

        #endregion
    }
}