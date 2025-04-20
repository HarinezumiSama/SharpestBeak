using System;
using System.Drawing;
using Omnifactotum.Annotations;

namespace SharpestBeak.Physics;

[Serializable]
public readonly struct Vector2D : IEquatable<Vector2D>
{
    public static readonly Vector2D Zero = new();
    public static readonly Vector2D UnitX = new(1f, 0f);
    public static readonly Vector2D UnitY = new(0f, 1f);
    public static readonly Vector2D One = new(1f, 1f);

    /// <summary>
    ///     Initializes a new instance of the <see cref="Vector2D"/> class.
    /// </summary>
    public Vector2D(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Vector2D"/> class.
    /// </summary>
    public Vector2D(Point point)
        : this(point.X, point.Y)
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

    public float X { get; }

    public float Y { get; }

    public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);

    public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

    public static Vector2D operator +(Vector2D value) => value;

    public static Vector2D operator -(Vector2D value) => value.Negate();

    public static Vector2D operator !(Vector2D value) => value.GetNormal();

    public static Vector2D operator +(Vector2D left, Vector2D right) => new(left.X + right.X, left.Y + right.Y);

    public static Vector2D operator -(Vector2D left, Vector2D right) => new(left.X - right.X, left.Y - right.Y);

    public static Vector2D operator *(Vector2D left, float right) => new(left.X * right, left.Y * right);

    public static Vector2D operator *(float left, Vector2D right) => new(left * right.X, left * right.Y);

    public static Vector2D operator /(Vector2D left, float right)
    {
        if (right.IsZero())
        {
            throw new DivideByZeroException();
        }

        return left * (1f / right);
    }

    /// <summary>
    ///     Computes the dot product of the two specified 2D vectors.
    /// </summary>
    /// <param name="left">
    ///     The left operand.
    /// </param>
    /// <param name="right">
    ///     The right operand.
    /// </param>
    /// <returns>
    ///     A scalar value representing the dot product.
    /// </returns>
    public static float operator *(Vector2D left, Vector2D right) => DotProduct(left, right);

    /// <summary>
    ///     Computes the cross product of the two specified vectors.
    ///     Returns the Z component of the resulting 3D vector (0, 0, Z).
    /// </summary>
    /// <param name="left">
    ///     The first operand.
    /// </param>
    /// <param name="right">
    ///     The second operand.
    /// </param>
    /// <returns>
    ///     The Z component of the resulting 3D vector (0, 0, Z).
    /// </returns>
    public static float operator ^(Vector2D left, Vector2D right) => CrossProduct(left, right);

    public static implicit operator Vector2D(Point value) => new(value);

    public static implicit operator Vector2D(PointF value) => new(value);

    public static implicit operator Vector2D(SizeF value) => new(value);

    public static explicit operator Vector2D(Point2D value) => new(value);

    public static explicit operator PointF(Vector2D value) => value.ToPointF();

    public static explicit operator SizeF(Vector2D value) => value.ToSizeF();

    public static float DotProduct(Vector2D left, Vector2D right) => left.X * right.X + left.Y * right.Y;

    // Returns Z component of a resulting Vector3D(0, 0, Z)
    public static float CrossProduct(Vector2D left, Vector2D right) => left.X * right.Y - left.Y * right.X;

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

    public override bool Equals(object obj) => obj is Vector2D castObj && Equals(castObj);

    public override string ToString() => $"{{{GetType().Name}: X = {X}, Y = {Y}}}";

    public PointF ToPointF() => new(X, Y);

    public SizeF ToSizeF() => new(X, Y);

    public Point2D ToPoint2D() => new(this);

    public Vector2D OffsetX(float offset) => new(X + offset, Y);

    public Vector2D OffsetY(float offset) => new(X, Y + offset);

    public Vector2D Negate() => new(-X, -Y);

    public float GetLengthSquared() => X.Sqr() + Y.Sqr();

    public float GetLength() => GetLengthSquared().Sqrt();

    public float GetDistanceSquared(Vector2D otherValue) => (otherValue - this).GetLengthSquared();

    public float GetDistance(Vector2D otherValue) => GetDistanceSquared(otherValue).Sqrt();

    public Vector2D GetNormal() => new(Y, -X);

    public Vector2D Normalize()
    {
        var length = GetLength();
        return length.IsZero() ? this : this / length;
    }

    public Vector2D Project(Vector2D target)
    {
        var coefficient = this * target / target.GetLengthSquared();
        return target * coefficient;
    }

    public float ProjectScalar(Vector2D target) => this * target / target.GetLength();

    [Pure]
    public GameAngle GetAngle(Vector2D target)
    {
        var radianAngle = MathHelper.Atan2(target.Y, target.X) - MathHelper.Atan2(Y, X);
        var degreeAngle = radianAngle.ToDegrees();
        return GameAngle.FromDegrees(degreeAngle);
    }

    public float GetAngleCosine(Vector2D target) => this * target / (GetLength() * target.GetLength());

    //// ReSharper disable CompareOfFloatsByEqualityOperator
    public bool Equals(Vector2D other) => X == other.X && Y == other.Y;
    //// ReSharper restore CompareOfFloatsByEqualityOperator
}