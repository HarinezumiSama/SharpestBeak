using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common;

namespace SharpestBeak.UI.WinForms
{
    public sealed class RandomChickenLogic : ChickenUnitLogic
    {
        #region Fields

        private static readonly BeakTurn[] s_turns = (BeakTurn[])Enum.GetValues(typeof(BeakTurn));
        private static readonly MoveDirection[] s_moveDirections =
            (MoveDirection[])Enum.GetValues(typeof(MoveDirection));
        private static readonly FireMode[] s_fireModes = (FireMode[])Enum.GetValues(typeof(FireMode));

        private static readonly Random s_random = new Random();
        private static readonly object s_randomSyncLock = new object();

        #endregion

        #region Private Methods

        private static int GetNextRandom(int maxValue)
        {
            lock (s_randomSyncLock)
            {
                return s_random.Next(maxValue);
            }
        }

        private static T ChooseRandomValue<T>(IList<T> collection)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            #endregion

            return collection[GetNextRandom(collection.Count)];
        }

        #endregion

        #region Protected Methods

        protected override MoveInfo OnMakeMove(GameState state)
        {
            return new MoveInfo(
                ChooseRandomValue(s_moveDirections),
                ChooseRandomValue(s_turns),
                ChooseRandomValue(s_fireModes));
        }

        #endregion
    }
}