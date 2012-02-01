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
        private readonly AutoResetEvent m_makeMoveEvent = new AutoResetEvent(false);
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
        private readonly ThreadSafeValue<ulong> m_moveCount;
        private readonly ThreadSafeValue<GameTeam?> m_winningTeam;
        private readonly ConvexPolygonPrimitive m_boardPolygon;
        private Thread m_engineThread;
        private bool m_finalizingStage;

        private readonly object m_lastGamePresentationLock = new object();
        private GamePresentation m_lastGamePresentation;

        private readonly Dictionary<ChickenUnit, MoveInfo> m_previousMoves;

        private readonly object m_shotIndexCounterLock = new object();
        private int m_shotIndexCounter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(
            Action<GamePaintEventArgs> paintCallback,
            Size nominalSize,
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
            this.Data = new StaticData(nominalSize);
            m_moveCount = new ThreadSafeValue<ulong>(m_syncLock);
            m_winningTeam = new ThreadSafeValue<GameTeam?>(m_syncLock);

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
            m_previousMoves = new Dictionary<ChickenUnit, MoveInfo>(this.AllChickens.Count);

            var realSize = this.Data.RealSize;
            m_boardPolygon = new ConvexPolygonPrimitive(
                Point2D.Zero,
                new Point2D(realSize.Width, 0f),
                new Point2D(realSize),
                new Point2D(0f, realSize.Height));

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
            GamePresentation presentation;
            lock (m_lastGamePresentationLock)
            {
                presentation = m_lastGamePresentation;
            }

            m_paintCallback(new GamePaintEventArgs(presentation));
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
            this.Logics.DoForEach(
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
            m_previousMoves.Clear();

            PositionChickens();
            UpdateUnitStates();

            this.Logics.DoForEach(item => item.Reset());

            UpdateLastGamePresentation();
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
            var sw = new Stopwatch();
            var moveInfos = new Dictionary<ChickenUnit, MoveInfo>(this.AllChickens.Count);
            var newShotUnits = new List<ShotUnit>(this.AllChickens.Count);

            while (!IsStopping())
            {
                if (m_disposed)
                {
                    return;
                }

                if (!UpdateUnitStates())
                {
                    return;
                }

                m_makeMoveEvent.Set();
                sw.Restart();
                while (sw.Elapsed < GameConstants.LogicPollFrequency)
                {
                    if (IsStopping())
                    {
                        return;
                    }
                    Thread.Yield();
                }
                m_makeMoveEvent.Reset();

                using (SettingsCache.Instance.EnableDebugOutput
                    ? new AutoStopwatch(s => DebugHelper.WriteLine(s))
                    {
                        OutputFormat = string.Format("Engine step #{0} took {{0}}.", this.MoveCount + 1)
                    }
                    : null)
                {
                    ProcessEngineStep(moveInfos, newShotUnits);
                }

                // Wrap with lock
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

                if (IsStopping() || m_winningTeam.Value.HasValue)
                {
                    return;
                }
            }
        }

        private void ProcessEngineStep(
            Dictionary<ChickenUnit, MoveInfo> moveInfos,
            List<ShotUnit> newShotUnits)
        {
            var aliveChickens = this.AliveChickens
                .Where(item => !item.IsDead && item.Logic.Error == null)
                .ToList();

            moveInfos.Clear();
            m_previousMoves.Clear();
            for (int logicIndex = 0; logicIndex < this.Logics.Count; logicIndex++)
            {
                var logic = this.Logics[logicIndex];

                lock (logic.UnitsMovesLock)
                {
                    foreach (var pair in logic.UnitsMoves)
                    {
                        moveInfos.Add(pair.Key, pair.Value);
                        m_previousMoves.Add(pair.Key, pair.Value);
                    }
                }
            }

            if (!ProcessChickenUnitMoves(aliveChickens, moveInfos))
            {
                return;
            }

            // Processing new shot units
            var shootingMoves = moveInfos.Where(item => item.Value.FireMode != FireMode.None);
            ProcessNewShots(shootingMoves, newShotUnits);

            #region Processing Shot Collisions

            foreach (var shotUnit in this.ShotUnitsDirect)
            {
                shotUnit.Position = GameHelper.GetNewPosition(
                    shotUnit.Position,
                    shotUnit.Angle,
                    MoveDirection.MoveForward,
                    GameConstants.ShotUnit.DefaultRectilinearStepDistance);
                DebugHelper.WriteLine("Shot {{{0}}} has moved.", shotUnit);

                if (HasOutOfBoardCollision(shotUnit.GetElement()))
                {
                    shotUnit.Exploded = true;

                    DebugHelper.WriteLine("Shot {{{0}}} has exploded outside of game board.", shotUnit);
                }
            }

            var injuredChickens = new List<ChickenUnit>(aliveChickens.Count);
            for (int index = 0; index < this.ShotUnitsDirect.Count; index++)
            {
                var shotUnit = this.ShotUnitsDirect[index];

                for (int otherIndex = index + 1; otherIndex < this.ShotUnitsDirect.Count; otherIndex++)
                {
                    var otherShotUnit = this.ShotUnitsDirect[otherIndex];

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

                var shotElement = shotUnit.GetElement();

                injuredChickens.Clear();
                for (int chickenIndex = 0; chickenIndex < aliveChickens.Count; chickenIndex++)
                {
                    var aliveChicken = aliveChickens[chickenIndex];
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

            #endregion

            UpdateLastGamePresentation();

            this.AliveChickensDirect.RemoveAll(item => item.IsDead);
            this.ShotUnitsDirect.RemoveAll(item => item.Exploded);

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
        }

        private void ProcessNewShots(
            IEnumerable<KeyValuePair<ChickenUnit, MoveInfo>> shootingMoves,
            List<ShotUnit> newShotUnits)
        {
            newShotUnits.Clear();
            foreach (var shootingMovePair in shootingMoves)
            {
                var unit = shootingMovePair.Key;

                // Is there any active shot unit from the same chicken unit?
                if (unit.HasShots())
                {
                    if (!unit.CanShootDueToTimer())
                    {
                        DebugHelper.WriteLine("New shot from {{{0}}} has been skipped - too fast.", unit);
                        continue;
                    }
                }

                if (m_finalizingStage)
                {
                    var thisShotTeam = unit.Team;
                    if (!this.ShotUnitsDirect.Any(su => su.Owner.Team != thisShotTeam))
                    {
                        DebugHelper.WriteLine(
                            "New shot from {{{0}}} has been skipped - finalizing stage and no enemy shots.",
                            unit);
                        continue;
                    }
                }

                var shot = new ShotUnit(unit, GetShotUniqueIndex());
                newShotUnits.Add(shot);
                unit.ShotTimer.Restart();

                DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, unit);
            }
            this.ShotUnitsDirect.AddRange(newShotUnits);
        }

        private bool ProcessChickenUnitMoves(
            IList<ChickenUnit> aliveChickens,
            Dictionary<ChickenUnit, MoveInfo> moveInfos)
        {
            // TODO: [VM] Use bisection to get conflicting units closer to each other
            // TODO: [VM] Optimize number of collision checks!

            for (int unitIndex = 0; unitIndex < aliveChickens.Count; unitIndex++)
            {
                if (IsStopping())
                {
                    return false;
                }

                var unit = aliveChickens[unitIndex];
                var moveInfo = moveInfos.GetValueOrDefault(unit);
                if (moveInfo == null)
                {
                    continue;
                }
                DebugHelper.WriteLine(
                    "{0} is processing move {{{1}}} of chicken {{{2}}}.",
                    GetType().Name,
                    moveInfo,
                    unit);

                var newPosition = GameHelper.GetNewPosition(
                    unit.Position,
                    unit.BeakAngle,
                    moveInfo.MoveDirection,
                    GameConstants.ChickenUnit.DefaultRectilinearStepDistance);
                var newBeakAngle = GameHelper.GetNewBeakAngle(
                    unit.BeakAngle,
                    moveInfo.BeakTurn,
                    GameConstants.StepTimeDelta);

                var newPositionElement = new ChickenElement(newPosition, newBeakAngle);
                if (HasOutOfBoardCollision(newPositionElement))
                {
                    moveInfo.State = MoveInfoState.Rejected;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with game board border.",
                        unit);
                    continue;
                }

                ChickenUnit conflictingChicken = null;
                for (int conflictingIndex = 0; conflictingIndex < aliveChickens.Count; conflictingIndex++)
                {
                    var aliveChicken = aliveChickens[conflictingIndex];
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
                    moveInfo.State = MoveInfoState.Rejected;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with {{{1}}}.",
                        unit,
                        conflictingChicken);
                }
                else
                {
                    unit.Position = newPosition;
                    unit.BeakAngle = newBeakAngle;
                    moveInfo.State = MoveInfoState.Handled;

                    DebugHelper.WriteLine("Chicken {{{0}}} has moved.", unit);
                }
            }

            return true;
        }

        private bool UpdateUnitStates()
        {
            for (int logicIndex = 0; logicIndex < this.Logics.Count; logicIndex++)
            {
                var logic = this.Logics[logicIndex];

                lock (logic.UnitsStatesLock)
                {
                    logic.UnitsStates.Clear();

                    for (int unitIndex = 0; unitIndex < logic.Units.Count; unitIndex++)
                    {
                        if (IsStopping())
                        {
                            return false;
                        }

                        var unit = logic.Units[unitIndex];

                        var previousMove = m_previousMoves.GetValueOrDefault(unit);
                        var unitState = new ChickenUnitState(unit, previousMove);
                        logic.UnitsStates.Add(unit, unitState);
                    }
                }
            }

            return true;
        }

        private void UpdateLastGamePresentation()
        {
            lock (m_lastGamePresentationLock)
            {
                m_lastGamePresentation = new GamePresentation(this);
            }
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
                    while (!m_makeMoveEvent.WaitOne(0))
                    {
                        if (IsStopping())
                        {
                            return;
                        }

                        Thread.Yield();
                    }

                    var moveResult = logic.MakeMove();

                    lock (logic.UnitsMovesLock)
                    {
                        logic.UnitsMoves.Clear();
                        foreach (var movePair in moveResult.InnerMap)
                        {
                            DebugHelper.WriteLine(
                                "Logic '{0}', for chicken {{{1}}}, is making move {{{2}}}.",
                                logic.GetType().Name,
                                movePair.Key,
                                movePair.Value == null ? "NONE" : movePair.Value.ToString());

                            logic.UnitsMoves.Add(movePair.Key, movePair.Value);
                        }
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
            UpdateLastGamePresentation();

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
                m_makeMoveEvent.DisposeSafely();

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