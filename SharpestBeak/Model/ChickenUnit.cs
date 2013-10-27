using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model
{
    internal sealed class ChickenUnit : IDirectionalPosition
    {
        #region Constants and Fields

        private Point2D _position;
        private GameAngle _beakAngle;
        private ChickenElement _cachedElement;

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
                return _position;
            }

            set
            {
                if (_position != value)
                {
                    _position = value;
                    ResetCachedElement();
                }
            }
        }

        public GameAngle BeakAngle
        {
            [DebuggerStepThrough]
            get
            {
                return _beakAngle;
            }

            set
            {
                if (_beakAngle != value)
                {
                    _beakAngle = value;
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
            if (_cachedElement == null)
            {
                _cachedElement = new ChickenElement(this.Position, this.BeakAngle);
            }

            return _cachedElement;
        }

        #endregion

        #region IDirectionalPosition Members

        Point2D IDirectionalPosition.Position
        {
            [DebuggerStepThrough]
            get
            {
                return _position;
            }
        }

        GameAngle IDirectionalPosition.Angle
        {
            [DebuggerStepThrough]
            get
            {
                return _beakAngle;
            }
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

        #region Private Methods

        private void ResetCachedElement()
        {
            _cachedElement = null;
        }

        #endregion
    }
}