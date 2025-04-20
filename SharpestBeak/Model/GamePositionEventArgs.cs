using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpestBeak.Physics;

namespace SharpestBeak.Model;

public sealed class GamePositionEventArgs : EventArgs
{
    private readonly Dictionary<ChickenUnit, DirectionalPosition> _unitToPositionMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePositionEventArgs"/> class.
    /// </summary>
    internal GamePositionEventArgs(GameEngine engine)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        Data = engine.Data;

        var unitStatesProxy = new List<ChickenUnitState>(engine.AllChickens.Count);
        UnitStates = unitStatesProxy.AsReadOnly();
        foreach (var logic in engine.LogicExecutors)
        {
            lock (logic.UnitsStatesLock)
            {
                unitStatesProxy.AddRange(logic.UnitsStates.Values);
            }
        }

        _unitToPositionMap = new Dictionary<ChickenUnit, DirectionalPosition>();
    }

    public GameEngineData Data { get; }

    public ReadOnlyCollection<ChickenUnitState> UnitStates { get; }

    public bool TryGetPosition(ChickenUnitState unitState, out DirectionalPosition position)
    {
        if (unitState is null)
        {
            throw new ArgumentNullException(nameof(unitState));
        }

        return _unitToPositionMap.TryGetValue(unitState.Unit, out position);
    }

    public DirectionalPosition GetPosition(ChickenUnitState unitState)
    {
        if (unitState is null)
        {
            throw new ArgumentNullException(nameof(unitState));
        }

        return _unitToPositionMap[unitState.Unit];
    }

    public void SetPosition(ChickenUnitState unitState, DirectionalPosition position)
    {
        if (unitState is null)
        {
            throw new ArgumentNullException(nameof(unitState));
        }

        _unitToPositionMap[unitState.Unit] = position;
    }

    internal void Reset() => _unitToPositionMap.Clear();

    internal DirectionalPosition GetPosition(ChickenUnit unit)
    {
        if (unit is null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        return _unitToPositionMap[unit];
    }
}