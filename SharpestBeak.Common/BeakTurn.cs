using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpestBeak.Common
{
    // NOTE: [VM] The game engine coordinates are so that (0,0) is the bottom left corner
    // while in UI (0,0) is the top left corner. That's why Clockwise in engine means CounterClockwise in UI
    // and vice versa.

    // TODO: [VM] Fix the problem with engine and UI coordinate system conflict

    [Serializable]
    public struct BeakTurn : IEquatable<BeakTurn>, IComparable<BeakTurn>
    {
        #region Constants

        public const float MinValue = -1f;
        public const float MaxValue = 1f;

        #endregion

        #region Fields

        public static readonly ValueRange<float> ValueRange = new ValueRange<float>(MinValue, MaxValue);

        public static readonly BeakTurn FullyClockwise = new BeakTurn(MinValue);
        public static readonly BeakTurn None = new BeakTurn();
        public static readonly BeakTurn FullyCounterClockwise = new BeakTurn(MaxValue);

        private readonly float m_value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeakTurn"/> class.
        /// </summary>
        public BeakTurn(float value)
        {
            #region Argument Check

            if (!value.IsInRange(ValueRange))
            {
                throw new ArgumentOutOfRangeException(
                    "value",
                    value,
                    string.Format(
                        "The value must be in the range {0} to {1}.",
                        ValueRange.Min,
                        ValueRange.Max));
            }

            #endregion

            m_value = value;
        }

        #endregion

        #region Public Properties

        public float Value
        {
            [DebuggerStepThrough]
            get { return m_value; }
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is BeakTurn && this.Equals((BeakTurn)obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(
                "{0}. Value = {1}",
                GetType().Name,
                m_value);
        }

        #endregion

        #region IEquatable<BeakTurn> Members

        public bool Equals(BeakTurn other)
        {
            return m_value == other.m_value;
        }

        #endregion

        #region IComparable<BeakTurn> Members

        public int CompareTo(BeakTurn other)
        {
            return m_value.CompareTo(other.m_value);
        }

        #endregion
    }
}