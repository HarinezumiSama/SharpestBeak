using System;
using System.Collections.Generic;
using System.Drawing;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
// ReSharper disable once CheckNamespace
namespace SharpestBeak;

public static class MathHelper
{
    public const float ZeroTolerance = 1E-6f;

    public const float Pi = 3.14159265358979f;
    public const float DoublePi = 6.283185307179586f;

    public const float RevolutionRadians = DoublePi;
    public const float HalfRevolutionRadians = Pi;
    public const float QuarterRevolutionRadians = Pi / 2f;

    public const float RevolutionDegrees = 360f;
    public const float HalfRevolutionDegrees = RevolutionDegrees / 2f;
    public const float QuarterRevolutionDegrees = RevolutionDegrees / 4f;

    public static float Abs(this float value) => Math.Abs(value);

    public static float Cos(this float value) => (float)Math.Cos(value);

    public static float Sin(this float value) => (float)Math.Sin(value);

    public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

    public static float Sqr(this float value) => value * value;

    public static float Sqrt(this float value) => (float)Math.Sqrt(value);

    public static bool IsZero(this float value, float tolerance = ZeroTolerance)
    {
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be positive.");
        }

        return Math.Abs(value) < tolerance;
    }

    public static bool IsPositive(this float value, float tolerance = ZeroTolerance)
    {
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be positive.");
        }

        return value >= tolerance;
    }

    public static bool IsPositiveOrZero(this float value, float tolerance = ZeroTolerance)
    {
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be positive.");
        }

        return value > -tolerance;
    }

    public static bool IsNegative(this float value, float tolerance = ZeroTolerance)
    {
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be positive.");
        }

        return value <= -tolerance;
    }

    public static bool IsNegativeOrZero(this float value, float tolerance = ZeroTolerance)
    {
        if (tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be positive.");
        }

        return value < tolerance;
    }

    public static bool IsValidDegreeAngle(this float angle) => angle is > -HalfRevolutionDegrees and <= HalfRevolutionDegrees;

    public static float EnsureValidDegreeAngle(this float angle)
    {
        if (!IsValidDegreeAngle(angle))
        {
            throw new ArgumentOutOfRangeException(nameof(angle), angle, "Invalid angle.");
        }

        return angle;
    }

    public static float ToRadians(this float degreeAngle) => degreeAngle / HalfRevolutionDegrees * Pi;

    public static float ToDegrees(this float radianAngle) => radianAngle / Pi * HalfRevolutionDegrees;

    public static void MinMax(this IEnumerable<float> collection, out float min, out float max)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        float? minProxy = null;
        float? maxProxy = null;
        foreach (var item in collection)
        {
            if (!minProxy.HasValue || item < minProxy.Value)
            {
                minProxy = item;
            }

            if (!maxProxy.HasValue || item > maxProxy.Value)
            {
                maxProxy = item;
            }
        }

        if (!minProxy.HasValue || !maxProxy.HasValue)
        {
            throw new InvalidOperationException("No elements in the collection.");
        }

        min = minProxy.Value;
        max = maxProxy.Value;
    }

    public static bool IsInRange<T>(this T value, ValueRange<T> range)
        where T : struct, IComparable<T>
    {
        var comparer = Comparer<T>.Default;
        return comparer.Compare(value, range.Min) >= 0 && comparer.Compare(value, range.Max) <= 0;
    }

    public static bool IsInRange(this float value, ValueRange<float> range, float tolerance = ZeroTolerance)
        => (value - range.Min).IsPositiveOrZero(tolerance) && (value - range.Max).IsNegativeOrZero(tolerance);

    public static T ReduceToRange<T>(this T value, ValueRange<T> range)
        where T : struct, IComparable<T>
        => Comparer<T>.Default.Compare(value, range.Min) <= 0 ? range.Min : Comparer<T>.Default.Compare(value, range.Max) >= 0 ? range.Max : value;

    public static float ReduceToRange(this float value, ValueRange<float> range, float tolerance = ZeroTolerance)
        => (value - range.Min).IsNegativeOrZero(tolerance) ? range.Min : (value - range.Max).IsPositiveOrZero(tolerance) ? range.Max : value;

    public static Size MultiplyTruncated(this Size value, float ratio) => new((int)(value.Width * ratio), (int)(value.Height * ratio));

    public static Size Negate(this Size value) => new(-value.Width, -value.Height);
}