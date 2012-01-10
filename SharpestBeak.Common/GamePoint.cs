using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public struct GamePoint : IEquatable<GamePoint>
    {
        #region Fields

        public static readonly GamePoint Zero = new GamePoint();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePoint"/> class.
        /// </summary>
        public GamePoint(float x, float y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePoint"/> class.
        /// </summary>
        public GamePoint(Point point)
            : this((float)point.X, (float)point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePoint"/> class.
        /// </summary>
        public GamePoint(PointF point)
            : this(point.X, point.Y)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePoint"/> class.
        /// </summary>
        public GamePoint(SizeF size)
            : this(size.Width, size.Height)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public float X
        {
            get;
            private set;
        }

        public float Y
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is GamePoint && Equals((GamePoint)obj);
        }

        public override string ToString()
        {
            return string.Format("{{{0}: X = {1}, Y = {2}}}", GetType().Name, this.X, this.Y);
        }

        public PointF ToPointF()
        {
            return new PointF(this.X, this.Y);
        }

        public SizeF ToSizeF()
        {
            return new SizeF(this.X, this.Y);
        }

        public GamePoint OffsetX(float offset)
        {
            return new GamePoint(this.X + offset, this.Y);
        }

        public GamePoint OffsetY(float offset)
        {
            return new GamePoint(this.X, this.Y + offset);
        }

        public GamePoint Negate()
        {
            return new GamePoint(-this.X, -this.Y);
        }

        public float GetLengthSquared()
        {
            return this.X.Sqr() + this.Y.Sqr();
        }

        public float GetLength()
        {
            return this.GetLengthSquared().Sqrt();
        }

        public float GetDistanceSquared(GamePoint otherValue)
        {
            return (otherValue - this).GetLengthSquared();
        }

        public float GetDistance(GamePoint otherValue)
        {
            return GetDistanceSquared(otherValue).Sqrt();
        }

        public GamePoint Project(GamePoint target)
        {
            var coefficient = (this * target) / target.GetLengthSquared();
            return new GamePoint(target.X * coefficient, target.Y * coefficient);
        }

        #endregion

        #region Operators

        public static bool operator ==(GamePoint left, GamePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GamePoint left, GamePoint right)
        {
            return !left.Equals(right);
        }

        public static GamePoint operator +(GamePoint value)
        {
            return value;
        }

        public static GamePoint operator -(GamePoint value)
        {
            return value.Negate();
        }

        public static GamePoint operator +(GamePoint left, GamePoint right)
        {
            return new GamePoint(left.X + right.X, left.Y + right.Y);
        }

        public static GamePoint operator -(GamePoint left, GamePoint right)
        {
            return new GamePoint(left.X - right.X, left.Y - right.Y);
        }

        public static GamePoint operator *(GamePoint left, float right)
        {
            return new GamePoint(left.X * right, left.Y * right);
        }

        public static GamePoint operator *(float left, GamePoint right)
        {
            return new GamePoint(left * right.X, left * right.Y);
        }

        public static float operator *(GamePoint left, GamePoint right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        public static implicit operator GamePoint(Point value)
        {
            return new GamePoint(value);
        }

        public static implicit operator GamePoint(PointF value)
        {
            return new GamePoint(value);
        }

        public static implicit operator GamePoint(SizeF value)
        {
            return new GamePoint(value);
        }

        public static explicit operator PointF(GamePoint value)
        {
            return value.ToPointF();
        }

        public static explicit operator SizeF(GamePoint value)
        {
            return value.ToSizeF();
        }

        #endregion

        #region IEquatable<GamePoint> Members

        public bool Equals(GamePoint other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        #endregion
    }
}