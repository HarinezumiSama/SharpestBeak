using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SharpestBeak
{
    public struct ValueRange<T>
        where T : struct, IComparable<T>
    {
        #region Fields

        private readonly T _min;
        private readonly T _max;

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

            _min = min;
            _max = max;
        }

        #endregion

        #region Public Properties

        public T Min
        {
            [DebuggerStepThrough]
            get
            {
                return _min;
            }
        }

        public T Max
        {
            [DebuggerStepThrough]
            get
            {
                return _max;
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0} .. {1}]", _min, _max);
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
            return value.IsInRange(this);
        }

        #endregion
    }
}