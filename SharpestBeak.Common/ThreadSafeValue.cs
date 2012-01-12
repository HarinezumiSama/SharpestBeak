using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    public sealed class ThreadSafeValue<T> : IDisposable
    {
        #region Constants

        public const LockRecursionPolicy DefaultRecursionPolicy = LockRecursionPolicy.NoRecursion;

        #endregion

        #region Fields

        private readonly bool m_ownLock;
        private T m_value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        private ThreadSafeValue(ReaderWriterLockSlim @lock, bool ownLock, T value = default(T))
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }

            #endregion

            this.Lock = @lock;
            m_ownLock = ownLock;
            m_value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSafeValue(ReaderWriterLockSlim @lock, T value = default(T))
            : this(@lock, false, value)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }

            #endregion

            this.Lock = @lock;
            m_value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSafeValue(
            LockRecursionPolicy recursionPolicy = DefaultRecursionPolicy,
            T value = default(T))
            : this(new ReaderWriterLockSlim(recursionPolicy), true, value)
        {
            // Nothing to do
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeValue&lt;T&gt;"/> class.
        /// </summary>
        public ThreadSafeValue(T value)
            : this(DefaultRecursionPolicy, value)
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public ReaderWriterLockSlim Lock
        {
            get;
            private set;
        }

        public T Value
        {
            [DebuggerNonUserCode]
            get
            {
                this.Lock.EnterReadLock();
                try
                {
                    return m_value;
                }
                finally
                {
                    this.Lock.ExitReadLock();
                }
            }
            [DebuggerNonUserCode]
            set
            {
                this.Lock.EnterWriteLock();
                try
                {
                    m_value = value;
                }
                finally
                {
                    this.Lock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var value = this.Value;
            return value == null ? string.Empty : value.ToString();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Lock.DisposeSafely();
            this.Lock = null;
        }

        #endregion
    }
}