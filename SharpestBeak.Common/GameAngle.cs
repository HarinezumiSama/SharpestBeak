using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public struct GameAngle : IEquatable<GameAngle>, IFormattable
    {
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

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public override string ToString()
        {
            return ToString(null);
        }

        #endregion

        #region IEquatable<GameAngle> Members

        public bool Equals(GameAngle other)
        {
            return (this.DegreeValue - other.DegreeValue).IsZero();
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
                sb.AppendFormat(formatProvider, "{1:N3} rad", this.RadianValue);
            }
            sb.Append("}");
            return sb.ToString();
        }

        #endregion
    }
}