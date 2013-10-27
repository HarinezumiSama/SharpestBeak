using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SharpestBeak.Model
{
    public abstract class ChickenUnitLogic : IDisposable
    {
        #region Constants and Fields

        private readonly List<ChickenUnit> _unitsDirect = new List<ChickenUnit>();
        private readonly ThreadSafeValue<Exception> _error;
        private readonly Lazy<string> _caption;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            _error = new ThreadSafeValue<Exception>();
            this.MakeMoveEvent = new AutoResetEvent(false);

            this.Units = _unitsDirect.AsReadOnly();
            _caption = new Lazy<string>(this.GetCaption);

            this.UnitsStates = new Dictionary<ChickenUnit, ChickenUnitState>(_unitsDirect.Count);
            this.UnitsStatesLock = new object();

            this.UnitsMoves = new Dictionary<ChickenUnit, MoveInfo>(_unitsDirect.Count);
            this.UnitsMovesLock = new object();
        }

        #endregion

        #region Public Properties

        public GameTeam Team
        {
            get;
            private set;
        }

        public string Caption
        {
            [DebuggerNonUserCode]
            get
            {
                return _caption.Value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.MakeMoveEvent.DisposeSafely();
        }

        #endregion

        #region Protected Methods

        protected abstract void OnReset(GameState gameState);

        protected abstract void OnMakeMove(GameState gameState, LogicMoveResult moves);

        protected virtual string GetCaption()
        {
            return GetType().Name;
        }

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

        internal AutoResetEvent MakeMoveEvent
        {
            get;
            private set;
        }

        internal Exception Error
        {
            [DebuggerNonUserCode]
            get
            {
                return _error.Value;
            }

            [DebuggerNonUserCode]
            set
            {
                _error.Value = value;
            }
        }

        internal IList<ChickenUnit> Units
        {
            get;
            private set;
        }

        internal Dictionary<ChickenUnit, ChickenUnitState> UnitsStates
        {
            get;
            private set;
        }

        internal object UnitsStatesLock
        {
            get;
            private set;
        }

        internal Dictionary<ChickenUnit, MoveInfo> UnitsMoves
        {
            get;
            private set;
        }

        internal object UnitsMovesLock
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

            _unitsDirect.ChangeContents(
                Enumerable.Range(1, unitCount).Select(i => new ChickenUnit(this)));
        }

        internal void Reset()
        {
            this.Error = null;

            var gameState = GetGameState();
            OnReset(gameState);
        }

        internal LogicMoveResult MakeMove()
        {
            var gameState = GetGameState();
            var result = new LogicMoveResult(gameState.UnitStates.Count);
            OnMakeMove(gameState, result);
            return result;
        }

        #endregion

        #region Private Methods

        private GameState GetGameState()
        {
            GameState result;
            lock (this.UnitsStatesLock)
            {
                result = new GameState(this.Engine, this.UnitsStates.Values);
            }
            return result;
        }

        #endregion
    }
}