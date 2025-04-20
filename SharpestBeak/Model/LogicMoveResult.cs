using System;
using System.Collections.Generic;

namespace SharpestBeak.Model;

public sealed class LogicMoveResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LogicMoveResult"/> class.
    /// </summary>
    internal LogicMoveResult(int capacity) => InnerMap = new Dictionary<ChickenUnit, MoveInfo>(capacity);

    internal Dictionary<ChickenUnit, MoveInfo> InnerMap { get; }

    public void Set(ChickenUnitState unitState, MoveInfo move)
    {
        if (unitState is null)
        {
            throw new ArgumentNullException(nameof(unitState));
        }

        if (move is null || unitState.IsDead)
        {
            InnerMap.Remove(unitState.Unit);
        }
        else
        {
            InnerMap[unitState.Unit] = move;
        }
    }

    public MoveInfo Get(ChickenUnitState unitState)
    {
        if (unitState is null)
        {
            throw new ArgumentNullException(nameof(unitState));
        }

        return InnerMap.GetValueOrDefault(unitState.Unit);
    }
}