using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Common;
using SharpestBeak.Common.View;

namespace SharpestBeak.Logic.Default
{
    public sealed class AimingChickenLogic : ChickenUnitLogic
    {
        #region Fields

        private readonly Dictionary<int, HashSet<MoveDirection>> m_blockedDirectionMap =
            new Dictionary<int, HashSet<MoveDirection>>();

        #endregion

        #region Protected Methods

        protected override void OnReset(GameState gameState)
        {
            // Nothing to do
        }

        protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
        {
            foreach (var unitState in gameState.UnitStates.Where(item => !item.IsDead))
            {
                if (unitState.PreviousMoveState.IsRejected())
                {
                    var blockedDirections = m_blockedDirectionMap.GetValueOrDefault(unitState.UniqueId);
                    if (blockedDirections == null)
                    {
                        blockedDirections = new HashSet<MoveDirection>();
                        m_blockedDirectionMap[unitState.UniqueId] = blockedDirections;
                    }
                    blockedDirections.Add(unitState.PreviousMove.MoveDirection);

                    var moveDirection = GameHelper
                        .ActiveMoveDirections
                        .FirstOrDefault(item => !blockedDirections.Contains(item));
                    var unblockingMove = moveDirection == MoveDirection.None
                        ? new MoveInfo(MoveDirection.None, BeakTurn.FullyClockwise, FireMode.None)
                        : new MoveInfo(moveDirection, BeakTurn.None, FireMode.None);
                    moves.Set(unitState, unblockingMove);
                    continue;
                }
                m_blockedDirectionMap.Remove(unitState.UniqueId);

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
                        unitState.PreviousMoveState.IsRejected()
                            ? MoveDirection.None
                            : MoveDirection.MoveForward,
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

        protected override string GetCaption()
        {
            return "Aiming Chicken Logic";
        }

        #endregion
    }
}