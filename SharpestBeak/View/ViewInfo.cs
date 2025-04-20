using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using SharpestBeak.Model;

namespace SharpestBeak.View;

[DebuggerDisplay("{GetType().Name,nq}. Chickens.Count = {Chickens.Count}, Shots.Count = {Shots.Count}")]
public sealed class ViewInfo
{
    private static readonly ReadOnlyCollection<ChickenViewData> EmptyChickens = Array.Empty<ChickenViewData>().AsReadOnly();

    private static readonly ReadOnlyCollection<ShotViewData> EmptyShots = Array.Empty<ShotViewData>().AsReadOnly();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ViewInfo"/> class.
    /// </summary>
    internal ViewInfo(ChickenUnit unit)
    {
        if (unit is null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        var engine = unit.LogicExecutor.Engine;
        var unitTeam = unit.Team;

        if (unit.IsDead)
        {
            Chickens = EmptyChickens;
            Shots = EmptyShots;
        }
        else
        {
            var chickens = new List<ChickenViewData>(unit.IsDead ? 0 : engine.AliveChickens.Count);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var aliveChicken in engine.AliveChickens)
            {
                if (!aliveChicken.IsDead
                    && aliveChicken != unit
                    && (aliveChicken.Team == unitTeam || unit.CanSee(aliveChicken.Position)))
                {
                    chickens.Add(new ChickenViewData(aliveChicken));
                }
            }

            Chickens = chickens.AsReadOnly();

            var shots = new List<ShotViewData>(unit.IsDead ? 0 : engine.ShotUnits.Count);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var shotUnit in engine.ShotUnits)
            {
                if (unit.CanSee(shotUnit.Position))
                {
                    shots.Add(new ShotViewData(shotUnit));
                }
            }

            Shots = shots.AsReadOnly();
        }
    }

    public ReadOnlyCollection<ChickenViewData> Chickens { get; }

    public ReadOnlyCollection<ShotViewData> Shots { get; }
}