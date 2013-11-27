using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model
{
    internal sealed class ChickenUnit
    {
        #region Constants and Fields

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
            get;
            private set;
        }

        public Vector2D Movement
        {
            get;
            private set;
        }

        public Point2D NextPosition
        {
            get;
            private set;
        }

        public GameAngle BeakAngle
        {
            get;
            private set;
        }

        public GameAngle BeakMovement
        {
            get;
            private set;
        }

        public GameAngle NextBeakAngle
        {
            get;
            private set;
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

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[{0}] Position = {1}, BeakAngle = {2:D}, Team = {3}, IsDead = {4}",
                ToShortString(),
                this.Position,
                this.BeakAngle,
                this.Team,
                this.IsDead);
        }

        public string ToShortString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} #{1}", GetType().Name, this.UniqueId);
        }

        public ChickenElement GetElement()
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_cachedElement == null)
            {
                _cachedElement = new ChickenElement(this.NextPosition, this.NextBeakAngle);
            }

            return _cachedElement;
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
                || this.Engine.MoveCount - this.ShotEngineStepIndex >= GameConstants.ShotUnit.EngineStepsBetweenShots;
        }

        internal void SetMovement(Vector2D movement, GameAngle beakMovement)
        {
            this.Movement = movement;
            this.NextPosition = this.Position + this.Movement;

            this.BeakMovement = beakMovement;
            this.NextBeakAngle = this.BeakAngle + this.BeakMovement;

            ResetCachedElement();
        }

        internal void ApplyMovement()
        {
            this.Position = this.NextPosition;
            this.Movement = Vector2D.Zero;

            this.BeakAngle = this.NextBeakAngle;
            this.BeakMovement = GameAngle.Zero;

            ResetCachedElement();
        }

        internal void ResetPosition(Point2D position, GameAngle beakAngle)
        {
            this.Position = position;
            this.Movement = Vector2D.Zero;
            this.NextPosition = this.Position;

            this.BeakAngle = beakAngle;
            this.BeakMovement = GameAngle.Zero;
            this.NextBeakAngle = this.BeakAngle;

            ResetCachedElement();
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