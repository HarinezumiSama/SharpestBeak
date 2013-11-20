using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private bool _isDead;
        private MoveDirection _moveDirection;

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

            internal set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                ResetCachedElement();
                ResetMoveDirection();
            }
        }

        public MoveDirection MoveDirection
        {
            [DebuggerStepThrough]
            get
            {
                return _moveDirection;
            }

            internal set
            {
                if (this.IsDead && !value.IsNone)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, "The dead {{{0}}} cannot move.", ToShortString()),
                        "value");
                }

                _moveDirection = value;
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
            [DebuggerStepThrough]
            get
            {
                return _isDead;
            }

            internal set
            {
                _isDead = value;

                if (_isDead)
                {
                    ResetMoveDirection();
                }
            }
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
            ResetMoveDirection();
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

        private void ResetMoveDirection()
        {
            _moveDirection = MoveDirection.None;
        }

        #endregion
    }
}