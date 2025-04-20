using System;
using System.ComponentModel;
using System.Reflection;
using Omnifactotum.Annotations;

namespace SharpestBeak.Model;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class ChickenUnitLogic
{
    public GameTeam Team { get; private set; }

    public static string GetCaption(Type logicType)
    {
        if (logicType is null)
        {
            throw new ArgumentNullException(nameof(logicType));
        }

        if (!typeof(ChickenUnitLogic).IsAssignableFrom(logicType))
        {
            throw new ArgumentException("The specified type is not inherited properly.", nameof(logicType));
        }

        var descriptionAttribute = logicType.GetSingleOrDefaultCustomAttribute<DescriptionAttribute>(false);
        return descriptionAttribute is null ? logicType.Name : descriptionAttribute.Description;
    }

    internal void Reset(GameState gameState)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        Team = gameState.Team;

        OnReset(gameState);
    }

    internal void MakeMove(GameState gameState, LogicMoveResult moves)
    {
        if (gameState is null)
        {
            throw new ArgumentNullException(nameof(gameState));
        }

        if (moves is null)
        {
            throw new ArgumentNullException(nameof(moves));
        }

        OnMakeMove(gameState, moves);
    }

    protected abstract void OnReset(GameState gameState);

    protected abstract void OnMakeMove(GameState gameState, LogicMoveResult moves);
}