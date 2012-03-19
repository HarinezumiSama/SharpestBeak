using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak
{
    [Serializable]
    public struct Vector2D : IEquatable<Vector2D>
    {
        #region Fields

        public static readonly Vector2D Zero = new Vector2D();
        public static readonly Vector2D UnitX = new Vector2D(1f, 0f);
        public static readonly Vector2D UnitY = new Vector2D(0f, 1f);
        public static readonly Vector2D One = new Vector2D(1f, 1f);

        private readonly float m_x;
        private readonly float m_y;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        public Vector2D(float x, float y)
        {
            m_x = x;
            m_y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        public Vector2D(Point point)
            : this((float)point.X, (float)point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        public Vector2D(PointF point)
            : this(point.X, point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        public Vector2D(SizeF size)
            : this(size.Width, size.Height)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Vector2D"/> class.
        /// </summary>
        public Vector2D(Point2D value)
            : this(value.X, value.Y)
        {
            // Nothing to do
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

        public static float DotProduct(Vector2D left, Vector2D right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        // Returns Z component of resulting Vector3D(0, 0, Z)
        public static float CrossProduct(Vector2D left, Vector2D right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        public override int GetHashCode()
        {
            return m_x.GetHashCode() ^ m_y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2D && Equals((Vector2D)obj);
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

        public Point2D ToPoint2D()
        {
            return new Point2D(this);
        }

        public Vector2D OffsetX(float offset)
        {
            return new Vector2D(m_x + offset, m_y);
        }

        public Vector2D OffsetY(float offset)
        {
            return new Vector2D(m_x, m_y + offset);
        }

        public Vector2D Negate()
        {
            return new Vector2D(-m_x, -m_y);
        }

        public float GetLengthSquared()
        {
            return m_x.Sqr() + m_y.Sqr();
        }

        public float GetLength()
        {
            return this.GetLengthSquared().Sqrt();
        }

        public float GetDistanceSquared(Vector2D otherValue)
        {
            return (otherValue - this).GetLengthSquared();
        }

        public float GetDistance(Vector2D otherValue)
        {
            return GetDistanceSquared(otherValue).Sqrt();
        }

        public Vector2D GetNormal()
        {
            return new Vector2D(m_y, -m_x);
        }

        public Vector2D Normalize()
        {
            var length = GetLength();
            return length.IsZero() ? this : this / length;
        }

        public Vector2D Project(Vector2D target)
        {
            var coefficient = (this * target) / target.GetLengthSquared();
            return target * coefficient;
        }

        public float ProjectScalar(Vector2D target)
        {
            return (this * target) / target.GetLength();
        }

        public GameAngle GetAngle(Vector2D target)
        {
            var radianAngle = MathHelper.Atan2(target.m_y, target.m_x) - MathHelper.Atan2(m_y, m_x);
            var degreeAngle = radianAngle.ToDegrees();
            return GameAngle.FromDegrees(GameAngle.NormalizeDegreeAngle(degreeAngle));
        }

        public float GetAngleCosine(Vector2D target)
        {
            return (this * target) / (this.GetLength() * target.GetLength());
        }

        #endregion

        #region Operators

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }

        public static Vector2D operator +(Vector2D value)
        {
            return value;
        }

        public static Vector2D operator -(Vector2D value)
        {
            return value.Negate();
        }

        public static Vector2D operator !(Vector2D value)
        {
            return value.GetNormal();
        }

        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2D operator *(Vector2D left, float right)
        {
            return new Vector2D(left.X * right, left.Y * right);
        }

        public static Vector2D operator *(float left, Vector2D right)
        {
            return new Vector2D(left * right.X, left * right.Y);
        }

        public static Vector2D operator /(Vector2D left, float right)
        {
            if (right.IsZero())
            {
                throw new DivideByZeroException();
            }
            return left * (1f / right);
        }

        public static float operator *(Vector2D left, Vector2D right)
        {
            return DotProduct(left, right);
        }

        /// <summary>
        ///     Computes the cross product of the specified vectors.
        ///     Returns the Z component of the resulting 3D vector (0, 0, Z).
        /// </summary>
        /// <param name="left">
        ///     The first vector of the cross product.
        /// </param>
        /// <param name="right">
        ///     The second vector of the cross product.
        /// </param>
        /// <returns>
        ///     The Z component of the resulting 3D vector (0, 0, Z).
        /// </returns>
        public static float operator ^(Vector2D left, Vector2D right)
        {
            return CrossProduct(left, right);
        }

        public static implicit operator Vector2D(Point value)
        {
            return new Vector2D(value);
        }

        public static implicit operator Vector2D(PointF value)
        {
            return new Vector2D(value);
        }

        public static implicit operator Vector2D(SizeF value)
        {
            return new Vector2D(value);
        }

        public static explicit operator Vector2D(Point2D value)
        {
            return new Vector2D(value);
        }

        public static explicit operator PointF(Vector2D value)
        {
            return value.ToPointF();
        }

        public static explicit operator SizeF(Vector2D value)
        {
            return value.ToSizeF();
        }

        #endregion

        #region IEquatable<Vector2D> Members

        public bool Equals(Vector2D other)
        {
            return m_x == other.X && m_y == other.Y;
        }

        #endregion
    }
}