using System;
using SharpestBeak.Model;

namespace SharpestBeak;

public sealed class GameEndedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameEndedEventArgs"/> class.
    /// </summary>
    internal GameEndedEventArgs(GameTeam winningTeam, ChickenUnitLogic winningLogic)
    {
        if (winningLogic is null ^ winningTeam == GameTeam.None)
        {
            throw new ArgumentException("Invalid combination of argument values.");
        }

        WinningTeam = winningTeam;
        WinningLogic = winningLogic;
    }

    /// <summary>
    ///     Gets the team that has won the game or <see cref="GameTeam.None"/>, if there is a draw,
    ///     that is, both team units got killed.
    /// </summary>
    public GameTeam WinningTeam { get; }

    public ChickenUnitLogic WinningLogic { get; }
}