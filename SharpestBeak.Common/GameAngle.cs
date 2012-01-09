﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public struct GameAngle : IEquatable<GameAngle>, IFormattable
    {
        #region Internal Methods

        internal static float NormalizeDegreeAngle(float value)
        {
            var result = value;
            while (result > GameHelper.HalfRevolutionDegrees && result > -GameHelper.HalfRevolutionDegrees)
            {
                result -= GameHelper.RevolutionDegrees;
            }
            while (result <= -GameHelper.HalfRevolutionDegrees && result <= GameHelper.HalfRevolutionDegrees)
            {
                result += GameHelper.RevolutionDegrees;
            }

            if (result > GameHelper.HalfRevolutionDegrees || result <= -GameHelper.HalfRevolutionDegrees)
            {
                throw new InvalidOperationException("Computed angle was not fixed correctly.");
            }
            return result;
        }

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

        #region Public Methods

        public static GameAngle FromRadians(float radianAngle)
        {
            return new GameAngle
            {
                DegreeValue = radianAngle.ToDegrees().EnsureValidDegreeAngle(),
                RadianValue = radianAngle
            };
        }

        public static GameAngle FromRadians(double radianAngle)
        {
            return FromRadians((float)radianAngle);
        }

        public static GameAngle FromDegrees(float degreeAngle)
        {
            return new GameAngle
            {
                DegreeValue = degreeAngle.EnsureValidDegreeAngle(),
                RadianValue = degreeAngle.ToRadians()
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

        public string ToString(string format)
        {
            return ToString(format, (IFormatProvider)null);
        }

        public override string ToString()
        {
            return ToString((string)null, (IFormatProvider)null);
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
            return GameAngle.FromDegrees(NormalizeDegreeAngle(-value.DegreeValue));
        }

        public static GameAngle operator +(GameAngle left, GameAngle right)
        {
            return GameAngle.FromDegrees(NormalizeDegreeAngle(left.DegreeValue + right.DegreeValue));
        }

        public static GameAngle operator -(GameAngle left, GameAngle right)
        {
            return GameAngle.FromDegrees(NormalizeDegreeAngle(left.DegreeValue - right.DegreeValue));
        }

        public static GameAngle operator *(GameAngle left, float right)
        {
            return GameAngle.FromDegrees(NormalizeDegreeAngle(left.DegreeValue * right));
        }

        public static GameAngle operator *(float left, GameAngle right)
        {
            return GameAngle.FromDegrees(NormalizeDegreeAngle(left * right.DegreeValue));
        }

        #endregion

        #region IEquatable<GameAngle> Members

        public bool Equals(GameAngle other)
        {
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
    }
}