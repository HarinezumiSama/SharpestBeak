using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SharpestBeak.Physics;

[Serializable]
public readonly struct Point2D : IEquatable<Point2D>
{
    public static readonly Point2D Zero = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="Point2D"/> class.
    /// </summary>
    public Point2D(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Point2D"/> class.
    /// </summary>
    public Point2D(Point point)
        : this(point.X, point.Y)
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

    public float X { get; }

    public float Y { get; }

    public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);

    public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

    public static Point2D operator +(Point2D value) => value;

    public static Point2D operator -(Point2D value) => value.Negate();

    public static Point2D operator +(Vector2D left, Point2D right) => new(left.X + right.X, left.Y + right.Y);

    public static Point2D operator +(Point2D left, Vector2D right) => new(left.X + right.X, left.Y + right.Y);

    public static Vector2D operator -(Point2D left, Point2D right) => new(left.X - right.X, left.Y - right.Y);

    public static Point2D operator -(Vector2D left, Point2D right) => new(left.X - right.X, left.Y - right.Y);

    public static Point2D operator -(Point2D left, Vector2D right) => new(left.X - right.X, left.Y - right.Y);

    public static Point2D operator *(Point2D left, float right) => new(left.X * right, left.Y * right);

    public static Point2D operator *(float left, Point2D right) => new(left * right.X, left * right.Y);

    public static Point2D operator /(Point2D left, float right) => new(left.X / right, left.Y / right);

    public static implicit operator Point2D(Point value) => new(value);

    public static implicit operator Point2D(PointF value) => new(value);

    public static implicit operator Point2D(SizeF value) => new(value);

    public static explicit operator Point2D(Vector2D value) => new(value);

    public static explicit operator PointF(Point2D value) => value.ToPointF();

    public static explicit operator SizeF(Point2D value) => value.ToSizeF();

    public static Point2D[] Rotate(IEnumerable<Point2D> values, Point2D center, GameAngle angle)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        angle.CosSin(out var cos, out var sin);

        return values.Select(value => RotatePointInternal(value, center, cos, sin)).ToArray();
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public override bool Equals(object obj) => obj is Point2D castObj && Equals(castObj);

    public override string ToString() => $"{{{GetType().Name}: X = {X}, Y = {Y}}}";

    public PointF ToPointF() => new(X, Y);

    public SizeF ToSizeF() => new(X, Y);

    public Vector2D ToVector2D() => new(this);

    public Point2D OffsetX(float offset) => new(X + offset, Y);

    public Point2D OffsetY(float offset) => new(X, Y + offset);

    public Point2D Negate() => new(-X, -Y);

    public float GetDistanceSquared(Point2D otherValue) => (otherValue.ToVector2D() - ToVector2D()).GetLengthSquared();

    public float GetDistance(Point2D otherValue) => GetDistanceSquared(otherValue).Sqrt();

    public Point2D Rotate(Point2D center, GameAngle angle)
    {
        angle.CosSin(out var cos, out var sin);

        return RotatePointInternal(this, center, cos, sin);
    }

    public Point2D Project(Vector2D target)
    {
        var coefficient = ToVector2D() * target / target.GetLengthSquared();
        return target.ToPoint2D() * coefficient;
    }

    public float ProjectScalar(Vector2D target) => ToVector2D() * target / target.GetLength();

    //// ReSharper disable CompareOfFloatsByEqualityOperator
    public bool Equals(Point2D other) => X == other.X && Y == other.Y;
    //// ReSharper restore CompareOfFloatsByEqualityOperator

    internal Point2D RotateHalfRevolutionClockwise() => new(Y, -X);

    private static Point2D RotatePointInternal(Point2D value, Point2D center, float cos, float sin)
    {
        var newX = center.X + (value.X - center.X) * cos - (value.Y - center.Y) * sin;
        var newY = center.Y + (value.X - center.X) * sin + (value.Y - center.Y) * cos;
        return new Point2D(newX, newY);
    }
}