﻿using System;
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

        #endregion

        #region Private Methods

        private static T ChooseRandomValue<T>(IList<T> collection)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            #endregion

            return collection[s_random.Next(collection.Count)];
        }

        #endregion

        #region Protected Methods

        protected override MoveInfo OnMakeMove()
        {
            //var peckPossibilities = new List<MoveInfo>(s_turns.Length);
            //foreach (var beakTurn in s_turns)
            //{
            //    var attackPoint = this.Board.GetPeckAttackPoint(this.Unit, beakTurn);
            //    if (attackPoint.HasValue && this.Board.GetChickenAtPoint(attackPoint.Value) != null)
            //    {
            //        peckPossibilities.Add(new MoveInfo(beakTurn, MoveDirection.Peck));
            //    }
            //}
            //if (peckPossibilities.Count > 0)
            //{
            //    var index = s_random.Next(peckPossibilities.Count);
            //    return peckPossibilities[index];
            //}

            return new MoveInfo(
                ChooseRandomValue(s_moveDirections),
                ChooseRandomValue(s_turns),
                ChooseRandomValue(s_fireModes));
        }

        #endregion
    }
}