using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Common;

namespace SharpestBeak.Logic.Default
{
    public sealed class RandomChickenLogic : ChickenUnitLogic
    {
        #region Fields

        private static readonly IList<BeakTurn> s_turns = Helper.GetEnumValues<BeakTurn>().AsReadOnly();
        private static readonly IList<MoveDirection> s_moveDirections =
            Helper.GetEnumValues<MoveDirection>().AsReadOnly();
        private static readonly IList<FireMode> s_fireModes = Helper.GetEnumValues<FireMode>().AsReadOnly();

        private static readonly ThreadSafeRandom s_random = new ThreadSafeRandom();

        private readonly Dictionary<int, Point2D> m_unitIdToTargetPointMap = new Dictionary<int, Point2D>();
        private readonly object m_targetPointsLock = new object();

        #endregion

        #region Private Methods

        private Point2D ChooseTargetPoint(GameState state, ChickenUnitState unitState)
        {
            var point = new Point(
                s_random.Next(state.Data.NominalSize.Width),
                s_random.Next(state.Data.NominalSize.Height));
            var result = GameHelper.NominalToReal(point);
            lock (m_targetPointsLock)
            {
                m_unitIdToTargetPointMap[unitState.UniqueId] = result;
            }
            return result;
        }

        #endregion

        #region Protected Methods

        protected override void OnReset(GameState gameState)
        {
            lock (m_targetPointsLock)
            {
                m_unitIdToTargetPointMap.Clear();
                gameState.UnitStates.DoForEach(item => ChooseTargetPoint(gameState, item));
            }
        }

        protected override void OnMakeMove(GameState gameState)
        {
            foreach (var unitState in gameState.UnitStates)
            {
                if (unitState.IsDead)
                {
                    lock (m_targetPointsLock)
                    {
                        m_unitIdToTargetPointMap.Remove(unitState.UniqueId);
                    }
                    continue;
                }

                Point2D targetPoint;
                lock (m_targetPointsLock)
                {
                    targetPoint = m_unitIdToTargetPointMap.GetValueOrDefault(unitState.UniqueId, unitState.Position);
                }

                var needNewTargetPoint =
                    (unitState.PreviousMove != null
                        && unitState.PreviousMove.State == MoveInfoState.Rejected)
                    || unitState.Position.GetDistance(targetPoint)
                        .IsZero(GameConstants.ChickenUnit.DefaultRectilinearSpeed * GameConstants.StepTimeDelta);
                if (needNewTargetPoint)
                {
                    targetPoint = ChooseTargetPoint(gameState, unitState);
                }

                var move = Tuple.Create(MoveDirection.None, float.MaxValue);
                foreach (var item in s_moveDirections)
                {
                    var potentialMovePoint = GameHelper.GetNewPosition(
                        unitState.Position,
                        unitState.BeakAngle,
                        item,
                        GameConstants.ChickenUnit.DefaultRectilinearSpeed,
                        GameConstants.StepTimeDelta);
                    var distanceSquared = targetPoint.GetDistanceSquared(potentialMovePoint);
                    if (distanceSquared < move.Item2)
                    {
                        move = Tuple.Create(item, distanceSquared);
                    }
                }

                var targetOffset = targetPoint - unitState.Position;
                var targetAngle = GameAngle.FromRadians((float)Math.Atan2(targetOffset.Y, targetOffset.X));
                var turn = GameHelper.GetBeakTurn(unitState.BeakAngle, targetAngle);
                var fireMode = unitState.CanShoot() && s_random.Next(10) == 0 ? FireMode.Regular : FireMode.None;

                var moveInfo = new MoveInfo(move.Item1, turn, fireMode);
                unitState.SetCurrentMove(moveInfo);
            }
        }

        #endregion

        #region Public Properties

        // For debug only
        public Point2D[] TargetPoints
        {
            [DebuggerStepThrough]
            get
            {
                lock (m_targetPointsLock)
                {
                    return m_unitIdToTargetPointMap.Values.ToArray();
                }
            }
        }

        #endregion
    }
}