using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SharpestBeak.Model
{
    public abstract class ChickenUnitLogic
    {
        #region Public Properties

        public GameTeam Team
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public static string GetCaption(Type logicType)
        {
            #region Argument Check

            if (logicType == null)
            {
                throw new ArgumentNullException("logicType");
            }

            if (!typeof(ChickenUnitLogic).IsAssignableFrom(logicType))
            {
                throw new ArgumentException("The specified type is not inherited properly.", "logicType");
            }

            #endregion

            var descriptionAttribute = logicType.GetSingleOrDefaultCustomAttribute<DescriptionAttribute>(false);
            return descriptionAttribute == null ? logicType.Name : descriptionAttribute.Description;
        }

        #endregion

        #region Protected Internal Methods

        internal void Reset(GameState gameState)
        {
            #region Argument Check

            if (gameState == null)
            {
                throw new ArgumentNullException("gameState");
            }

            #endregion

            this.Team = gameState.Team;

            OnReset(gameState);
        }

        internal void MakeMove(GameState gameState, LogicMoveResult moves)
        {
            #region Argument Check

            if (gameState == null)
            {
                throw new ArgumentNullException("gameState");
            }

            if (moves == null)
            {
                throw new ArgumentNullException("moves");
            }

            #endregion

            OnMakeMove(gameState, moves);
        }

        #endregion

        #region Protected Methods

        protected abstract void OnReset(GameState gameState);

        protected abstract void OnMakeMove(GameState gameState, LogicMoveResult moves);

        #endregion
    }
}