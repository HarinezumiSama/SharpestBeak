using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using SharpestBeak.Diagnostics;

namespace SharpestBeak.Model
{
    internal sealed class ChickenUnitLogicExecutor : IDisposable
    {
        #region Constants and Fields

        private readonly ManualResetEventSlim _makeMoveEvent;
        private readonly ThreadSafeValue<Exception> _error;
        private Thread _thread;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogicExecutor"/> class.
        /// </summary>
        public ChickenUnitLogicExecutor(
            GameEngine engine,
            int unitCount,
            GameTeam team,
            ManualResetEventSlim makeMoveEvent,
            ChickenUnitLogic logic)
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
                    @"The value must be positive.");
            }

            if (team == GameTeam.None)
            {
                throw new ArgumentException("The team must be specific.", "team");
            }

            if (makeMoveEvent == null)
            {
                throw new ArgumentNullException("makeMoveEvent");
            }

            if (logic == null)
            {
                throw new ArgumentNullException("logic");
            }

            #endregion

            this.Engine = engine;
            this.Team = team;
            _makeMoveEvent = makeMoveEvent;
            this.Logic = logic;

            var unitsDirect = Enumerable.Range(1, unitCount).Select(i => new ChickenUnit(this)).ToList();

            _error = new ThreadSafeValue<Exception>();

            this.Units = unitsDirect.AsReadOnly();

            this.UnitsStates = new Dictionary<ChickenUnit, ChickenUnitState>(unitsDirect.Count);
            this.UnitsStatesLock = new object();

            this.UnitsMoves = new Dictionary<ChickenUnit, MoveInfo>(unitsDirect.Count);
            this.UnitsMovesLock = new object();
        }

        #endregion

        #region Public Properties

        public GameEngine Engine
        {
            get;
            private set;
        }

        public GameTeam Team
        {
            get;
            private set;
        }

        public ChickenUnitLogic Logic
        {
            get;
            private set;
        }

        public Exception Error
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

        public ReadOnlyCollection<ChickenUnit> Units
        {
            get;
            private set;
        }

        public Dictionary<ChickenUnit, ChickenUnitState> UnitsStates
        {
            get;
            private set;
        }

        public object UnitsStatesLock
        {
            get;
            private set;
        }

        public Dictionary<ChickenUnit, MoveInfo> UnitsMoves
        {
            get;
            private set;
        }

        public object UnitsMovesLock
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            if (_thread != null)
            {
                throw new InvalidOperationException("Already started.");
            }

            _thread = new Thread(this.DoExecuteLogic)
            {
                IsBackground = true,
                Name =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}: {1} [{2}]",
                        GetType().Name,
                        this.Team,
                        this.Logic.GetType().Name)
            };

            _thread.Start();
        }

        public void Stop()
        {
            if (_thread == null)
            {
                return;
            }

            _thread.Abort();
            _thread.Join();

            _thread = null;
        }

        public void Reset()
        {
            this.Error = null;

            var gameState = GetGameState();
            this.Logic.Reset(gameState);
        }

        public LogicMoveResult MakeMove()
        {
            var gameState = GetGameState();
            var result = new LogicMoveResult(gameState.UnitStates.Count);
            this.Logic.MakeMove(gameState, result);
            return result;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region Private Methods

        private GameState GetGameState()
        {
            GameState result;
            lock (this.UnitsStatesLock)
            {
                result = new GameState(this.Engine, this.Team, this.UnitsStates.Values);
            }

            return result;
        }

        private void DoExecuteLogic()
        {
            while (true)
            {
                if (SettingsCache.Instance.InstrumentationMode
                    && this.Engine.MoveCount >= GameEngine.InstrumentationMoveCountLimit)
                {
                    return;
                }

                try
                {
                    while (!_makeMoveEvent.Wait(0))
                    {
                        Thread.Sleep(0);
                    }

                    var moveResult = MakeMove();

                    lock (this.UnitsMovesLock)
                    {
                        this.UnitsMoves.Clear();
                        foreach (var movePair in moveResult.InnerMap)
                        {
                            DebugHelper.WriteLine(
                                "[Logic '{0}'] Chicken {{{1}}} is making move {{{2}}}.",
                                this.Logic.GetType().Name,
                                movePair.Key,
                                movePair.Value == null ? "NONE" : movePair.Value.ToString());

                            this.UnitsMoves.Add(movePair.Key, movePair.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }

                    this.Error = ex;

                    this.Units.DoForEach(
                        item =>
                        {
                            item.IsDead = true;
                            DebugHelper.WriteLine(
                                "Chicken #{0} is now dead since the logic '{1}' has caused an error:{2}{3}",
                                item.UniqueId,
                                this.Logic.GetType().FullName,
                                Environment.NewLine,
                                ex.ToString());
                        });

                    return;
                }
            }
        }

        #endregion
    }
}