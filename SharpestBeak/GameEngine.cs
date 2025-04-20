using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Omnifactotum;
using Omnifactotum.Annotations;
using SharpestBeak.Configuration;
using SharpestBeak.Diagnostics;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak;
//// TODO: [vmcl] Capture the game to allow playback

//// TODO: [vmcl] Allow game frame snapshot as a start of a game

//// TODO: [vmcl] Implement single-thread feature: logics are running with engine in single thread - ???

//// TODO: [vmcl] Seems that in some cases collisions are detected incorrectly (mostly chicken/chicken)

public sealed class GameEngine : IDisposable
{
    internal static readonly int InstrumentationMoveCountLimit = 500;

    private static readonly TimeSpan StopTimeout = GameConstants.LogicPollFrequency.Multiply(5);

    private readonly ReaderWriterLockSlim _syncLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly ManualResetEvent _stopEvent = new(false);
    private readonly ManualResetEventSlim _makeMoveEvent = new(false);
    private readonly List<ChickenUnit> _aliveChickensDirect;
    private readonly List<ShotUnit> _shotUnitsDirect;
    private readonly ChickenUnitLogicExecutor _lightTeamLogicExecutor;
    private readonly ChickenUnitLogicExecutor _darkTeamLogicExecutor;
    private readonly Action<GamePaintEventArgs> _paintCallback;

    [NotNull]
    private readonly Action<GamePositionEventArgs> _positionCallback;

    private readonly ThreadSafeValue<long> _moveCount;
    private readonly ThreadSafeValue<GameTeam?> _winningTeam;

    private readonly ThreadSafeValue<GamePresentation> _lastGamePresentation;

    private readonly Dictionary<ChickenUnit, MoveInfo> _moveInfos;
    private readonly Dictionary<ChickenUnit, MoveInfoStates> _moveInfoStates;
    private readonly Dictionary<ChickenUnit, MoveInfo> _previousMoves;
    private readonly List<ShotUnit> _newShotUnits;

    private readonly object _shotIndexCounterLock = new();

    private bool _disposed;

    private Thread _engineThread;
    private bool _finalizingStage;

    private int _shotIndexCounter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameEngine"/> class.
    /// </summary>
    public GameEngine(GameEngineSettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (SettingsCache.Instance.UsePerformanceCounters)
        {
            PerformanceCounterHelper.Initialize();
        }

        // Pre-initialized fields and properties
        _paintCallback = settings.PaintCallback;
        _positionCallback = settings.PositionCallback.EnsureNotNull();
        Data = new GameEngineData(settings.NominalSize);
        _moveCount = new ThreadSafeValue<long>();
        _winningTeam = new ThreadSafeValue<GameTeam?>();
        _lastGamePresentation = new ThreadSafeValue<GamePresentation>();

        // Post-initialized fields and properties
        _lightTeamLogicExecutor = CreateLogicExecutor(this, settings.LightTeamSettings, GameTeam.Light);
        _darkTeamLogicExecutor = CreateLogicExecutor(this, settings.DarkTeamSettings, GameTeam.Dark);
        LogicExecutors = new[] { _lightTeamLogicExecutor, _darkTeamLogicExecutor }.AsReadOnly();

        AllChickens = LogicExecutors.SelectMany(item => item.Units).ToArray().AsReadOnly();
        AllChickens.DoForEach((item, index) => item.UniqueId = new GameObjectId(index + 1));

        _aliveChickensDirect = [];
        AliveChickens = _aliveChickensDirect.AsReadOnly();

        _shotUnitsDirect = new List<ShotUnit>(AllChickens.Count);
        ShotUnits = _shotUnitsDirect.AsReadOnly();

        _moveInfos = new Dictionary<ChickenUnit, MoveInfo>(AllChickens.Count);
        _moveInfoStates = new Dictionary<ChickenUnit, MoveInfoStates>(AllChickens.Count);
        _previousMoves = new Dictionary<ChickenUnit, MoveInfo>(AllChickens.Count);
        _newShotUnits = new List<ShotUnit>(AllChickens.Count);

        var maxChickenCount = Data.NominalSize.Width * Data.NominalSize.Height / 2;
        if (AllChickens.Count > maxChickenCount)
        {
            throw new ArgumentException(
                $"Too many chickens ({AllChickens.Count}) for the board of nominal size {Data.NominalSize.Width}x{Data.NominalSize.Height}. Maximum is {
                    maxChickenCount}.",
                nameof(settings));
        }

        ResetInternal();
    }

    /// <summary>
    ///     Occurs when the game has ended.
    ///     NOTE: handlers of this event are called from engine thread!
    /// </summary>
    public event EventHandler<GameEndedEventArgs> GameEnded;

    public GameEngineData Data { get; }

    public long MoveCount
    {
        [DebuggerNonUserCode]
        get => _moveCount.Value;
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
        get => _winningTeam.Value;
    }

    [DebuggerNonUserCode]
    internal ReadOnlyCollection<ChickenUnit> AllChickens { get; }

    internal ReadOnlyCollection<ChickenUnit> AliveChickens { get; }

    internal ReadOnlyCollection<ShotUnit> ShotUnits { get; }

    [DebuggerNonUserCode]
    internal ReadOnlyCollection<ChickenUnitLogicExecutor> LogicExecutors { get; }

    public void Start() => _syncLock.ExecuteInWriteLock(StartInternal);

    public void Stop()
    {
        EnsureNotDisposed();

        _syncLock.ExecuteInWriteLock(
            () => LogicExecutors.DoForEach(item => item.Stop()));

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
        if (IsRunning)
        {
            throw new GameException("Cannot reset game engine while it is running.");
        }

        _syncLock.ExecuteInWriteLock(ResetInternal);
        CallPaintCallback();
    }

    public void CallPaint() => CallPaintCallback();

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

    private static ChickenUnitLogicExecutor CreateLogicExecutor(
        GameEngine engine,
        ChickenTeamSettings teamSettings,
        GameTeam team)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        if (teamSettings is null)
        {
            throw new ArgumentNullException(nameof(teamSettings));
        }

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
        if (e is null)
        {
            throw new ArgumentNullException(nameof(e));
        }

        for (var index = 0; index < e.UnitStates.Count; index++)
        {
            var unitState = e.UnitStates[index];

            if (!e.TryGetPosition(unitState, out var position))
            {
                throw new GameException($"Chicken unit {{{unitState}}} has undefined position.");
            }

            var tooCloseUnit = e.UnitStates
                .Take(index)
                .FirstOrDefault(item => e.GetPosition(item).Position.GetDistance(position.Position) < GameConstants.NominalCellSize);

            if (tooCloseUnit != null)
            {
                throw new GameException($"Chicken unit {{{unitState}}} is too close to chicken unit {{{tooCloseUnit}}}.");
            }
        }
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
        foreach (var unit in AllChickens)
        {
            var directionalPosition = e.GetPosition(unit);
            unit.ResetPosition(directionalPosition.Position, directionalPosition.Angle);
        }
    }

    private void PositionChickens()
    {
        UpdateUnitStates(true); // Unit states are used in positioning

        var eventArgs = new GamePositionEventArgs(this);
        _positionCallback(eventArgs);
        ValidatePositioning(eventArgs);
        ApplyPositioning(eventArgs);
    }

    private void CallPaintCallback()
    {
        if (_paintCallback is null)
        {
            return;
        }

        var presentation = _lastGamePresentation.Value;
        _paintCallback(new GamePaintEventArgs(presentation));
    }

    private bool IsStopping() => _stopEvent.WaitOne(0);

    private bool HasOutOfBoardCollision(ICollidableElement element)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        var primitives = element.GetPrimitives();

        // First, checking just base points
        //// ReSharper disable once LoopCanBeConvertedToQuery
        //// ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < primitives.Count; index++)
        {
            var primitive = primitives[index];
            if (!CollisionDetector.IsPointInPolygon(primitive.BasePoint, Data.BoardBorder))
            {
                return true;
            }
        }

        // Then checking intersection of element's primitives with board borders
        //// ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < primitives.Count; index++)
        {
            var primitive = primitives[index];

            //// ReSharper disable once LoopCanBeConvertedToQuery
            //// ReSharper disable once ForCanBeConvertedToForeach
            for (var edgeIndex = 0; edgeIndex < Data.BoardBorder.Edges.Count; edgeIndex++)
            {
                var edge = Data.BoardBorder.Edges[edgeIndex];
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

        _engineThread = new Thread(DoExecuteEngine)
        {
            Name = GetType().FullName,
            IsBackground = true
        };

        _stopEvent.Reset();
        CallPaintCallback();
        _engineThread.Start();

        LogicExecutors.DoForEach(item => item.Start());
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

        _aliveChickensDirect.ChangeContents(AllChickens);
        _shotUnitsDirect.Clear();
        AllChickens.DoForEach(item => item.Reset());
        _previousMoves.Clear();

        PositionChickens();
        UpdateUnitStates(true);

        LogicExecutors.DoForEach(item => item.Reset());

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
                           OutputFormat = $"Engine step #{MoveCount + 1} took {{0}}."
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
        var aliveChickens = AliveChickens
            .Where(item => !item.IsDead && item.LogicExecutor.Error is null)
            .ToList();

        _moveInfos.Clear();
        _previousMoves.Clear();
        for (var logicIndex = 0; logicIndex < LogicExecutors.Count; logicIndex++)
        {
            var logic = LogicExecutors[logicIndex];

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

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
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

        var winningLogic = winningTeam switch
        {
            GameTeam.Light => _lightTeamLogicExecutor.Logic,
            GameTeam.Dark => _darkTeamLogicExecutor.Logic,
            _ => null
        };

        RaiseGameEnded(winningTeam, winningLogic);
    }

    private void ProcessNewShots(IEnumerable<KeyValuePair<ChickenUnit, MoveInfo>> shootingMoves)
    {
        if (SettingsCache.Instance.DebugModeDisableShooting)
        {
            DebugHelper.WriteLine(
                "[{0}] Skipping any new shots since {1} is on.",
                MethodBase.GetCurrentMethod()?.GetQualifiedName(),
                Factotum.For<SettingsCache>.GetPropertyName(obj => obj.DebugModeDisableShooting));

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
            if (moveInfo is null)
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

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
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
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var logicIndex = 0; logicIndex < LogicExecutors.Count; logicIndex++)
        {
            var logic = LogicExecutors[logicIndex];

            lock (logic.UnitsStatesLock)
            {
                logic.UnitsStates.Clear();

                // ReSharper disable once ForCanBeConvertedToForeach
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

    private void OnGameEnded(GameEndedEventArgs e) => GameEnded?.Invoke(this, e);

    private void RaiseGameEnded(GameTeam winningTeam, ChickenUnitLogic winningLogic)
    {
        UpdateLastGamePresentation();

        _winningTeam.Value = winningTeam;
        var e = new GameEndedEventArgs(winningTeam, winningLogic);
        OnGameEnded(e);
    }
}