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
        private readonly List<ChickenUnit> m_unitsDirect = new List<ChickenUnit>();

        private readonly ThreadSafeValue<Exception> m_error;
        //private readonly ThreadSafeValue<ulong> m_moveCount;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            m_error = new ThreadSafeValue<Exception>(m_lock);
            //m_moveCount = new ThreadSafeValue<ulong>(m_lock);

            this.Units = m_unitsDirect.AsReadOnly();
        }

        #endregion

        #region Protected Methods

        protected abstract void OnReset();

        protected abstract void OnInitialize();

        protected abstract void OnMakeMove(GameState state);

        #endregion

        #region Internal Properties

        internal Exception Error
        {
            [DebuggerNonUserCode]
            get { return m_error.Value; }
            [DebuggerNonUserCode]
            set { m_error.Value = value; }
        }

        internal IList<ChickenUnit> Units
        {
            get;
            private set;
        }

        #endregion

        #region Internal Methods

        internal void CreateUnits(int unitCount)
        {
            #region Argument Check

            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "unitCount",
                    unitCount,
                    "The number of units must be positive.");
            }

            #endregion

            m_unitsDirect.ChangeContents(
                Enumerable.Range(1, unitCount).Select(i => new ChickenUnit(this)));
        }

        internal void Reset()
        {
            m_unitsDirect.DoForEach(item => item.IsDead = false);

            //this.MoveCount = 0;
            this.Error = null;

            OnReset();
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        internal void MakeMove(GameState state)
        {
            if (state != null)
            {
                OnMakeMove(state);
            }
        }

        #endregion

        #region Protected Internal Properties

        protected internal virtual bool ClearCurrentMoveWhileMaking
        {
            [DebuggerStepThrough]
            get { return false; }
        }

        #endregion

        #region Public Properties

        //public MoveInfo PreviousMove
        //{
        //    [DebuggerNonUserCode]
        //    get { return m_previousMove.Value; }
        //    [DebuggerNonUserCode]
        //    protected internal set { m_previousMove.Value = value; }
        //}

        //public ulong MoveCount
        //{
        //    [DebuggerNonUserCode]
        //    get { return m_moveCount.Value; }
        //    [DebuggerNonUserCode]
        //    internal set { m_moveCount.Value = value; }
        //}

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            m_lock.DisposeSafely();
            m_error.DisposeSafely();
            //m_moveCount.DisposeSafely();
            //m_currentMove.DisposeSafely();
        }

        #endregion
    }
}