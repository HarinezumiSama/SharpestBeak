using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public struct ValueRange<T>
        where T : struct, IComparable<T>
    {
        #region Fields

        private readonly T m_min;
        private readonly T m_max;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueRange&lt;T&gt;"/> structure.
        /// </summary>
        public ValueRange(T min, T max)
        {
            #region Argument Check

            if (Comparer<T>.Default.Compare(min, max) > 0)
            {
                throw new ArgumentException("The minimum is greater than maximum.");
            }

            #endregion

            m_min = min;
            m_max = max;
        }

        #endregion

        #region Public Properties

        public T Min
        {
            [DebuggerStepThrough]
            get
            {
                return m_min;
            }
        }

        public T Max
        {
            [DebuggerStepThrough]
            get
            {
                return m_max;
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0} .. {1}]", m_min, m_max);
        }

        /// <summary>
        ///     Determines whether the specified value belongs to this range.
        /// </summary>
        /// <param name="value">
        ///     The value to check.
        /// </param>
        /// <returns>
        ///     <b>true</b> if the specified value belongs to this range; otherwise, <b>false</b>.
        /// </returns>
        public bool Belongs(T value)
        {
            return MathHelper.IsInRange(value, this);
        }

        #endregion
    }
}