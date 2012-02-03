using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Common;
using SharpestBeak.Common.View;

namespace SharpestBeak.Logic.Default
{
    public sealed class AimingChickenLogic : ChickenUnitLogic
    {
        #region Protected Methods

        protected override void OnReset(GameState gameState)
        {
            // Nothing to do
        }

        protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
        {
            foreach (var unitState in gameState.UnitStates)
            {
                var list = new List<Tuple<ChickenViewData, MoveDirection, float>>();
                foreach (var otherUnit in unitState.View.Chickens.Where(item => item.Team != this.Team))
                {
                    var move = GameHelper.GetBestMoveDirection(
                        unitState.Position,
                        unitState.BeakAngle,
                        otherUnit.Position);
                    var turn = GameHelper.GetBestBeakTurn(
                        unitState.Position,
                        unitState.BeakAngle,
                        otherUnit.Position);

                    list.Add(Tuple.Create(otherUnit, move, turn));
                }

                //var best = list.FirstOrDefault(
                //    item => 
                //    //item => item.Item3.IsInRange(BeakTurn.ValueRange));
            }
        }

        #endregion
    }
}