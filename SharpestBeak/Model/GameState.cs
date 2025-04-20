using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpestBeak.Model;

public sealed class GameState
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameState"/> class.
    /// </summary>
    internal GameState(GameEngine engine, GameTeam team, ICollection<ChickenUnitState> unitStates)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        if (unitStates is null)
        {
            throw new ArgumentNullException(nameof(unitStates));
        }

        if (unitStates.Any(item => item is null))
        {
            throw new ArgumentException("The collection contains a null element.", nameof(unitStates));
        }

        Data = engine.Data;
        Team = team;
        UnitStates = unitStates.ToArray().AsReadOnly();
    }

    public GameEngineData Data { get; }

    public GameTeam Team { get; }

    public ReadOnlyCollection<ChickenUnitState> UnitStates { get; }
}