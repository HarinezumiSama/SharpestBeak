using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Omnifactotum.Annotations;
using SharpestBeak.Diagnostics;

namespace SharpestBeak.Model;

internal sealed class ChickenUnitLogicExecutor : IDisposable
{
    private readonly ManualResetEventSlim _makeMoveEvent;
    private readonly ThreadSafeValue<Exception> _error;
    private Thread _thread;

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
        if (unitCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitCount), unitCount, "The value must be positive.");
        }

        if (team == GameTeam.None)
        {
            throw new ArgumentException("The team must be specific.", nameof(team));
        }

        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        Team = team;
        _makeMoveEvent = makeMoveEvent ?? throw new ArgumentNullException(nameof(makeMoveEvent));
        Logic = logic ?? throw new ArgumentNullException(nameof(logic));

        var unitsDirect = Enumerable.Range(1, unitCount).Select(_ => new ChickenUnit(this)).ToList();

        _error = new ThreadSafeValue<Exception>();

        Units = unitsDirect.AsReadOnly();

        UnitsStates = new Dictionary<ChickenUnit, ChickenUnitState>(unitsDirect.Count);
        UnitsStatesLock = new object();

        UnitsMoves = new Dictionary<ChickenUnit, MoveInfo>(unitsDirect.Count);
        UnitsMovesLock = new object();
    }

    public GameEngine Engine { get; }

    public GameTeam Team { get; }

    public ChickenUnitLogic Logic { get; }

    public Exception Error
    {
        [DebuggerNonUserCode]
        get => _error.Value;

        [DebuggerNonUserCode]
        [UsedImplicitly]
        set => _error.Value = value;
    }

    public ReadOnlyCollection<ChickenUnit> Units { get; }

    public Dictionary<ChickenUnit, ChickenUnitState> UnitsStates { get; }

    public object UnitsStatesLock { get; }

    public Dictionary<ChickenUnit, MoveInfo> UnitsMoves { get; }

    public object UnitsMovesLock { get; }

    public void Start()
    {
        if (_thread != null)
        {
            throw new InvalidOperationException("Already started.");
        }

        _thread = new Thread(DoExecuteLogic)
        {
            IsBackground = true,
            Name = $"{GetType().Name}: {Team} [{Logic.GetType().Name}]"
        };

        _thread.Start();
    }

    public void Stop()
    {
        if (_thread is null)
        {
            return;
        }

        _thread.Abort();
        _thread.Join();

        _thread = null;
    }

    public void Reset()
    {
        Error = null;

        var gameState = GetGameState();
        Logic.Reset(gameState);
    }

    public LogicMoveResult MakeMove()
    {
        var gameState = GetGameState();
        var result = new LogicMoveResult(gameState.UnitStates.Count);
        Logic.MakeMove(gameState, result);
        return result;
    }

    public void Dispose() => Stop();

    private GameState GetGameState()
    {
        GameState result;
        lock (UnitsStatesLock)
        {
            result = new GameState(Engine, Team, UnitsStates.Values);
        }

        return result;
    }

    private void DoExecuteLogic()
    {
        while (true)
        {
            if (SettingsCache.Instance.InstrumentationMode && Engine.MoveCount >= GameEngine.InstrumentationMoveCountLimit)
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

                lock (UnitsMovesLock)
                {
                    UnitsMoves.Clear();
                    foreach (var movePair in moveResult.InnerMap)
                    {
                        DebugHelper.WriteLine(
                            "[Logic '{0}'] Chicken {{{1}}} is making move {{{2}}}.",
                            Logic.GetType().Name,
                            movePair.Key,
                            movePair.Value is null ? "NONE" : movePair.Value.ToString());

                        UnitsMoves.Add(movePair.Key, movePair.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                Error = ex;

                Units.DoForEach(
                    item =>
                    {
                        item.IsDead = true;
                        DebugHelper.WriteLine(
                            "Chicken #{0} is now dead since the logic '{1}' has caused an error:{2}{3}",
                            item.UniqueId,
                            Logic.GetType().FullName,
                            Environment.NewLine,
                            ex);
                    });

                return;
            }
        }
    }
}