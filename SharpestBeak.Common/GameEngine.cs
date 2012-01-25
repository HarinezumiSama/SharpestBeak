using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SharpestBeak.Common.Diagnostics;
using SharpestBeak.Common.Presentation;
using SharpestBeak.Common.Presentation.Elements;
using SharpestBeak.Common.Presentation.Primitives;
using SharpestBeak.Common.Properties;

namespace SharpestBeak.Common
{
    public sealed class GameEngine : IDisposable
    {
        #region Constants

        public const int InstrumentationMoveCountLimit = 500;

        #endregion

        #region Fields

        private static readonly ThreadSafeRandom s_random = new ThreadSafeRandom();
        private static readonly TimeSpan s_stopTimeout = TimeSpan.FromSeconds(5d);

        private readonly ReaderWriterLockSlim m_syncLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly ManualResetEvent m_stopEvent = new ManualResetEvent(false);
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
        private readonly ThreadSafeValue<ulong> m_moveCount;
        private readonly ThreadSafeValue<GameTeam?> m_winningTeam;
        private readonly ConvexPolygonPrimitive m_boardPolygon;
        private Thread m_engineThread;
        private bool m_finalizingStage;

        private readonly object m_lastMovesLock = new object();

        private readonly object m_shotIndexCounterLock = new object();
        private int m_shotIndexCounter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(
            Action<GamePaintEventArgs> paintCallback,
            Size size,
            ChickenTeamRecord lightTeam,
            ChickenTeamRecord darkTeam)
        {
            #region Argument Check

            if (paintCallback == null)
            {
                throw new ArgumentNullException("paintCallback");
            }
            if (lightTeam == null)
            {
                throw new ArgumentNullException("lightTeam");
            }
            if (darkTeam == null)
            {
                throw new ArgumentNullException("darkTeam");
            }

            #endregion

            m_paintCallback = paintCallback;

            if (SettingsCache.Instance.UsePerformanceCounters)
            {
                PerformanceCounterHelper.Initialize();
            }

            // Pre-initialized properties
            this.Data = new StaticData(size);
            m_moveCount = new ThreadSafeValue<ulong>(m_syncLock);
            m_winningTeam = new ThreadSafeValue<GameTeam?>(m_syncLock);
            this.LastMoves = new Dictionary<ChickenUnit, ChickenUnitState>();

            // Post-initialized properties
            this.Logics =
                new[]
                {
                    CreateLogic(this, lightTeam, GameTeam.Light),
                    CreateLogic(this, darkTeam, GameTeam.Dark)
                }
                .ToList()
                .AsReadOnly();
            this.AllChickens = this.Logics
                .SelectMany(item => item.Units)
                .ToList()
                .AsReadOnly();
            this.AllChickens.DoForEach((item, index) => item.UniqueId = index + 1);
            this.AliveChickensDirect = new List<ChickenUnit>();
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();
            this.ShotUnitsDirect = new List<ShotUnit>();
            this.ShotUnits = this.ShotUnitsDirect.AsReadOnly();

            m_boardPolygon = new ConvexPolygonPrimitive(
                Point2D.Zero,
                new Point2D(this.Data.RealSize.Width, 0f),
                new Point2D(this.Data.RealSize.Width, this.Data.RealSize.Height),
                new Point2D(0f, this.Data.RealSize.Height));

            #region Argument Check

            var maxChickenCount = this.Data.NominalSize.Width * this.Data.NominalSize.Height / 2;
            if (this.AllChickens.Count > maxChickenCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}. Maximum is {3}.",
                        this.AllChickens.Count,
                        this.Data.NominalSize.Width,
                        this.Data.NominalSize.Height,
                        maxChickenCount),
                    "size");
            }

            #endregion

            ResetInternal();
        }

        #endregion

        #region Private Methods

        private static ChickenUnitLogic CreateLogic(GameEngine engine, ChickenTeamRecord logicRecord, GameTeam team)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }
            if (logicRecord == null)
            {
                throw new ArgumentNullException("logicRecord");
            }

            #endregion

            var result = (ChickenUnitLogic)Activator.CreateInstance(logicRecord.Type);
            result.InitializeInstance(engine, logicRecord.UnitCount, team);
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
                    var nominalPosition = new Point(
                        s_random.Next(this.Data.NominalSize.Width),
                        s_random.Next(this.Data.NominalSize.Height));
                    newPosition = GameHelper.NominalToReal(nominalPosition);
                }
                while (this.AllChickens.Take(index).Any(
                    item => item.Position.GetDistance(newPosition) < GameConstants.NominalCellSize));

                var newAngle = (float)Math.Floor(
                    MathHelper.HalfRevolutionDegrees - s_random.NextDouble() * GameConstants.FullRevolutionAngle);

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

        private bool HasOutOfBoardCollision(ICollidableElement element)
        {
            #region Argument Check

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            #endregion

            var primitives = element.GetPrimitives();

            // First, checking just base points
            for (int index = 0; index < primitives.Count; index++)
            {
                var primitive = primitives[index];
                if (!CollisionDetector.IsPointInPolygon(primitive.BasePoint, m_boardPolygon))
                {
                    return true;
                }
            }

            // Then checking intersection of element's primitives with board borders
            for (int index = 0; index < primitives.Count; index++)
            {
                var primitive = primitives[index];
                for (int edgeIndex = 0; edgeIndex < m_boardPolygon.Edges.Count; edgeIndex++)
                {
                    var edge = m_boardPolygon.Edges[edgeIndex];
                    if (edge.HasCollision(primitive))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void StartInternal()
        {
            EnsureNotDisposed();

            if (m_engineThread != null)
            {
                throw new InvalidOperationException("Engine is already running.");
            }

            if (m_winningTeam.Value.HasValue)
            {
                throw new InvalidOperationException("The game has ended. Reset the game before starting it again.");
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
                });

            m_stopEvent.Reset();
            CallPaintCallback();
            m_engineThread.Start();

            this.Logics.DoForEach(item => item.Thread.Start(item));
        }

        private void ResetInternal()
        {
            m_moveCount.Value = 0;
            m_finalizingStage = false;
            m_winningTeam.Value = null;
            lock (m_shotIndexCounterLock)
            {
                m_shotIndexCounter = 0;
            }

            this.AliveChickensDirect.ChangeContents(this.AllChickens);
            this.ShotUnitsDirect.Clear();
            this.AllChickens.DoForEach(item => item.Reset());

            PositionChickens();

            this.Logics.DoForEach(item => item.Reset());
        }

        private int GetShotUniqueIndex()
        {
            lock (m_shotIndexCounterLock)
            {
                m_shotIndexCounter++;
                return m_shotIndexCounter;
            }
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

                m_moveCount.Value++;
                if (SettingsCache.Instance.UsePerformanceCounters)
                {
                    PerformanceCounterHelper.Instance.CollisionCountPerStepBase.Increment();
                }

                if (SettingsCache.Instance.InstrumentationMode && m_moveCount.Value >= InstrumentationMoveCountLimit)
                {
                    if (!m_winningTeam.Value.HasValue)
                    {
                        RaiseGameEnded(GameTeam.None);
                    }
                    return;
                }

                if (IsStopping())
                {
                    return;
                }
            }
        }

        private void ProcessEngineStep(ref List<ChickenUnitState> previousUnitStates)
        {
            var aliveChickens = this.AliveChickens
                .Where(item => item.Logic.Error == null)
                .ToList();

            var newUnitStates = new List<ChickenUnitState>(aliveChickens.Count);
            lock (m_lastMovesLock)
            {
                for (int index = 0; index < aliveChickens.Count; index++)
                {
                    var aliveChicken = aliveChickens[index];
                    var unitState = this.LastMoves.GetValueOrDefault(aliveChicken);
                    if (unitState != null
                        && unitState.CurrentMove != null
                        && unitState.CurrentMove.State == MoveInfoState.None)
                    {
                        newUnitStates.Add(unitState);
                    }
                }
            }

            var oldShotUnits = this.ShotUnitsDirect.ToArray();

            // Processing new shot units
            var shootingMoves = newUnitStates.Where(item => item.CurrentMove.FireMode != FireMode.None).ToArray();
            ProcessNewShots(oldShotUnits, shootingMoves);

            // TODO: [VM] Optimize process of collision detection: don't check if already checked or smth like that

            #region Processing Shot Collisions

            foreach (var oldShotUnit in oldShotUnits)
            {
                oldShotUnit.Position = GameHelper.GetNewPosition(
                    oldShotUnit.Position,
                    oldShotUnit.Angle,
                    MoveDirection.MoveForward,
                    GameConstants.ShotUnit.DefaultRectilinearSpeed,
                    GameConstants.StepTimeDelta);
                DebugHelper.WriteLine("Shot {{{0}}} has moved.", oldShotUnit);

                if (HasOutOfBoardCollision(oldShotUnit.GetElement()))
                {
                    oldShotUnit.Exploded = true;

                    DebugHelper.WriteLine("Shot {{{0}}} has exploded outside of game board.", oldShotUnit);
                }
            }

            using (new AutoStopwatch(s => DebugHelper.WriteLine(s))
                {
                    OutputFormat = "Shot collisions took {0}."
                })
            {
                for (int index = 0; index < oldShotUnits.Length; index++)
                {
                    var shotUnit = oldShotUnits[index];

                    for (int otherIndex = index + 1; otherIndex < oldShotUnits.Length; otherIndex++)
                    {
                        var otherShotUnit = oldShotUnits[otherIndex];

                        if (CollisionDetector.CheckCollision(shotUnit.GetElement(), otherShotUnit.GetElement()))
                        {
                            shotUnit.Exploded = true;
                            otherShotUnit.Exploded = true;

                            DebugHelper.WriteLine(
                                "Mutual annihilation of shots {{{0}}} and {{{1}}}.",
                                shotUnit,
                                otherShotUnit);
                        }
                    }
                }

                foreach (var shotUnit in oldShotUnits)
                {
                    var shotElement = shotUnit.GetElement();

                    var injuredChickens = new List<ChickenUnit>(aliveChickens.Count);
                    for (int index = 0; index < aliveChickens.Count; index++)
                    {
                        var aliveChicken = aliveChickens[index];
                        if (!aliveChicken.IsDead
                            && CollisionDetector.CheckCollision(shotElement, aliveChicken.GetElement()))
                        {
                            injuredChickens.Add(aliveChicken);
                        }
                    }

                    foreach (var injuredChicken in injuredChickens)
                    {
                        shotUnit.Exploded = true;

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

            #endregion

            if (!ProcessChickenUnitMoves(previousUnitStates, aliveChickens))
            {
                return;
            }

            lock (m_lastMovesLock)
            {
                this.AliveChickensDirect.RemoveAll(item => item.IsDead);
                this.ShotUnitsDirect.RemoveAll(item => item.Exploded);
            }

            var aliveTeams = this.AliveChickensDirect.Select(item => item.Team).Distinct().ToList();
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
                if (SettingsCache.Instance.InstrumentationMode && m_moveCount.Value >= InstrumentationMoveCountLimit)
                {
                    return;
                }
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
                                item.UniqueId,
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

        private void ProcessNewShots(IEnumerable<ShotUnit> oldShotUnits, IEnumerable<ChickenUnitState> shootingMoves)
        {
            foreach (var shootingMove in shootingMoves)
            {
                // Is there any active shot unit from the same chicken unit?
                if (shootingMove.Unit.HasShots())
                {
                    if (!shootingMove.Unit.CanShootDueToTimer())
                    {
                        DebugHelper.WriteLine("New shot from {{{0}}} has been skipped - too fast.", shootingMove.Unit);
                        continue;
                    }
                }

                if (m_finalizingStage)
                {
                    var thisShotTeam = shootingMove.Team;
                    if (!oldShotUnits.Any(su => su.Owner.Team != thisShotTeam))
                    {
                        DebugHelper.WriteLine(
                            "New shot from {{{0}}} has been skipped - finalizing stage and no enemy shots.",
                            shootingMove.Unit);
                        continue;
                    }
                }

                var shot = new ShotUnit(shootingMove.Unit, GetShotUniqueIndex());
                lock (m_lastMovesLock)
                {
                    this.ShotUnitsDirect.Add(shot);
                }
                shootingMove.Unit.ShotTimer.Restart();

                DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, shootingMove.Unit);
            }
        }

        private bool ProcessChickenUnitMoves(
            IList<ChickenUnitState> previousUnitStates,
            IList<ChickenUnit> aliveChickens)
        {
            if (previousUnitStates == null || !previousUnitStates.Any())
            {
                return true;
            }

            foreach (var unitState in previousUnitStates)
            {
                if (IsStopping())
                {
                    return false;
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
                    GameConstants.StepTimeDelta);
                var newBeakAngle = GameHelper.GetNewBeakAngle(
                    unit.BeakAngle,
                    unitState.CurrentMove.BeakTurn,
                    GameConstants.StepTimeDelta);

                var newPositionElement = new ChickenElement(newPosition, newBeakAngle);
                if (HasOutOfBoardCollision(newPositionElement))
                {
                    unitState.CurrentMove.State = MoveInfoState.Rejected;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with game board border.",
                        unit);
                    continue;
                }

                ChickenUnit conflictingChicken = null;
                for (int index = 0; index < aliveChickens.Count; index++)
                {
                    var aliveChicken = aliveChickens[index];
                    if (aliveChicken == unit)
                    {
                        continue;
                    }

                    if (CollisionDetector.CheckCollision(newPositionElement, aliveChicken.GetElement()))
                    {
                        conflictingChicken = aliveChicken;
                        break;
                    }
                }
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

            return true;
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
            // TODO: [VM] Fix double message
            m_winningTeam.Value = winningTeam;
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

        public StaticData Data
        {
            get;
            private set;
        }

        // TODO: [VM] Remove this property, or change to some GetExtraBlaBlaBla for logics
        public IList<ChickenUnitLogic> Teams
        {
            [DebuggerNonUserCode]
            get
            {
                return this.Logics;
            }
        }

        public ulong MoveCount
        {
            [DebuggerNonUserCode]
            get
            {
                return m_moveCount.Value;
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

        public GameTeam? WinningTeam
        {
            [DebuggerNonUserCode]
            get
            {
                return m_winningTeam.Value;
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
                        if (item.Thread != null)
                        {
                            item.Thread.Abort();
                            item.Thread = null;
                        }
                    }));

            m_stopEvent.Set();
            Thread.Sleep((int)(GameConstants.LogicPollFrequency.TotalMilliseconds * 5));
            m_syncLock.ExecuteInWriteLock(
                () =>
                {
                    if (m_engineThread != null)
                    {
                        if (!m_engineThread.Join(s_stopTimeout))
                        {
                            m_engineThread.Abort();
                            m_engineThread.Join();
                        }

                        m_engineThread = null;
                    }
                });
        }

        public void Reset()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException("Cannot reset game engine while it is running.");
            }

            m_syncLock.ExecuteInWriteLock(this.ResetInternal);
            CallPaintCallback();
        }

        // TODO: [VM] Implement Pause method (?)

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