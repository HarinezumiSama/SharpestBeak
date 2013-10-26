using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpestBeak
{
    public sealed class ThreadSafeValue<T>
    {
        #region Fields

        private T _value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSafeValue(object @lock, T value = default(T))
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }

            #endregion

            this.Lock = @lock;
            _value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSafeValue(T value = default(T))
            : this(new object(), value)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public object Lock
        {
            get;
            private set;
        }

        public T Value
        {
            [DebuggerNonUserCode]
            get
            {
                lock (this.Lock)
                {
                    return _value;
                }
            }

            [DebuggerNonUserCode]
            set
            {
                lock (this.Lock)
                {
                    _value = value;
                }
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var value = this.Value;
            return ReferenceEquals(value, null) ? string.Empty : value.ToString();
        }

        #endregion
    }
}