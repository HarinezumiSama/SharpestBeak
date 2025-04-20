using System;
using System.Text;

namespace SharpestBeak.Physics;

public readonly struct GameAngle : IEquatable<GameAngle>, IFormattable
{
    public static readonly GameAngle Zero = new();

    private GameAngle(float degreeAngle)
    {
        var normalizedDegreeAngle = NormalizeDegreeAngle(degreeAngle);

        DegreeValue = normalizedDegreeAngle;
        RadianValue = normalizedDegreeAngle.ToRadians();
    }

    public float DegreeValue { get; }

    public float RadianValue { get; }

    public static bool operator ==(GameAngle left, GameAngle right) => left.Equals(right);

    public static bool operator !=(GameAngle left, GameAngle right) => !left.Equals(right);

    public static GameAngle operator +(GameAngle value) => value;

    public static GameAngle operator -(GameAngle value) => FromDegrees(-value.DegreeValue);

    public static GameAngle operator +(GameAngle left, GameAngle right) => FromDegrees(left.DegreeValue + right.DegreeValue);

    public static GameAngle operator -(GameAngle left, GameAngle right) => FromDegrees(left.DegreeValue - right.DegreeValue);

    public static GameAngle operator *(GameAngle left, float right) => FromDegrees(left.DegreeValue * right);

    public static GameAngle operator *(float left, GameAngle right) => FromDegrees(left * right.DegreeValue);

    public static GameAngle FromRadians(float radianAngle)
    {
        var degrees = radianAngle.ToDegrees();
        return FromDegrees(degrees);
    }

    public static GameAngle FromRadians(double radianAngle) => FromRadians((float)radianAngle);

    public static GameAngle FromDegrees(float degreeAngle) => new(degreeAngle);

    public static GameAngle FromDegrees(double degreeAngle) => FromDegrees((float)degreeAngle);

    public override bool Equals(object obj) => obj is GameAngle castObj && Equals(castObj);

    public override int GetHashCode() => DegreeValue.GetHashCode();

    public override string ToString() => ToString(null, null);

    public string ToString(string format) => ToString(format, null);

    public void CosSin(out float cos, out float sin)
    {
        cos = RadianValue.Cos();
        sin = RadianValue.Sin();
    }

    public Vector2D ToUnitVector()
    {
        CosSin(out var cos, out var sin);
        return new Vector2D(cos, sin);
    }

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public bool Equals(GameAngle other) => DegreeValue == other.DegreeValue;

    public string ToString(string format, IFormatProvider formatProvider)
    {
        var degrees = false;
        var radians = false;
        var separator = false;
        if (string.IsNullOrEmpty(format))
        {
            degrees = radians = separator = true;
        }
        else if (string.Equals(format, "D", StringComparison.OrdinalIgnoreCase))
        {
            degrees = true;
        }
        else if (string.Equals(format, "R", StringComparison.OrdinalIgnoreCase))
        {
            radians = true;
        }

        var sb = new StringBuilder();
        sb.Append("{");
        if (degrees)
        {
            sb.AppendFormat(formatProvider, "{0:N1}°", DegreeValue);
        }

        if (separator)
        {
            sb.Append(" : ");
        }

        if (radians)
        {
            sb.AppendFormat(formatProvider, "{0:N3} rad", RadianValue);
        }

        sb.Append("}");

        return sb.ToString();
    }

    private static float NormalizeDegreeAngle(float value)
    {
        var result = value;

        while (result is > MathHelper.HalfRevolutionDegrees and > -MathHelper.HalfRevolutionDegrees)
        {
            result -= MathHelper.RevolutionDegrees;
        }

        while (result is <= -MathHelper.HalfRevolutionDegrees and <= MathHelper.HalfRevolutionDegrees)
        {
            result += MathHelper.RevolutionDegrees;
        }

        if (result is > MathHelper.HalfRevolutionDegrees or <= -MathHelper.HalfRevolutionDegrees)
        {
            throw new InvalidOperationException("Computed angle was not fixed correctly.");
        }

        return result;
    }
}