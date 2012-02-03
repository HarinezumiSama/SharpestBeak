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
            foreach (var unitState in gameState.UnitStates.Where(item => !item.IsDead))
            {
                var list = new List<Tuple<ChickenViewData, MoveDirection, float, float>>();
                foreach (var otherUnit in unitState.View.Chickens.Where(item => item.Team != this.Team))
                {
                    var moveDirection = GameHelper.GetBestMoveDirection(
                        unitState.Position,
                        unitState.BeakAngle,
                        otherUnit.Position);
                    var turn = GameHelper.GetBestBeakTurn(
                        unitState.Position,
                        unitState.BeakAngle,
                        otherUnit.Position);

                    list.Add(
                        Tuple.Create(
                            otherUnit,
                            moveDirection,
                            turn,
                            unitState.Position.GetDistanceSquared(otherUnit.Position)));
                }

                var best = list
                    .OrderBy(item => item.Item4)
                    .ThenBy(item => item.Item3)
                    .FirstOrDefault();

                MoveInfo move;
                if (best == null)
                {
                    move = new MoveInfo(
                        unitState.PreviousMove == null || unitState.PreviousMove.State != MoveInfoState.Rejected
                            ? MoveDirection.MoveForward
                            : MoveDirection.None,
                        BeakTurn.FullyClockwise,
                        FireMode.None);
                }
                else
                {
                    move = new MoveInfo(
                        best.Item2,
                        GameHelper.NormalizeBeakTurn(best.Item3),
                        FireMode.Regular);
                }

                moves.Set(unitState, move);
            }
        }

        #endregion
    }
}