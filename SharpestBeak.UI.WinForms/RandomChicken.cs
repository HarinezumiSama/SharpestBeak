using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common;

namespace SharpestBeak.UI.WinForms
{
    public sealed class RandomChicken : ChickenUnit
    {
        #region Fields

        private static readonly BeakTurn[] s_turns = (BeakTurn[])Enum.GetValues(typeof(BeakTurn));
        private static readonly MoveAction[] s_actions = (MoveAction[])Enum.GetValues(typeof(MoveAction));
        private static readonly Random s_random = new Random();

        #endregion

        #region Private Methods

        private static T ChooseRandomValue<T>(T[] array)
        {
            return array[s_random.Next(array.Length)];
        }

        #endregion

        #region Protected Methods

        protected override MoveInfo OnMakeMove()
        {
            var attackPoint = this.Board.GetPeckAttackPoint(this);
            if (attackPoint.HasValue && this.Board.GetChickenAtPoint(attackPoint.Value) != null)
            {
                return new MoveInfo(BeakTurn.None, MoveAction.Peck);
            }

            return new MoveInfo(ChooseRandomValue(s_turns), ChooseRandomValue(s_actions));
        }

        #endregion
    }
}