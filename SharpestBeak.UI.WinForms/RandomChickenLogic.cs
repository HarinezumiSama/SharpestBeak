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

        private PointF m_targetPoint;

        #endregion

        #region Constructors

        public RandomChickenLogic()
        {
            m_targetPoint = new PointF(-GameConstants.NominalCellSize, -GameConstants.NominalCellSize);
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
                .IsZero(GameConstants.ChickenUnit.DefaultSpeed * (float)GameConstants.LogicPollFrequency.TotalSeconds))
            {
                // Choosing new target point
                var point = new Point(
                    GetNextRandom(state.Data.NominalSize.Width),
                    GetNextRandom(state.Data.NominalSize.Height));
                m_targetPoint = ((PointF)point).Scale(GameConstants.NominalCellSize)
                    + new SizeF(GameConstants.NominalCellSize / 2f, GameConstants.NominalCellSize / 2f);
            }

            var targetOffset = m_targetPoint - this.Unit.Position.ToSizeF();
            var move = Math.Abs(targetOffset.X) > Math.Abs(targetOffset.Y)
                ? targetOffset.X.MapValueSign(MoveDirection.None, MoveDirection.MoveLeft, MoveDirection.MoveRight)
                : targetOffset.Y.MapValueSign(MoveDirection.None, MoveDirection.MoveUp, MoveDirection.MoveDown);

            var targetAngle = GameHelper.ToDegrees((float)Math.Atan2(targetOffset.Y, targetOffset.X));
            var turn = GameHelper.GetBeakTurn(this.Unit.BeakAngle, targetAngle);

            return new MoveInfo(move, turn, GetNextRandom(10) >= 9 ? FireMode.Single : FireMode.None);
        }

        #endregion

        #region Public Properties

        // For debug only
        public PointF TargetPoint
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