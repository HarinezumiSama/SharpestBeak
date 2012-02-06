using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common.View;

namespace SharpestBeak.Common
{
    public sealed class ChickenUnitState
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitState"/> class.
        /// </summary>
        internal ChickenUnitState(ChickenUnit unit, MoveInfo previousMove, MoveInfoStates previousMoveState)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }
            if (previousMove == null)
            {
                throw new ArgumentNullException("previousMove");
            }

            #endregion

            this.Unit = unit;
            this.UniqueId = unit.UniqueId;
            this.Team = unit.Team;
            this.IsDead = unit.IsDead;
            this.Position = unit.Position;
            this.BeakAngle = unit.BeakAngle;
            this.HasShots = unit.HasShots();
            this.PreviousMove = previousMove;
            this.PreviousMoveState = previousMoveState;
            this.View = new ViewInfo(unit);
        }

        #endregion

        #region Internal Properties

        internal ChickenUnit Unit
        {
            get;
            private set;
        }

        internal bool HasShots
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public int UniqueId
        {
            get;
            private set;
        }

        public GameTeam Team
        {
            get;
            private set;
        }

        public bool IsDead
        {
            get;
            private set;
        }

        public Point2D Position
        {
            get;
            private set;
        }

        public GameAngle BeakAngle
        {
            get;
            private set;
        }

        public MoveInfo PreviousMove
        {
            get;
            private set;
        }

        public MoveInfoStates PreviousMoveState
        {
            get;
            private set;
        }

        public ViewInfo View
        {
            get;
            private set;
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

        public bool CanShoot()
        {
            return !this.HasShots || this.Unit.CanShootDueToTimer();
        }

        #endregion
    }
}