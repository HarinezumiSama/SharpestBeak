using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

        private readonly Dictionary<int, Point2D> m_targetPoints = new Dictionary<int, Point2D>();
        private readonly object m_targetPointsLock = new object();

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

        protected override void OnReset()
        {
            lock (m_targetPointsLock)
            {
                m_targetPoints.Clear();
            }
        }

        protected override void OnInitialize()
        {
            lock (m_targetPointsLock)
            {
                m_targetPoints.Clear();
            }
        }

        protected override void OnMakeMove(GameState state)
        {
            foreach (var unitState in state.UnitStates)
            {
                Point2D targetPoint;
                lock (m_targetPointsLock)
                {
                    targetPoint = m_targetPoints.GetValueOrDefault(unitState.UniqueIndex, unitState.Position);
                }

                var needNewTargetPoint =
                    (unitState.PreviousMove != null
                        && unitState.PreviousMove.State == MoveInfoState.Rejected)
                    || unitState.Position.GetDistance(targetPoint)
                        .IsZero(GameConstants.ChickenUnit.DefaultRectilinearSpeed
                            * (float)GameConstants.LogicPollFrequency.TotalSeconds);
                if (needNewTargetPoint)
                {
                    // Choosing new target point
                    var point = new Point2D(
                        GetNextRandom(state.Data.NominalSize.Width),
                        GetNextRandom(state.Data.NominalSize.Height));
                    targetPoint = point * GameConstants.NominalCellSize
                        + new Vector2D(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);
                    lock (m_targetPointsLock)
                    {
                        m_targetPoints[unitState.UniqueIndex] = targetPoint;
                    }
                }

                var move = Tuple.Create(MoveDirection.None, float.MaxValue);
                foreach (var item in s_moveDirections)
                {
                    var potentialMovePoint = GameHelper.GetNewPosition(
                        unitState.Position,
                        unitState.BeakAngle,
                        item,
                        GameConstants.ChickenUnit.DefaultRectilinearSpeed,
                        (float)GameConstants.LogicPollFrequency.TotalSeconds);
                    var distanceSquared = targetPoint.GetDistanceSquared(potentialMovePoint);
                    if (distanceSquared < move.Item2)
                    {
                        move = Tuple.Create(item, distanceSquared);
                    }
                }

                var targetOffset = targetPoint - unitState.Position;
                var targetAngle = GameAngle.FromRadians((float)Math.Atan2(targetOffset.Y, targetOffset.X));
                var turn = GameHelper.GetBeakTurn(unitState.BeakAngle, targetAngle);

                unitState.CurrentMove = new MoveInfo(
                    move.Item1,
                    turn,
                    GetNextRandom(10) >= 9 ? FireMode.Single : FireMode.None);
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
                    return m_targetPoints.Values.ToArray();
                }
            }
        }

        #endregion
    }
}