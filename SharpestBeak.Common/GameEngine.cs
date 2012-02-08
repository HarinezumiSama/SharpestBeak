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
    // TODO: [VM] Capture the game to allow playback

    // TODO: [VM] Implement custom chicken unit positioning (or even allow game frame snapshot as a start of a game)

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
        private readonly IList<ChickenUnit> m_allChickens;
        private readonly List<ChickenUnit> m_aliveChickensDirect;
        private readonly List<ShotUnit> m_shotUnitsDirect;
        private readonly IList<ChickenUnitLogic> m_logics;
        private readonly ChickenUnitLogic m_lightTeamLogic;
        private readonly ChickenUnitLogic m_darkTeamLogic;
        private bool m_disposed;
        private readonly Action<GamePaintEventArgs> m_paintCallback;
        private readonly ThreadSafeValue<long> m_moveCount;
        private readonly ThreadSafeValue<GameTeam?> m_winningTeam;
        private readonly ConvexPolygonPrimitive m_boardPolygon;
        private Thread m_engineThread;
        private bool m_finalizingStage;

        private readonly ThreadSafeValue<GamePresentation> m_lastGamePresentation;

        private readonly Dictionary<ChickenUnit, MoveInfo> m_moveInfos;
        private readonly Dictionary<ChickenUnit, MoveInfoStates> m_moveInfoStates;
        private readonly Dictionary<ChickenUnit, MoveInfo> m_previousMoves;
        private readonly List<ShotUnit> m_newShotUnits;

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
            m_moveCount = new ThreadSafeValue<long>();
            m_winningTeam = new ThreadSafeValue<GameTeam?>();
            m_lastGamePresentation = new ThreadSafeValue<GamePresentation>();

            // Post-initialized properties
            m_lightTeamLogic = CreateLogic(this, lightTeam, GameTeam.Light);
            m_darkTeamLogic = CreateLogic(this, darkTeam, GameTeam.Dark);
            m_logics = new[] { m_lightTeamLogic, m_darkTeamLogic }
                .ToList()
                .AsReadOnly();

            m_allChickens = m_logics
                .SelectMany(item => item.Units)
                .ToList()
                .AsReadOnly();
            m_allChickens.DoForEach((item, index) => item.UniqueId = index + 1);

            m_aliveChickensDirect = new List<ChickenUnit>();
            this.AliveChickens = m_aliveChickensDirect.AsReadOnly();

            m_shotUnitsDirect = new List<ShotUnit>(m_allChickens.Count);
            this.ShotUnits = m_shotUnitsDirect.AsReadOnly();

            m_moveInfos = new Dictionary<ChickenUnit, MoveInfo>(m_allChickens.Count);
            m_moveInfoStates = new Dictionary<ChickenUnit, MoveInfoStates>(m_allChickens.Count);
            m_previousMoves = new Dictionary<ChickenUnit, MoveInfo>(m_allChickens.Count);
            m_newShotUnits = new List<ShotUnit>(m_allChickens.Count);

            var realSize = this.Data.RealSize;
            m_boardPolygon = new ConvexPolygonPrimitive(
                Point2D.Zero,
                new Point2D(realSize.Width, 0f),
                new Point2D(realSize),
                new Point2D(0f, realSize.Height));

            #region Argument Check

            var maxChickenCount = this.Data.NominalSize.Width * this.Data.NominalSize.Height / 2;
            if (m_allChickens.Count > maxChickenCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}. Maximum is {3}.",
                        m_allChickens.Count,
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

            var result = (ChickenUnitLogic)Activator.CreateInstance(logicRecord.Type).EnsureNotNull();
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
            for (int index = 0; index < m_allChickens.Count; index++)
            {
                var chicken = m_allChickens[index];

                Point2D newPosition;
                do
                {
                    var nominalPosition = new Point(
                        s_random.Next(this.Data.NominalSize.Width),
                        s_random.Next(this.Data.NominalSize.Height));
                    newPosition = GameHelper.NominalToReal(nominalPosition);
                }
                while (m_allChickens.Take(index).Any(
                    item => item.Position.GetDistance(newPosition) < GameConstants.NominalCellSize));

                var newAngle = (float)Math.Floor(
                    MathHelper.HalfRevolutionDegrees - s_random.NextDouble() * GameConstants.FullRevolutionAngle);

                chicken.Position = newPosition;
                chicken.BeakAngle = GameAngle.FromDegrees(GameAngle.NormalizeDegreeAngle(newAngle));
            }
        }

        private void CallPaintCallback()
        {
            if (m_paintCallback == null)
            {
                return;
            }

            var presentation = m_lastGamePresentation.Value;
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
            m_logics.DoForEach(
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

            m_logics.DoForEach(item => item.Thread.Start(item));
        }

        private void ResetInternal()
        {
            m_moveCount.Value = 0L;
            m_finalizingStage = false;
            m_winningTeam.Value = null;
            lock (m_shotIndexCounterLock)
            {
                m_shotIndexCounter = 0;
            }

            m_aliveChickensDirect.ChangeContents(m_allChickens);
            m_shotUnitsDirect.Clear();
            m_allChickens.DoForEach(item => item.Reset());
            m_previousMoves.Clear();

            PositionChickens();
            UpdateUnitStates();

            m_logics.DoForEach(item => item.Reset());

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

                m_lightTeamLogic.MakeMoveEvent.Set();
                m_darkTeamLogic.MakeMoveEvent.Set();
                sw.Restart();
                while (sw.Elapsed < GameConstants.LogicPollFrequency)
                {
                    if (IsStopping())
                    {
                        return;
                    }
                    Thread.Yield();
                }
                m_lightTeamLogic.MakeMoveEvent.Reset();
                m_darkTeamLogic.MakeMoveEvent.Reset();

                using (SettingsCache.Instance.EnableDebugOutput
                    ? new AutoStopwatch(s => DebugHelper.WriteLine(s))
                    {
                        OutputFormat = string.Format("Engine step #{0} took {{0}}.", this.MoveCount + 1)
                    }
                    : null)
                {
                    ProcessEngineStep();
                }

                // Wrap with lock
                lock (m_moveCount.Lock)
                {
                    m_moveCount.Value++;
                }

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

        private void ProcessEngineStep()
        {
            var aliveChickens = this.AliveChickens
                .Where(item => !item.IsDead && item.Logic.Error == null)
                .ToList();

            m_moveInfos.Clear();
            m_previousMoves.Clear();
            for (int logicIndex = 0; logicIndex < m_logics.Count; logicIndex++)
            {
                var logic = m_logics[logicIndex];

                lock (logic.UnitsMovesLock)
                {
                    foreach (var pair in logic.UnitsMoves)
                    {
                        m_moveInfos.Add(pair.Key, pair.Value);
                        m_previousMoves.Add(pair.Key, pair.Value);
                    }
                }
            }

            if (!ProcessChickenUnitMoves(aliveChickens))
            {
                return;
            }

            // Processing new shot units
            var shootingMoves = m_moveInfos.Where(item => !item.Key.IsDead && item.Value.FireMode != FireMode.None);
            ProcessNewShots(shootingMoves);

            #region Processing Shot Collisions

            foreach (var shotUnit in m_shotUnitsDirect)
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
            for (int index = 0; index < m_shotUnitsDirect.Count; index++)
            {
                var shotUnit = m_shotUnitsDirect[index];

                for (int otherIndex = index + 1; otherIndex < m_shotUnitsDirect.Count; otherIndex++)
                {
                    var otherShotUnit = m_shotUnitsDirect[otherIndex];

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

            m_aliveChickensDirect.RemoveAll(item => item.IsDead);
            m_shotUnitsDirect.RemoveAll(item => item.Exploded);

            var aliveTeams = m_aliveChickensDirect.Select(item => item.Team).Distinct().ToList();
            if (aliveTeams.Count <= 1)
            {
                m_finalizingStage = true;
                if (!m_shotUnitsDirect.Any())
                {
                    var winningTeam = aliveTeams.SingleOrDefault();
                    RaiseGameEnded(winningTeam);
                    return;
                }
            }
        }

        private void ProcessNewShots(IEnumerable<KeyValuePair<ChickenUnit, MoveInfo>> shootingMoves)
        {
            m_newShotUnits.Clear();
            foreach (var shootingMovePair in shootingMoves)
            {
                var unit = shootingMovePair.Key;

                if (!unit.CanShoot())
                {
                    DebugHelper.WriteLine("New shot from {{{0}}} has been skipped - too fast.", unit);
                    continue;
                }

                if (m_finalizingStage)
                {
                    var thisShotTeam = unit.Team;
                    if (!m_shotUnitsDirect.Any(su => su.Owner.Team != thisShotTeam))
                    {
                        DebugHelper.WriteLine(
                            "New shot from {{{0}}} has been skipped - finalizing stage and no enemy shots.",
                            unit);
                        continue;
                    }
                }

                var shot = new ShotUnit(unit, GetShotUniqueIndex());
                m_newShotUnits.Add(shot);
                unit.ShotEngineStepIndex = m_moveCount.Value;

                DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, unit);
            }
            m_shotUnitsDirect.AddRange(m_newShotUnits);
        }

        private bool ProcessChickenUnitMoves(IList<ChickenUnit> aliveChickens)
        {
            // TODO: [VM] Use bisection to get conflicting units closer to each other
            // TODO: [VM] Optimize number of collision checks!
            // TODO: [VM] Divide move: eg. unit couldn't move but could turn beak or vice versa

            m_moveInfoStates.Clear();
            for (int unitIndex = 0; unitIndex < aliveChickens.Count; unitIndex++)
            {
                if (IsStopping())
                {
                    return false;
                }

                var unit = aliveChickens[unitIndex];
                m_moveInfoStates[unit] = MoveInfoStates.Handled;

                var moveInfo = m_moveInfos.GetValueOrDefault(unit);
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
                var newBeakAngle = GameHelper.GetNewBeakAngle(unit.BeakAngle, moveInfo.BeakTurn);

                var newPositionElement = new ChickenElement(newPosition, newBeakAngle);
                if (HasOutOfBoardCollision(newPositionElement))
                {
                    m_moveInfoStates[unit] = MoveInfoStates.RejectedBoardCollision;
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
                    m_moveInfoStates[unit] = MoveInfoStates.RejectedOtherUnitCollision;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with {{{1}}}.",
                        unit,
                        conflictingChicken);
                }
                else
                {
                    unit.Position = newPosition;
                    unit.BeakAngle = newBeakAngle;

                    DebugHelper.WriteLine("Chicken {{{0}}} has moved.", unit);
                }
            }

            return true;
        }

        private bool UpdateUnitStates()
        {
            for (int logicIndex = 0; logicIndex < m_logics.Count; logicIndex++)
            {
                var logic = m_logics[logicIndex];

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

                        var previousMove = m_previousMoves.GetValueOrDefault(unit) ?? MoveInfo.Empty;
                        var previousMoveState = m_moveInfoStates.GetValueOrDefault(unit, MoveInfoStates.Handled);
                        var unitState = new ChickenUnitState(unit, previousMove, previousMoveState);
                        logic.UnitsStates.Add(unit, unitState);
                    }
                }
            }

            return true;
        }

        private void UpdateLastGamePresentation()
        {
            var lastGamePresentation = new GamePresentation(this);
            m_lastGamePresentation.Value = lastGamePresentation;
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
                    while (!logic.MakeMoveEvent.WaitOne(0))
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
                return m_logics;
            }
        }

        public long MoveCount
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

            m_syncLock.ExecuteInWriteLock(
                () => m_logics.DoForEach(
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
                m_lightTeamLogic.DisposeSafely();
                m_darkTeamLogic.DisposeSafely();

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