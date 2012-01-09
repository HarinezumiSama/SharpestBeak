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

        private GamePoint m_targetPoint;

        #endregion

        #region Constructors

        public RandomChickenLogic()
        {
            m_targetPoint = new GamePoint(-GameConstants.NominalCellSize, -GameConstants.NominalCellSize);
        }

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

        protected override void OnInitialize()
        {
            m_targetPoint = this.Unit.Position;
        }

        protected override MoveInfo OnMakeMove(GameState state)
        {
            if (this.Unit.Position.GetDistance(m_targetPoint)
                .IsZero(GameConstants.ChickenUnit.DefaultRectilinearSpeed
                    * (float)GameConstants.LogicPollFrequency.TotalSeconds))
            {
                // Choosing new target point
                var point = new GamePoint(
                    GetNextRandom(state.Data.NominalSize.Width),
                    GetNextRandom(state.Data.NominalSize.Height));
                m_targetPoint = point * GameConstants.NominalCellSize
                    + new GamePoint(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);
            }

            var move = Tuple.Create(MoveDirection.None, float.MaxValue);
            foreach (var item in s_moveDirections)
            {
                var potentialMovePoint = GameHelper.GetNewPosition(
                    this.Unit.Position,
                    this.Unit.BeakAngle,
                    item,
                    GameConstants.ChickenUnit.DefaultRectilinearSpeed,
                    (float)GameConstants.LogicPollFrequency.TotalSeconds);
                var distanceSquared = m_targetPoint.GetDistanceSquared(potentialMovePoint);
                if (distanceSquared < move.Item2)
                {
                    move = Tuple.Create(item, distanceSquared);
                }
            }

            var targetOffset = m_targetPoint - this.Unit.Position.ToSizeF();
            var targetAngle = GameAngle.FromRadians((float)Math.Atan2(targetOffset.Y, targetOffset.X));
            var turn = GameHelper.GetBeakTurn(this.Unit.BeakAngle, targetAngle);

            return new MoveInfo(move.Item1, turn, GetNextRandom(10) >= 9 ? FireMode.Single : FireMode.None);
        }

        #endregion

        #region Public Properties

        // For debug only
        public GamePoint TargetPoint
        {
            [DebuggerStepThrough]
            get
            {
                return m_targetPoint;
            }
        }

        #endregion
    }
}