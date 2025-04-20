using System;

namespace SharpestBeak.Configuration;

public sealed class ChickenTeamSettings
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenTeamSettings"/> class.
    /// </summary>
    public ChickenTeamSettings(Type type, int unitCount)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!unitCount.IsInRange(GameConstants.TeamPlayerUnitCountRange))
        {
            throw new ArgumentOutOfRangeException(
                nameof(unitCount),
                unitCount,
                $"The number of units must be in the range {GameConstants.TeamPlayerUnitCountRange}.");
        }

        Type = type;
        UnitCount = unitCount;
    }

    public Type Type { get; }

    public int UnitCount { get; }
}