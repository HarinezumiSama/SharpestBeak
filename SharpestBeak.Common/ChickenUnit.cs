using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using SharpestBeak.Common.Presentation.Elements;

namespace SharpestBeak.Common
{
    internal sealed class ChickenUnit
    {
        #region Fields

        private Point2D m_position;
        private GameAngle m_beakAngle;
        private ChickenElement m_cachedElement;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnit"/> class.
        /// </summary>
        internal ChickenUnit(ChickenUnitLogic logic)
        {
            #region Argument Check

            if (logic == null)
            {
                throw new ArgumentNullException("logic");
            }

            #endregion

            this.Logic = logic;
            this.Engine = logic.Engine.EnsureNotNull();
            this.Team = logic.Team;

            Reset();
        }

        #endregion

        #region Private Methods

        private void ResetCachedElement()
        {
            m_cachedElement = null;
        }

        #endregion

        #region Internal Properties

        internal ChickenUnitLogic Logic
        {
            get;
            private set;
        }

        internal GameEngine Engine
        {
            get;
            private set;
        }

        internal long ShotEngineStepIndex
        {
            get;
            set;
        }

        #endregion

        #region Internal Methods

        internal void Reset()
        {
            this.IsDead = false;
            this.ShotEngineStepIndex = -1;
        }

        internal bool CanSee(Point2D point)
        {
            var viewDirection = GameHelper.GetBeakTipPosition(this.Position, this.BeakAngle) - this.Position;
            var pointDirection = point - this.Position;
            return viewDirection.GetAngle(pointDirection).DegreeValue.Abs() <= GameConstants.ChickenUnit.ViewAngle;
        }

        internal bool CanShoot()
        {
            return this.ShotEngineStepIndex < 0 
                || this.Engine.MoveCount - this.ShotEngineStepIndex >= GameConstants.ShotUnit.MaximumFrequency;
        }

        #endregion

        #region Public Properties

        public GameObjectId UniqueId
        {
            get;
            internal set;
        }

        public GameTeam Team
        {
            get;
            private set;
        }

        public Point2D Position
        {
            [DebuggerStepThrough]
            get
            {
                return m_position;
            }
            set
            {
                if (m_position != value)
                {
                    m_position = value;
                    ResetCachedElement();
                }
            }
        }

        public GameAngle BeakAngle
        {
            [DebuggerStepThrough]
            get
            {
                return m_beakAngle;
            }
            set
            {
                if (m_beakAngle != value)
                {
                    m_beakAngle = value;
                    ResetCachedElement();
                }
            }
        }

        public ChickenUnit KilledBy
        {
            get;
            internal set;
        }

        public bool IsDead
        {
            get;
            internal set;
        }

        public int KillCount
        {
            get;
            internal set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[{0} #{1}] Position = {2}, BeakAngle = {3:D}, Team = {4}, IsDead = {5}",
                this.GetType().Name,
                this.UniqueId,
                this.Position,
                this.BeakAngle,
                this.Team,
                this.IsDead);
        }

        public ChickenElement GetElement()
        {
            if (m_cachedElement == null)
            {
                m_cachedElement = new ChickenElement(this.Position, this.BeakAngle);
            }
            return m_cachedElement;
        }

        #endregion
    }
}