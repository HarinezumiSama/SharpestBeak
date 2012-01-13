using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using SharpestBeak.Common.Elements;

namespace SharpestBeak.Common
{
    public abstract class ChickenUnitLogic : IDisposable
    {
        #region Fields

        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly ThreadSafeValue<Exception> m_error;
        private readonly ThreadSafeValue<ulong> m_moveCount;
        private readonly ThreadSafeValue<MoveInfo> m_currentMove;
        private readonly ThreadSafeValue<MoveInfo> m_previousMove;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            m_error = new ThreadSafeValue<Exception>(m_lock);
            m_moveCount = new ThreadSafeValue<ulong>(m_lock);
            m_currentMove = new ThreadSafeValue<MoveInfo>(m_lock);
            m_previousMove = new ThreadSafeValue<MoveInfo>(m_lock);
        }

        #endregion

        #region Private Properties

        private bool CanPlay
        {
            [DebuggerNonUserCode]
            get { return !this.IsDead; }
        }

        #endregion

        #region Protected Properties

        protected internal virtual bool ClearCurrentMoveWhileMaking
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        #endregion

        #region Protected Methods

        protected abstract void OnReset();

        protected abstract void OnInitialize();

        protected abstract MoveInfo OnMakeMove(GameState state);

        #endregion

        #region Internal Properties

        internal Exception Error
        {
            [DebuggerNonUserCode]
            get { return m_error.Value; }
            [DebuggerNonUserCode]
            set { m_error.Value = value; }
        }

        #endregion

        #region Internal Methods

        internal void Reset()
        {
            this.Unit.IsDead = false;
            this.MoveCount = 0;
            this.Error = null;

            OnReset();
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        internal void MakeMove(GameState state)
        {
            this.CurrentMove = this.CanPlay && state != null ? OnMakeMove(state) : null;
        }

        #endregion

        #region Protected Internal Properties

        protected internal ChickenUnit Unit
        {
            get;
            internal set;
        }

        #endregion

        #region Public Properties

        public MoveInfo CurrentMove
        {
            [DebuggerNonUserCode]
            get { return m_currentMove.Value; }
            [DebuggerNonUserCode]
            protected internal set { m_currentMove.Value = value; }
        }

        public MoveInfo PreviousMove
        {
            [DebuggerNonUserCode]
            get { return m_previousMove.Value; }
            [DebuggerNonUserCode]
            protected internal set { m_previousMove.Value = value; }
        }

        public bool IsDead
        {
            [DebuggerNonUserCode]
            get { return this.Unit == null || this.Unit.IsDead; }
        }

        public ulong MoveCount
        {
            [DebuggerNonUserCode]
            get { return m_moveCount.Value; }
            [DebuggerNonUserCode]
            internal set { m_moveCount.Value = value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_lock.DisposeSafely();
            m_error.DisposeSafely();
            m_moveCount.DisposeSafely();
            m_currentMove.DisposeSafely();
        }

        #endregion
    }
}