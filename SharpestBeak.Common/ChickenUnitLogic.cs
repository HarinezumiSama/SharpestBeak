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
        private readonly List<ChickenUnit> m_unitsDirect = new List<ChickenUnit>();

        private readonly ThreadSafeValue<Exception> m_error;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            m_error = new ThreadSafeValue<Exception>(m_lock);

            this.Units = m_unitsDirect.AsReadOnly();
        }

        #endregion

        #region Private Methods

        private GameState GetGameState()
        {
            var unitStates = this.Units.Select(item => new ChickenUnitState(item)).ToArray();
            var result = new GameState(this.Engine, unitStates);
            return result;
        }

        #endregion

        #region Protected Methods

        protected abstract void OnReset(GameState gameState);

        protected abstract void OnMakeMove(GameState gameState);

        #endregion

        #region Internal Properties

        internal GameEngine Engine
        {
            get;
            private set;
        }

        internal Thread Thread
        {
            get;
            set;
        }

        internal Exception Error
        {
            [DebuggerNonUserCode]
            get
            {
                return m_error.Value;
            }
            [DebuggerNonUserCode]
            set
            {
                m_error.Value = value;
            }
        }

        internal IList<ChickenUnit> Units
        {
            get;
            private set;
        }

        #endregion

        #region Internal Methods

        internal void InitializeInstance(GameEngine engine, int unitCount, GameTeam team)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }
            if (unitCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "unitCount",
                    unitCount,
                    "The number of units must be positive.");
            }
            if (team == GameTeam.None)
            {
                throw new ArgumentException("The team must be specific.", "team");
            }

            #endregion

            this.Engine = engine;
            this.Team = team;

            m_unitsDirect.ChangeContents(
                Enumerable.Range(1, unitCount).Select(i => new ChickenUnit(this)));
        }

        internal void Reset()
        {
            this.Error = null;

            var gameState = GetGameState();
            OnReset(gameState);
        }

        internal void MakeMove(GameState gameState)
        {
            if (gameState != null)
            {
                OnMakeMove(gameState);
            }
        }

        #endregion

        #region Public Properties

        public GameTeam Team
        {
            get;
            private set;
        }

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