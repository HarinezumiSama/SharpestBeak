﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Physics
{
    public struct GameAngle : IEquatable<GameAngle>, IFormattable
    {
        #region Constants and Fields

        public static readonly GameAngle Zero = new GameAngle();

        #endregion

        #region Public Properties

        public float DegreeValue
        {
            get;
            private set;
        }

        public float RadianValue
        {
            get;
            private set;
        }

        #endregion

        #region Operators

        public static bool operator ==(GameAngle left, GameAngle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameAngle left, GameAngle right)
        {
            return !left.Equals(right);
        }

        public static GameAngle operator +(GameAngle value)
        {
            return value;
        }

        public static GameAngle operator -(GameAngle value)
        {
            return FromDegrees(-value.DegreeValue);
        }

        public static GameAngle operator +(GameAngle left, GameAngle right)
        {
            return FromDegrees(left.DegreeValue + right.DegreeValue);
        }

        public static GameAngle operator -(GameAngle left, GameAngle right)
        {
            return FromDegrees(left.DegreeValue - right.DegreeValue);
        }

        public static GameAngle operator *(GameAngle left, float right)
        {
            return FromDegrees(left.DegreeValue * right);
        }

        public static GameAngle operator *(float left, GameAngle right)
        {
            return FromDegrees(left * right.DegreeValue);
        }

        #endregion

        #region Public Methods

        public static GameAngle FromRadians(float radianAngle)
        {
            var degrees = radianAngle.ToDegrees();
            return FromDegrees(degrees);
        }

        public static GameAngle FromRadians(double radianAngle)
        {
            return FromRadians((float)radianAngle);
        }

        public static GameAngle FromDegrees(float degreeAngle)
        {
            var normalizedDegreeAngle = NormalizeDegreeAngle(degreeAngle);

            return new GameAngle
            {
                DegreeValue = normalizedDegreeAngle,
                RadianValue = normalizedDegreeAngle.ToRadians()
            };
        }

        public static GameAngle FromDegrees(double degreeAngle)
        {
            return FromDegrees((float)degreeAngle);
        }

        public override bool Equals(object obj)
        {
            return obj is GameAngle && Equals((GameAngle)obj);
        }

        public override int GetHashCode()
        {
            return this.DegreeValue.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public void CosSin(out float cos, out float sin)
        {
            var rad = this.RadianValue;

            cos = rad.Cos();
            sin = rad.Sin();
        }

        public Vector2D ToUnitVector()
        {
            float cos, sin;
            CosSin(out cos, out sin);

            return new Vector2D(cos, sin);
        }

        #endregion

        #region IEquatable<GameAngle> Members

        public bool Equals(GameAngle other)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return this.DegreeValue == other.DegreeValue;
        }

        #endregion

        #region IFormattable Members

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
                sb.AppendFormat(formatProvider, "{0:N1}°", this.DegreeValue);
            }

            if (separator)
            {
                sb.Append(" : ");
            }

            if (radians)
            {
                sb.AppendFormat(formatProvider, "{0:N3} rad", this.RadianValue);
            }

            sb.Append("}");

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private static float NormalizeDegreeAngle(float value)
        {
            var result = value;

            while (result > MathHelper.HalfRevolutionDegrees && result > -MathHelper.HalfRevolutionDegrees)
            {
                result -= MathHelper.RevolutionDegrees;
            }

            while (result <= -MathHelper.HalfRevolutionDegrees && result <= MathHelper.HalfRevolutionDegrees)
            {
                result += MathHelper.RevolutionDegrees;
            }

            if (result > MathHelper.HalfRevolutionDegrees || result <= -MathHelper.HalfRevolutionDegrees)
            {
                throw new InvalidOperationException("Computed angle was not fixed correctly.");
            }

            return result;
        }

        #endregion
    }
}