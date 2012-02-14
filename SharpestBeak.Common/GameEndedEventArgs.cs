using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameEndedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEndedEventArgs"/> class.
        /// </summary>
        internal GameEndedEventArgs(GameTeam winningTeam, ChickenUnitLogic winningLogic)
        {
            #region Argument Check

            if (winningLogic == null ^ winningTeam == GameTeam.None)
            {
                throw new ArgumentException("Invalid combination of argument values.");
            }

            #endregion

            this.WinningTeam = winningTeam;
            this.WinningLogic = winningLogic;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the team that has won the game or <see cref="GameTeam.None"/>, if there is a draw,
        ///     that is, both team units got killed.
        /// </summary>
        public GameTeam WinningTeam
        {
            get;
            private set;
        }

        public ChickenUnitLogic WinningLogic
        {
            get;
            private set;
        }

        #endregion
    }
}