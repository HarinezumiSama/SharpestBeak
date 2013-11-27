using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using SharpestBeak.Configuration;
using SharpestBeak.Diagnostics;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak
{
    //// TODO: [vmcl] Capture the game to allow playback

    //// TODO: [vmcl] Allow game frame snapshot as a start of a game

    //// TODO: [vmcl] Implement single-thread feature: logics are running with engine in single thread - ???

    //// TODO: [vmcl] Seems that in some cases collisions are detected incorrectly (mostly chicken/chicken)

    public sealed class GameEngine : IDisposable
    {
        #region Constants and Fields

        internal static readonly int InstrumentationMoveCountLimit = 500;

        private static readonly ThreadSafeRandom RandomGenerator = new ThreadSafeRandom();
        private static readonly TimeSpan StopTimeout = GameConstants.LogicPollFrequency.Multiply(5);

        private readonly ReaderWriterLockSlim _syncLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly ManualResetEventSlim _makeMoveEvent = new ManualResetEventSlim(false);
        private readonly ReadOnlyCollection<ChickenUnit> _allChickens;
        private readonly List<ChickenUnit> _aliveChickensDirect;
        private readonly List<ShotUnit> _shotUnitsDirect;
        private readonly ReadOnlyCollection<ChickenUnitLogicExecutor> _logicExecutors;
        private readonly ChickenUnitLogicExecutor _lightTeamLogicExecutor;
        private readonly ChickenUnitLogicExecutor _darkTeamLogicExecutor;
        private readonly Action<GamePaintEventArgs> _paintCallback;
        private readonly Action<GamePositionEventArgs> _positionCallback;
        private readonly ThreadSafeValue<long> _moveCount;
        private readonly ThreadSafeValue<GameTeam?> _winningTeam;

        private readonly ThreadSafeValue<GamePresentation> _lastGamePresentation;

        private readonly Dictionary<ChickenUnit, MoveInfo> _moveInfos;
        private readonly Dictionary<ChickenUnit, MoveInfoStates> _moveInfoStates;
        private readonly Dictionary<ChickenUnit, MoveInfo> _previousMoves;
        private readonly List<ShotUnit> _newShotUnits;

        private readonly object _shotIndexCounterLock = new object();

        private bool _disposed;

        private Thread _engineThread;
        private bool _finalizingStage;

        private int _shotIndexCounter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(GameEngineSettings settings)
        {
            #region Argument Check

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            #endregion

            if (SettingsCache.Instance.UsePerformanceCounters)
            {
                PerformanceCounterHelper.Initialize();
            }

            // Pre-initialized fields and properties
            _paintCallback = settings.PaintCallback;
            _positionCallback = settings.PositionCallback;
            this.Data = new GameEngineData(settings.NominalSize);
            _moveCount = new ThreadSafeValue<long>();
            _winningTeam = new ThreadSafeValue<GameTeam?>();
            _lastGamePresentation = new ThreadSafeValue<GamePresentation>();

            // Post-initialized fields and properties
            _lightTeamLogicExecutor = CreateLogicExecutor(this, settings.LightTeam, GameTeam.Light);
            _darkTeamLogicExecutor = CreateLogicExecutor(this, settings.DarkTeam, GameTeam.Dark);
            _logicExecutors = new[] { _lightTeamLogicExecutor, _darkTeamLogicExecutor }.ToArray().AsReadOnly();

            _allChickens = _logicExecutors.SelectMany(item => item.Units).ToArray().AsReadOnly();
            _allChickens.DoForEach((item, index) => item.UniqueId = new GameObjectId(index + 1));

            _aliveChickensDirect = new List<ChickenUnit>();
            this.AliveChickens = _aliveChickensDirect.AsReadOnly();

            _shotUnitsDirect = new List<ShotUnit>(_allChickens.Count);
            this.ShotUnits = _shotUnitsDirect.AsReadOnly();

            _moveInfos = new Dictionary<ChickenUnit, MoveInfo>(_allChickens.Count);
            _moveInfoStates = new Dictionary<ChickenUnit, MoveInfoStates>(_allChickens.Count);
            _previousMoves = new Dictionary<ChickenUnit, MoveInfo>(_allChickens.Count);
            _newShotUnits = new List<ShotUnit>(_allChickens.Count);

            #region Argument Check

            var maxChickenCount = this.Data.NominalSize.Width * this.Data.NominalSize.Height / 2;
            if (_allChickens.Count > maxChickenCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "Too many chickens ({0}) for the board of nominal size {1}x{2}. Maximum is {3}.",
                        _allChickens.Count,
                        this.Data.NominalSize.Width,
                        this.Data.NominalSize.Height,
                        maxChickenCount),
                    "settings");
            }

            #endregion

            ResetInternal();
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the game has ended. <b>NOTE</b>: handlers of this event are called from engine thread!
        /// </summary>
        public event EventHandler<GameEndedEventArgs> GameEnded;

        #endregion

        #region Public Properties

        public GameEngineData Data
        {
            get;
            private set;
        }

        public long MoveCount
        {
            [DebuggerNonUserCode]
            get
            {
                return _moveCount.Value;
            }
        }

        public bool IsRunning
        {
            [DebuggerNonUserCode]
            get
            {
                return _syncLock.ExecuteInReadLock(() => _engineThread != null);
            }
        }

        public GameTeam? WinningTeam
        {
            [DebuggerNonUserCode]
            get
            {
                return _winningTeam.Value;
            }
        }

        #endregion

        #region Internal Properties

        internal ReadOnlyCollection<ChickenUnit> AllChickens
        {
            [DebuggerStepThrough]
            get
            {
                return _allChickens;
            }
        }

        internal ReadOnlyCollection<ChickenUnit> AliveChickens
        {
            get;
            private set;
        }

        internal ReadOnlyCollection<ShotUnit> ShotUnits
        {
            get;
            private set;
        }

        internal ReadOnlyCollection<ChickenUnitLogicExecutor> LogicExecutors
        {
            [DebuggerStepThrough]
            get
            {
                return _logicExecutors;
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _syncLock.ExecuteInWriteLock(this.StartInternal);
        }

        public void Stop()
        {
            EnsureNotDisposed();

            _syncLock.ExecuteInWriteLock(
                () => _logicExecutors.DoForEach(item => item.Stop()));

            _stopEvent.Set();

            _syncLock.ExecuteInWriteLock(
                () =>
                {
                    if (_engineThread != null)
                    {
                        if (!_engineThread.Join(StopTimeout))
                        {
                            _engineThread.Abort();
                            _engineThread.Join();
                        }

                        _engineThread = null;
                    }
                });
        }

        public void Reset()
        {
            if (this.IsRunning)
            {
                throw new GameException("Cannot reset game engine while it is running.");
            }

            _syncLock.ExecuteInWriteLock(this.ResetInternal);
            CallPaintCallback();
        }

        public void CallPaint()
        {
            CallPaintCallback();
        }

        public GamePresentation GetPresentation()
        {
            _syncLock.EnterReadLock();
            try
            {
                return _lastGamePresentation.Value;
            }
            finally
            {
                _syncLock.ExitReadLock();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Stop();

            _syncLock.EnterWriteLock();
            try
            {
                _stopEvent.DisposeSafely();
                _lightTeamLogicExecutor.DisposeSafely();
                _darkTeamLogicExecutor.DisposeSafely();

                _makeMoveEvent.DisposeSafely();

                _disposed = true;
            }
            finally
            {
                _syncLock.ExitWriteLock();
            }

            _syncLock.DisposeSafely();
        }

        #endregion

        #region Private Methods

        private static ChickenUnitLogicExecutor CreateLogicExecutor(
            GameEngine engine,
            ChickenTeamSettings teamSettings,
            GameTeam team)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            if (teamSettings == null)
            {
                throw new ArgumentNullException("teamSettings");
            }

            #endregion

            var logic = (ChickenUnitLogic)Activator.CreateInstance(teamSettings.Type).EnsureNotNull();

            var result = new ChickenUnitLogicExecutor(
                engine,
                teamSettings.UnitCount,
                team,
                engine._makeMoveEvent,
                logic);

            return result;
        }

        private static void ValidatePositioning(GamePositionEventArgs e)
        {
            #region Argument Check

            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            #endregion

            for (var index = 0; index < e.UnitStates.Count; index++)
            {
                var unitState = e.UnitStates[index];

                DirectionalPosition position;
                if (!e.TryGetPosition(unitState, out position))
                {
                    throw new GameException(
                        string.Format(
                            "Chicken unit {{{0}}} has undefined position.",
                            unitState));
                }

                var tooCloseUnit = e.UnitStates.Take(index).FirstOrDefault(
                    item => e.GetPosition(item).Position.GetDistance(position.Position)
                        < GameConstants.NominalCellSize);
                if (tooCloseUnit != null)
                {
                    throw new GameException(
                        string.Format(
                            "Chicken unit {{{0}}} is too close to chicken unit {{{1}}}.",
                            unitState,
                            tooCloseUnit));
                }
            }
        }

        private static void PositionChickensDefault(GamePositionEventArgs e)
        {
            #region Argument Check

            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            #endregion

            for (var index = 0; index < e.UnitStates.Count; index++)
            {
                var unitState = e.UnitStates[index];

                Point2D position;
                do
                {
                    var nominalPosition = new Point(
                        RandomGenerator.Next(e.Data.NominalSize.Width),
                        RandomGenerator.Next(e.Data.NominalSize.Height));
                    position = GameHelper.NominalToReal(nominalPosition);
                }
                while (e.UnitStates.Take(index).Any(
                    item => e.GetPosition(item).Position.GetDistance(position) < GameConstants.NominalCellSize));

                var plainAngle = (float)Math.Floor(
                    MathHelper.HalfRevolutionDegrees - RandomGenerator.NextDouble() * MathHelper.RevolutionDegrees);
                var angle = GameAngle.FromDegrees(plainAngle);

                e.SetPosition(unitState, new DirectionalPosition(position, angle));
            }

            e.Handled = true;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void ApplyPositioning(GamePositionEventArgs e)
        {
            foreach (var unit in _allChickens)
            {
                var directionalPosition = e.GetPosition(unit);
                unit.ResetPosition(directionalPosition.Position, directionalPosition.Angle);
            }
        }

        private void PositionChickens()
        {
            UpdateUnitStates(true);  // Unit states are used in positioning
            var eventArgs = new GamePositionEventArgs(this);

            if (_positionCallback != null)
            {
                _positionCallback(eventArgs);
                if (eventArgs.Handled)
                {
                    ValidatePositioning(eventArgs);
                    ApplyPositioning(eventArgs);
                    return;
                }
            }

            eventArgs.Reset();
            PositionChickensDefault(eventArgs);
            ValidatePositioning(eventArgs);
            ApplyPositioning(eventArgs);
        }

        private void CallPaintCallback()
        {
            if (_paintCallback == null)
            {
                return;
            }

            var presentation = _lastGamePresentation.Value;
            _paintCallback(new GamePaintEventArgs(presentation));
        }

        private bool IsStopping()
        {
            return _stopEvent.WaitOne(0);
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
            for (var index = 0; index < primitives.Count; index++)
            {
                var primitive = primitives[index];
                if (!CollisionDetector.IsPointInPolygon(primitive.BasePoint, this.Data.BoardBorder))
                {
                    return true;
                }
            }

            // Then checking intersection of element's primitives with board borders
            for (var index = 0; index < primitives.Count; index++)
            {
                var primitive = primitives[index];
                for (var edgeIndex = 0; edgeIndex < this.Data.BoardBorder.Edges.Count; edgeIndex++)
                {
                    var edge = this.Data.BoardBorder.Edges[edgeIndex];
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

            if (_engineThread != null)
            {
                throw new GameException("Engine is already running.");
            }

            if (_winningTeam.Value.HasValue)
            {
                throw new GameException("The game has ended. Reset the game before starting it again.");
            }

            _engineThread = new Thread(this.DoExecuteEngine)
            {
                Name = GetType().FullName,
                IsBackground = true
            };

            _stopEvent.Reset();
            CallPaintCallback();
            _engineThread.Start();

            _logicExecutors.DoForEach(item => item.Start());
        }

        private void ResetInternal()
        {
            _moveCount.Value = 0L;
            _finalizingStage = false;
            _winningTeam.Value = null;
            lock (_shotIndexCounterLock)
            {
                _shotIndexCounter = 0;
            }

            _aliveChickensDirect.ChangeContents(_allChickens);
            _shotUnitsDirect.Clear();
            _allChickens.DoForEach(item => item.Reset());
            _previousMoves.Clear();

            PositionChickens();
            UpdateUnitStates(true);

            _logicExecutors.DoForEach(item => item.Reset());

            UpdateLastGamePresentation();
        }

        private GameObjectId GetShotUniqueId()
        {
            if (SettingsCache.Instance.DebugModeDisableShooting)
            {
                throw new GameException("Shooting is disabled.");
            }

            lock (_shotIndexCounterLock)
            {
                _shotIndexCounter++;
                return new GameObjectId(_shotIndexCounter);
            }
        }

        private void DoExecuteEngine()
        {
            var sw = new Stopwatch();

            while (!IsStopping())
            {
                if (_disposed)
                {
                    return;
                }

                if (!UpdateUnitStates(false))
                {
                    return;
                }

                _makeMoveEvent.Set();
                sw.Restart();
                while (sw.Elapsed < GameConstants.LogicPollFrequency)
                {
                    if (IsStopping())
                    {
                        return;
                    }

                    Thread.Sleep(0);
                }

                _makeMoveEvent.Reset();

                using (SettingsCache.Instance.EnableDebugOutput
                    ? new AutoStopwatch(DebugHelper.WriteLine)
                    {
                        OutputFormat = string.Format("Engine step #{0} took {{0}}.", this.MoveCount + 1)
                    }
                    : null)
                {
                    ProcessEngineStep();
                }

                // Wrap with lock
                lock (_moveCount.Lock)
                {
                    _moveCount.Value++;
                }

                if (SettingsCache.Instance.UsePerformanceCounters)
                {
                    PerformanceCounterHelper.Instance.CollisionCountPerStepBase.Increment();
                }

                if (SettingsCache.Instance.InstrumentationMode && _moveCount.Value >= InstrumentationMoveCountLimit)
                {
                    if (!_winningTeam.Value.HasValue)
                    {
                        RaiseGameEnded(GameTeam.None, null);
                    }

                    return;
                }

                if (IsStopping() || _winningTeam.Value.HasValue)
                {
                    return;
                }
            }
        }

        private void ProcessEngineStep()
        {
            var aliveChickens = this.AliveChickens
                .Where(item => !item.IsDead && item.LogicExecutor.Error == null)
                .ToList();

            _moveInfos.Clear();
            _previousMoves.Clear();
            for (var logicIndex = 0; logicIndex < _logicExecutors.Count; logicIndex++)
            {
                var logic = _logicExecutors[logicIndex];

                lock (logic.UnitsMovesLock)
                {
                    foreach (var pair in logic.UnitsMoves)
                    {
                        _moveInfos.Add(pair.Key, pair.Value);
                        _previousMoves.Add(pair.Key, pair.Value);
                    }
                }
            }

            if (!ProcessChickenUnitMoves(aliveChickens))
            {
                return;
            }

            // Processing new shot units
            var shootingMoves = _moveInfos.Where(item => !item.Key.IsDead && item.Value.FireMode != FireMode.None);
            ProcessNewShots(shootingMoves);

            #region Processing Shot Collisions

            foreach (var shotUnit in _shotUnitsDirect)
            {
                var movement = GameHelper.GetMovement(
                    shotUnit.Angle,
                    MoveDirection.MoveForward,
                    GameConstants.ShotUnit.DefaultRectilinearSpeed);

                shotUnit.SetMovement(movement);
                DebugHelper.WriteLine("Shot {{{0}}} has moved.", shotUnit);

                var hasOutOfBoardCollision = HasOutOfBoardCollision(shotUnit.GetElement());
                if (hasOutOfBoardCollision)
                {
                    shotUnit.Exploded = true;
                    DebugHelper.WriteLine("Shot {{{0}}} has exploded outside of game board.", shotUnit);
                }
            }

            var injuredChickens = new List<ChickenUnit>(aliveChickens.Count);
            for (var index = 0; index < _shotUnitsDirect.Count; index++)
            {
                var shotUnit = _shotUnitsDirect[index];

                for (var otherIndex = index + 1; otherIndex < _shotUnitsDirect.Count; otherIndex++)
                {
                    var otherShotUnit = _shotUnitsDirect[otherIndex];

                    var isCollision = CollisionDetector.CheckCollision(
                        shotUnit.GetElement(),
                        otherShotUnit.GetElement());
                    if (!isCollision)
                    {
                        continue;
                    }

                    shotUnit.Exploded = true;
                    otherShotUnit.Exploded = true;

                    DebugHelper.WriteLine(
                        "Mutual annihilation of shots {{{0}}} and {{{1}}}.",
                        shotUnit,
                        otherShotUnit);
                }

                var shotElement = shotUnit.GetElement();

                injuredChickens.Clear();
                for (var chickenIndex = 0; chickenIndex < aliveChickens.Count; chickenIndex++)
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
                    shotUnit.Exploded = true; //// TODO [vmcl] Move out of loop

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

            _aliveChickensDirect.RemoveAll(item => item.IsDead);
            _aliveChickensDirect.DoForEach(item => item.ApplyMovement());

            _shotUnitsDirect.RemoveAll(item => item.Exploded);
            _shotUnitsDirect.DoForEach(item => item.ApplyMovement());

            var aliveTeams = _aliveChickensDirect.Select(item => item.Team).Distinct().ToList();
            if (aliveTeams.Count > 1)
            {
                return;
            }

            _finalizingStage = true;
            if (_shotUnitsDirect.Any())
            {
                return;
            }

            var winningTeam = aliveTeams.SingleOrDefault();

            ChickenUnitLogic winningLogic;
            switch (winningTeam)
            {
                case GameTeam.Light:
                    winningLogic = _lightTeamLogicExecutor.Logic;
                    break;

                case GameTeam.Dark:
                    winningLogic = _darkTeamLogicExecutor.Logic;
                    break;

                default:
                    winningLogic = null;
                    break;
            }

            RaiseGameEnded(winningTeam, winningLogic);
        }

        private void ProcessNewShots(IEnumerable<KeyValuePair<ChickenUnit, MoveInfo>> shootingMoves)
        {
            if (SettingsCache.Instance.DebugModeDisableShooting)
            {
                DebugHelper.WriteLine(
                    "[{0}] Skipping any new shots since {1} is on.",
                    MethodBase.GetCurrentMethod().GetQualifiedName(),
                    Helper.GetPropertyName((SettingsCache obj) => obj.DebugModeDisableShooting));
                return;
            }

            _newShotUnits.Clear();
            foreach (var shootingMovePair in shootingMoves)
            {
                var unit = shootingMovePair.Key;

                if (!unit.CanShoot())
                {
                    DebugHelper.WriteLine("New shot from {{{0}}} has been skipped - too fast.", unit);
                    continue;
                }

                if (_finalizingStage)
                {
                    var thisShotTeam = unit.Team;
                    if (_shotUnitsDirect.All(su => su.Owner.Team == thisShotTeam))
                    {
                        DebugHelper.WriteLine(
                            "New shot from {{{0}}} has been skipped - finalizing stage and no enemy shots.",
                            unit);
                        continue;
                    }
                }

                var shot = new ShotUnit(unit, GetShotUniqueId());
                _newShotUnits.Add(shot);
                unit.ShotEngineStepIndex = _moveCount.Value;

                DebugHelper.WriteLine("New shot {{{0}}} has been made by {{{1}}}.", shot, unit);
            }

            _shotUnitsDirect.AddRange(_newShotUnits);
        }

        private bool ProcessChickenUnitMoves(IList<ChickenUnit> aliveChickens)
        {
            //// TODO: [vmcl] Use bisection to get conflicting units closer to each other
            //// TODO: [vmcl] Optimize number of collision checks!
            //// TODO: [vmcl] Divide move: eg. unit couldn't move but could turn beak or vice versa

            _moveInfoStates.Clear();
            for (var unitIndex = 0; unitIndex < aliveChickens.Count; unitIndex++)
            {
                if (IsStopping())
                {
                    return false;
                }

                var unit = aliveChickens[unitIndex];
                _moveInfoStates[unit] = MoveInfoStates.Handled;

                var moveInfo = _moveInfos.GetValueOrDefault(unit);
                if (moveInfo == null)
                {
                    continue;
                }

                DebugHelper.WriteLine(
                    "{0} is processing move {{{1}}} of chicken {{{2}}}.",
                    GetType().Name,
                    moveInfo,
                    unit);

                var movementAndNewPosition = GameHelper.GetMovementAndNewPosition(
                    unit.Position,
                    unit.BeakAngle,
                    moveInfo.MoveDirection,
                    GameConstants.ChickenUnit.DefaultRectilinearSpeed);

                var beakMovementAndNewAngle = GameHelper.GetBeakMovementAndNewAngle(unit.BeakAngle, moveInfo.BeakTurn);

                var newPositionElement = new ChickenElement(
                    movementAndNewPosition.Position,
                    beakMovementAndNewAngle.Position);
                if (HasOutOfBoardCollision(newPositionElement))
                {
                    _moveInfoStates[unit] = MoveInfoStates.RejectedBoardCollision;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with game board border.",
                        unit);
                    continue;
                }

                ChickenUnit conflictingChicken = null;
                for (var conflictingIndex = 0; conflictingIndex < aliveChickens.Count; conflictingIndex++)
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
                    _moveInfoStates[unit] = MoveInfoStates.RejectedOtherUnitCollision;
                    DebugHelper.WriteLine(
                        "Blocked collision of chicken {{{0}}} with {{{1}}}.",
                        unit,
                        conflictingChicken);
                    continue;
                }

                unit.SetMovement(movementAndNewPosition.Movement, beakMovementAndNewAngle.Movement);

                DebugHelper.WriteLine("Chicken {{{0}}} has moved.", unit);
            }

            return true;
        }

        private bool UpdateUnitStates(bool force)
        {
            for (var logicIndex = 0; logicIndex < _logicExecutors.Count; logicIndex++)
            {
                var logic = _logicExecutors[logicIndex];

                lock (logic.UnitsStatesLock)
                {
                    logic.UnitsStates.Clear();

                    for (var unitIndex = 0; unitIndex < logic.Units.Count; unitIndex++)
                    {
                        if (!force && IsStopping())
                        {
                            return false;
                        }

                        var unit = logic.Units[unitIndex];

                        var previousMove = _previousMoves.GetValueOrDefault(unit) ?? MoveInfo.Empty;
                        // ReSharper disable once RedundantArgumentDefaultValue
                        var previousMoveState = _moveInfoStates.GetValueOrDefault(unit, MoveInfoStates.Handled);
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
            _lastGamePresentation.Value = lastGamePresentation;
        }

        private void OnGameEnded(GameEndedEventArgs e)
        {
            var gameEnded = this.GameEnded;
            if (gameEnded != null)
            {
                gameEnded(this, e);
            }
        }

        private void RaiseGameEnded(GameTeam winningTeam, ChickenUnitLogic winningLogic)
        {
            UpdateLastGamePresentation();

            _winningTeam.Value = winningTeam;
            var e = new GameEndedEventArgs(winningTeam, winningLogic);
            OnGameEnded(e);
        }

        #endregion
    }
}