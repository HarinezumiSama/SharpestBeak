using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharpestBeak.Common.Diagnostics;
using SharpestBeak.Common.Elements;
using SharpestBeak.Common.Properties;

namespace SharpestBeak.Common
{
    public sealed class GameEngine : IDisposable
    {
        #region Fields

        private static readonly Random s_random = new Random();
        private static readonly TimeSpan s_stopTimeout = TimeSpan.FromSeconds(5d);

        private readonly ReaderWriterLockSlim m_syncLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ManualResetEvent m_stopEvent = new ManualResetEvent(false);
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
        private readonly ThreadSafeValue<ulong> m_moveCount;
        private Thread m_engineThread;
        private bool m_finalizingStage;

        private readonly object m_lastMovesLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(
            Action<GamePaintEventArgs> paintCallback,
            Size size,
            ChickenLogicRecord teamA,
            ChickenLogicRecord teamB)
        {
            #region Fields

            if (paintCallback == null)
            {
                throw new ArgumentNullException("paintCallback");
            }
            if (teamA == null)
            {
                throw new ArgumentNullException("teamA");
            }
            if (teamB == null)
            {
                throw new ArgumentNullException("teamB");
            }

            #endregion

            m_paintCallback = paintCallback;

            if (Settings.Default.UsePerformanceCounters)
            {
                PerformanceCounterHelper.Initialize();
            }

            // Pre-initialized properties
            this.CommonData = new GameCommonData(size);
            m_moveCount = new ThreadSafeValue<ulong>(m_syncLock);
            this.LastMoves = new Dictionary<ChickenUnit, ChickenUnitState>();

            // Post-initialized properties
            this.Logics =
                new[]
                {
                    CreateLogic(teamA, GameTeam.TeamA),
                    CreateLogic(teamB, GameTeam.TeamB)
                }
                .ToList()
                .AsReadOnly();
            this.AllChickens = this.Logics
                .SelectMany(item => item.Units)
                .ToList()
                .AsReadOnly();
            this.AllChickens.DoForEach((item, index) => item.UniqueIndex = index + 1);
            this.AliveChickensDirect = new List<ChickenUnit>();
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();
            this.ShotUnitsDirect = new List<ShotUnit>();
            this.ShotUnits = this.ShotUnitsDirect.AsReadOnly();

            #region Argument Check

            if (this.AllChickens.Count > this.CommonData.NominalSize.Width * this.CommonData.NominalSize.Height / 2)
            {
                throw new ArgumentException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}.",
                        this.AllChickens.Count,
                        this.CommonData.NominalSize.Width,
                        this.CommonData.NominalSize.Height),
                    "size");
            }

            #endregion

            ResetInternal();
        }

        #endregion

        #region Private Methods

        private static ChickenUnitLogic CreateLogic(ChickenLogicRecord logicRecord, GameTeam team)
        {
            #region Argument Check

            if (logicRecord == null)
            {
                throw new ArgumentNullException("logicRecord");
            }

            #endregion

            var result = (ChickenUnitLogic)Activator.CreateInstance(logicRecord.Type);
            result.InitializeInstance(logicRecord.UnitCount, team);
            return result;
        }

        private void EnsureNotDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void PositionChickens()
        {
            for (int index = 0; index < this.AllChickens.Count; index++)
            {
                var chicken = this.AllChickens[index];

                Point2D newPosition;
                do
                {
                    var discretePosition = new Point(
                        s_random.Next(this.CommonData.NominalSize.Width),
                        s_random.Next(this.CommonData.NominalSize.Height));
                    newPosition = new Point2D(
                        GameConstants.NominalCellSize * discretePosition.X + GameConstants.NominalCellSize / 2,
                        GameConstants.NominalCellSize * discretePosition.Y + GameConstants.NominalCellSize / 2);
                }
                while (this.AllChickens.Take(index).Any(
                    item => item.Position.GetDistance(newPosition) < GameConstants.NominalCellSize));

                var newAngle = (float)Math.Floor(
                    GameHelper.HalfRevolutionDegrees - s_random.NextDouble() * GameConstants.FullRevolutionAngle);

                chicken.Position = newPosition;
                chicken.BeakAngle = GameAngle.FromDegrees(GameAngle.NormalizeDegreeAngle(newAngle));
            }
        }

        private void CallPaintCallback()
        {
            m_paintCallback(new GamePaintEventArgs(GetPresentation()));
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            CallPaintCallback();
        }

        private bool IsStopping()
        {
            return m_stopEvent.WaitOne(0);
        }

        private void StartInternal()
        {
            EnsureNotDisposed();

            if (m_engineThread != null)
            {
                throw new InvalidOperationException("Engine is already running.");
            }

            Application.Idle += this.Application_Idle;

            m_engineThread = new Thread(this.DoExecuteEngine)
            {
                Name = GetType().FullName,
                IsBackground = true
            };
            this.Logics
                .DoForEach(
                item =>
                {
                    item.Thread = new Thread(this.DoExecuteLogic)
                    {
                        IsBackground = true,
                        Name = string.Format("Logic '{0}'", item.GetType().FullName)
                    };
                    item.Initialize();
                });
            this.ShotUnitsDirect.Clear();

            m_stopEvent.Reset();
            CallPaintCallback();
            m_engineThread.Start();

            this.Logics.DoForEach(item => item.Thread.Start(item));
        }

        private void ResetInternal()
        {
            m_moveCount.Value = 0;
            m_finalizingStage = false;

            this.AliveChickensDirect.ChangeContents(this.AllChickens);
            this.ShotUnitsDirect.Clear();

            PositionChickens();

            this.AllChickens.DoForEach(item => item.Logic.Reset());
        }

        private void DoExecuteEngine()
        {
            m_syncLock.ExecuteInWriteLock(
                () =>
                {
                    this.AliveChickens
                        .Select(item => item.Logic)
                        .DoForEach(item => item.Error = null);
                });

            var sw = new Stopwatch();
            List<ChickenUnitState> previousUnitStates = null;
            while (!IsStopping())
            {
                if (m_disposed)
                {
                    return;
                }

                if (sw.IsRunning)
                {
                    while (sw.Elapsed < GameConstants.LogicPollFrequency)
                    {
                        if (IsStopping())
                        {
                            return;
                        }
                        Thread.Yield();
                    }
                }
                sw.Restart();

                m_syncLock.ExecuteInWriteLock(
                    () =>
                    {
                        using (new AutoStopwatch(s => DebugHelper.WriteLine(s))
                            {
                                OutputFormat = string.Format("Engine step #{0} took {{0}}.", this.MoveCount + 1)
                            })
                        {
                            ProcessEngineStep(ref previousUnitStates);
                        }
                    });

                if (IsStopping())
                {
                    return;
                }

                this.MoveCount++;
                if (Settings.Default.UsePerformanceCounters)
                {
                    PerformanceCounterHelper.Instance.CollisionCountPerStepBase.Increment();
                }
            }
        }

        private void ProcessEngineStep(ref List<ChickenUnitState> previousUnitStates)
        {
            var timeDelta = (float)GameConstants.LogicPollFrequency.TotalSeconds;

            var aliveChickens = this.AliveChickens
                .Where(item => item.Logic.Error == null)
                .ToArray();

            List<ChickenUnitState> newUnitStates;
            lock (m_lastMovesLock)
            {
                newUnitStates = aliveChickens
                    .Select(item => this.LastMoves.GetValueOrDefault(item))
                    .Where(item => item != null && item.CurrentMove != null
                        && item.CurrentMove.State == MoveInfoState.None)
                    .ToList();
            }

            var oldShotUnits = this.ShotUnitsDirect.ToArray();

            // TODO: [VM] Allow to shoot if there are any enemy shots on game board
            if (!m_finalizingStage)
            {
                // Processing new shot units
                var shootingMoves = newUnitStates.Where(item => item.CurrentMove.FireMode != FireMode.None).ToArray();
                shootingMoves.DoForEach(
                    item =>
                    {
                        // Is there any active shot unit from the same chicken unit?
                        if (this.ShotUnitsDirect.Any(s => s.Owner == item.Unit))
                        {
                            if (item.Unit.ShotTimer.Elapsed < GameConstants.ShotUnit.MaximumFrequency)
                            {
                                DebugHelper.WriteLine("New shot from {{{0}}} has been skipped.", item.Unit);
                                return;
                            }
                        }

                        var shot = new ShotUnit(item.Unit);
                        this.ShotUnitsDirect.Add(shot);
                        item.Unit.ShotTimer.Restart();

                        DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, item.Unit);
                    });
            }

            #region Processing Shot Collisions

            var explodedShotUnits = new List<ShotUnit>();
            oldShotUnits.DoForEach(
                item =>
                {
                    item.Position = GameHelper.GetNewPosition(
                        item.Position,
                        item.Angle,
                        MoveDirection.MoveForward,
                        GameConstants.ShotUnit.DefaultSpeed,
                        timeDelta);
                    DebugHelper.WriteLine("Shot {{{0}}} has moved.", item);

                    if (item.Position.X < -GameConstants.ShotUnit.Radius
                        || item.Position.X > this.CommonData.RealSize.Width
                            + GameConstants.ShotUnit.Radius
                        || item.Position.Y < -GameConstants.ShotUnit.Radius
                        || item.Position.Y > this.CommonData.RealSize.Height
                            + GameConstants.ShotUnit.Radius)
                    {
                        item.Exploded = true;
                        explodedShotUnits.Add(item);

                        DebugHelper.WriteLine("Shot {{{0}}} has exploded outside of game board.", item);
                    }
                });

            using (new AutoStopwatch(s => DebugHelper.WriteLine(s))
                {
                    OutputFormat = "Shot/chicken collision took {0}."
                })
            {
                // TODO: [VM] Process shot-to-shot collisions

                foreach (var shotUnit in oldShotUnits)
                {
                    var shotElement = shotUnit.GetElement();

                    var injuredChicken = aliveChickens
                        .SingleOrDefault(
                            item => !item.IsDead
                                && CollisionDetector.CheckCollision(shotElement, item.GetElement()));
                    if (injuredChicken != null)
                    {
                        shotUnit.Exploded = true;
                        explodedShotUnits.Add(shotUnit);

                        injuredChicken.IsDead = true;
                        injuredChicken.KilledBy = shotUnit.Owner;
                        var suicide = shotUnit.Owner == injuredChicken;
                        if (!suicide)
                        {
                            shotUnit.Owner.KillCount++;
                        }

                        DebugHelper.WriteLine(
                            "Shot {{{0}}} has exploded and killed {{{1}}}{2}.",
                            shotUnit,
                            injuredChicken,
                            suicide ? " [suicide]" : string.Empty);
                    }
                }
            }

            this.ShotUnitsDirect.RemoveAll(item => explodedShotUnits.Contains(item));

            #endregion

            if (previousUnitStates != null && previousUnitStates.Any() && timeDelta > 0f)
            {
                foreach (var unitState in previousUnitStates)
                {
                    if (IsStopping())
                    {
                        return;
                    }

                    var unit = unitState.Unit;
                    DebugHelper.WriteLine(
                        "{0} is processing move {{{1}}} of chicken {{{2}}}.",
                        GetType().Name,
                        unitState.CurrentMove,
                        unit);

                    var newPosition = GameHelper.GetNewPosition(
                        unit.Position,
                        unit.BeakAngle,
                        unitState.CurrentMove.MoveDirection,
                        GameConstants.ChickenUnit.DefaultRectilinearSpeed,
                        timeDelta);
                    var newBeakAngle = GameHelper.GetNewBeakAngle(
                        unit.BeakAngle,
                        unitState.CurrentMove.BeakTurn,
                        timeDelta);

                    // TODO: [VM] Check out-of-game-board collision

                    var newPositionElement = new ChickenElement(newPosition, newBeakAngle);
                    var otherChickens = aliveChickens.Where(item => item != unit).ToArray();
                    var conflictingChicken = otherChickens.FirstOrDefault(
                        item => CollisionDetector.CheckCollision(newPositionElement, item.GetElement()));
                    if (conflictingChicken != null)
                    {
                        unitState.CurrentMove.State = MoveInfoState.Rejected;
                        DebugHelper.WriteLine(
                            "Blocked collision of chicken {{{0}}} with {{{1}}}.",
                            unit,
                            conflictingChicken);
                    }
                    else
                    {
                        unit.Position = newPosition;
                        unit.BeakAngle = newBeakAngle;
                        unitState.CurrentMove.State = MoveInfoState.Handled;

                        DebugHelper.WriteLine("Chicken {{{0}}} has moved.", unit);
                    }
                }
            }

            this.AliveChickensDirect.RemoveAll(item => item.IsDead);

            var aliveTeams = this.AliveChickensDirect.Select(item => item.Logic.Team).Distinct().ToList();
            if (aliveTeams.Count <= 1)
            {
                m_finalizingStage = true;
                if (!this.ShotUnitsDirect.Any())
                {
                    var winningTeam = aliveTeams.SingleOrDefault();
                    RaiseGameEnded(winningTeam);
                    return;
                }
            }

            previousUnitStates = newUnitStates;
        }

        private void DoExecuteLogic(object logicInstance)
        {
            var logic = logicInstance as ChickenUnitLogic;
            if (logic == null)
            {
                throw new InvalidOperationException("Invalid logic passed to thread method.");
            }

            while (!IsStopping())
            {
                if (m_disposed)
                {
                    return;
                }

                try
                {
                    GameState gameState;
                    lock (m_lastMovesLock)
                    {
                        if (logic.Units.All(item => item.IsDead))
                        {
                            DebugHelper.WriteLine(
                                "Exiting thread '{0}' since logic '{1}' has no alive units.",
                                Thread.CurrentThread.Name,
                                logic.GetType().FullName);
                            return;
                        }

                        var unitsToHandle = logic
                            .Units
                            .Select(
                                item => new
                                {
                                    Unit = item,
                                    State = this.LastMoves.GetValueOrDefault(item)
                                })
                            .Where(
                                info =>
                                {
                                    return info.State == null
                                        || info.State.CurrentMove == null
                                        || info.State.CurrentMove.State != MoveInfoState.None;
                                })
                                .Select(
                                    info => info.State == null
                                        ? new ChickenUnitState(info.Unit)
                                        : new ChickenUnitState(info.State))
                            .ToArray();
                        if (!unitsToHandle.Any())
                        {
                            Thread.Yield();
                            continue;
                        }

                        gameState = new GameState(this, unitsToHandle);
                    }

                    logic.MakeMove(gameState);

                    lock (m_lastMovesLock)
                    {
                        gameState.UnitStates.DoForEach(
                            item =>
                            {
                                DebugHelper.WriteLine(
                                    "Logic '{0}', for chicken {{{1}}}, is making move {{{2}}}.",
                                    logic.GetType().Name,
                                    item.Unit,
                                    item.CurrentMove == null ? "NONE" : item.CurrentMove.ToString());
                                this.LastMoves[item.Unit] = item;
                            });
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsThreadAbort())
                    {
                        throw;
                    }

                    logic.Error = ex;
                    logic.Units.DoForEach(
                        item =>
                        {
                            item.IsDead = true;
                            DebugHelper.WriteLine(
                                "Chicken #{0} is now dead since logic '{1}' caused an error:{2}{3}",
                                item.UniqueIndex,
                                logic.GetType().FullName,
                                Environment.NewLine,
                                ex.ToString());
                        });

                    return;
                }
                //logic.MoveCount++;

                Thread.Yield();
            }
        }

        private GamePresentation GetPresentation()
        {
            return m_syncLock.ExecuteInReadLock(() => new GamePresentation(this));
        }

        private void OnGameEnded(GameEndedEventArgs e)
        {
            var gameEnded = this.GameEnded;
            if (gameEnded != null)
            {
                gameEnded(this, e);
            }
        }

        private void RaiseGameEnded(GameTeam winningTeam)
        {
            var e = new GameEndedEventArgs(winningTeam);
            OnGameEnded(e);
        }

        #endregion

        #region Internal Properties

        internal List<ChickenUnit> AliveChickensDirect
        {
            get;
            private set;
        }

        internal IList<ChickenUnit> AllChickens
        {
            get;
            private set;
        }

        internal List<ShotUnit> ShotUnitsDirect
        {
            get;
            private set;
        }

        internal Dictionary<ChickenUnit, ChickenUnitState> LastMoves
        {
            get;
            private set;
        }

        internal IList<ChickenUnitLogic> Logics
        {
            get;
            private set;
        }

        internal IList<ChickenUnit> AliveChickens
        {
            get;
            private set;
        }

        internal IList<ShotUnit> ShotUnits
        {
            get;
            private set;
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when the game has ended. <b>NOTE</b>: handlers of this event are called from engine thread!
        /// </summary>
        public event EventHandler<GameEndedEventArgs> GameEnded;

        #endregion

        #region Public Properties

        public GameCommonData CommonData
        {
            get;
            private set;
        }

        public ulong MoveCount
        {
            [DebuggerNonUserCode]
            get
            {
                return m_moveCount.Value;
            }
            [DebuggerNonUserCode]
            private set
            {
                m_moveCount.Value = value;
            }
        }

        public bool IsRunning
        {
            [DebuggerNonUserCode]
            get
            {
                return m_syncLock.ExecuteInReadLock(() => m_engineThread != null);
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            m_syncLock.ExecuteInWriteLock(this.StartInternal);
        }

        public void Stop()
        {
            EnsureNotDisposed();

            var engineThread = m_syncLock.ExecuteInReadLock(() => m_engineThread);

            Application.Idle -= this.Application_Idle;

            m_syncLock.ExecuteInReadLock(
                () => this.Logics.DoForEach(
                    item =>
                    {
                        item.Thread.Abort();
                        item.Thread = null;
                    }));

            m_stopEvent.Set();
            Thread.Sleep((int)(GameConstants.LogicPollFrequency.TotalMilliseconds * 5));
            m_syncLock.ExecuteInWriteLock(
                () =>
                {
                    if (!m_engineThread.Join(s_stopTimeout))
                    {
                        m_engineThread.Abort();
                        m_engineThread.Join();
                    }

                    m_engineThread = null;
                });
        }

        public void Reset()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Cannot reset game engine since it is running.");
            }

            m_syncLock.ExecuteInWriteLock(this.ResetInternal);
            CallPaintCallback();
        }

        public void CallPaint()
        {
            CallPaintCallback();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (m_disposed)
            {
                return;
            }

            m_syncLock.EnterWriteLock();
            try
            {
                m_stopEvent.DisposeSafely();
                foreach (var item in this.AllChickens)
                {
                    item.Logic.DisposeSafely();
                }
                m_moveCount.DisposeSafely();

                m_disposed = true;
            }
            finally
            {
                m_syncLock.ExitWriteLock();
            }
            m_syncLock.DisposeSafely();
        }

        #endregion
    }
}