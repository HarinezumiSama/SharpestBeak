using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    public abstract class ChickenUnitLogic : IDisposable
    {
        #region Fields

        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private MoveInfo m_currentMove;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            // Nothing to do
        }

        #endregion

        #region Protected Properties

        protected bool CanPlay
        {
            [DebuggerNonUserCode]
            get { return this.Board != null && this.Unit != null && !this.IsDead; }
        }

        #endregion

        #region Protected Internal Properties

        protected internal ChickenUnit Unit
        {
            get;
            internal set;
        }

        protected internal GameBoard Board
        {
            get;
            internal set;
        }

        #endregion

        #region Protected Methods

        protected abstract MoveInfo OnMakeMove();

        #endregion

        #region Public Properties

        public MoveInfo CurrentMove
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_currentMove;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
            internal set
            {
                m_lock.EnterWriteLock();
                try
                {
                    m_currentMove = value;
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }

        public bool IsDead
        {
            [DebuggerNonUserCode]
            get { return this.Unit == null || this.Unit.IsDead; }
        }

        #endregion

        #region Public Methods

        public MoveInfo MakeMove()
        {
            this.CurrentMove = this.CanPlay ? OnMakeMove() : null;
            return this.CurrentMove;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_lock != null)
            {
                m_lock.Dispose();
            }
        }

        #endregion
    }
}