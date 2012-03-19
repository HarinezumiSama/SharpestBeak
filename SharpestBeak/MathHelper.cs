using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak
{
    public static class MathHelper
    {
        #region Constants

        public const float ZeroTolerance = 1E-6f;

        public const float Pi = 3.14159265358979f;
        public const float DoublePi = 6.283185307179586f;

        public const float RevolutionRadians = DoublePi;
        public const float HalfRevolutionRadians = Pi;
        public const float QuarterRevolutionRadians = Pi / 2f;

        public const float RevolutionDegrees = 360f;
        public const float HalfRevolutionDegrees = RevolutionDegrees / 2f;
        public const float QuarterRevolutionDegrees = RevolutionDegrees / 4f;

        #endregion

        #region Public Methods

        public static float Abs(this float value)
        {
            return Math.Abs(value);
        }

        public static float Cos(this float value)
        {
            return (float)Math.Cos(value);
        }

        public static float Sin(this float value)
        {
            return (float)Math.Sin(value);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        public static float Sqr(this float value)
        {
            return value * value;
        }

        public static float Sqrt(this float value)
        {
            return (float)Math.Sqrt(value);
        }

        public static bool IsZero(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return Math.Abs(value) < tolerance;
        }

        public static bool IsNotZero(this float value, float tolerance = ZeroTolerance)
        {
            return !IsZero(value, tolerance);
        }

        public static bool IsPositive(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value >= tolerance;
        }

        public static bool IsPositiveOrZero(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value > -tolerance;
        }

        public static bool IsNegative(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value <= -tolerance;
        }

        public static bool IsNegativeOrZero(this float value, float tolerance = ZeroTolerance)
        {
            #region Argument Check

            if (tolerance < 0)
            {
                throw new ArgumentOutOfRangeException("tolerance", tolerance, "Tolerance must be positive.");
            }

            #endregion

            return value < tolerance;
        }

        public static bool IsValidDegreeAngle(this float angle)
        {
            return angle > -MathHelper.HalfRevolutionDegrees && angle <= MathHelper.HalfRevolutionDegrees;
        }

        public static float EnsureValidDegreeAngle(this float angle)
        {
            if (!IsValidDegreeAngle(angle))
            {
                throw new ArgumentOutOfRangeException("angle", angle, "Invalid angle.");
            }

            return angle;
        }

        public static float ToRadians(this float degreeAngle)
        {
            return degreeAngle / HalfRevolutionDegrees * Pi;
        }

        public static float ToDegrees(this float radianAngle)
        {
            return radianAngle / Pi * HalfRevolutionDegrees;
        }

        public static void MinMax(this IEnumerable<float> collection, out float min, out float max)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            #endregion

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
        {
            return (value - range.Min).IsPositiveOrZero(tolerance)
                && (value - range.Max).IsNegativeOrZero(tolerance);
        }

        public static T ReduceToRange<T>(this T value, ValueRange<T> range)
            where T : struct, IComparable<T>
        {
            if (Comparer<T>.Default.Compare(value, range.Min) <= 0)
            {
                return range.Min;
            }
            if (Comparer<T>.Default.Compare(value, range.Max) >= 0)
            {
                return range.Max;
            }

            return value;
        }

        public static float ReduceToRange(this float value, ValueRange<float> range, float tolerance = ZeroTolerance)
        {
            if ((value - range.Min).IsNegativeOrZero(tolerance))
            {
                return range.Min;
            }
            if ((value - range.Max).IsPositiveOrZero(tolerance))
            {
                return range.Max;
            }

            return value;
        }

        #endregion
    }
}