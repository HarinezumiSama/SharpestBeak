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
        internal ChickenUnitState(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            this.Unit = unit;
            this.UniqueIndex = unit.UniqueIndex;
            this.Team = unit.Logic.Team;
            this.IsDead = unit.IsDead;
            this.Position = unit.Position;
            this.BeakAngle = unit.BeakAngle;
            this.View = new ViewInfo(unit);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitState"/> class.
        /// </summary>
        internal ChickenUnitState(ChickenUnitState unitState)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            this.Unit = unitState.Unit;
            this.IsDead = unitState.Unit.IsDead;  // IsDead should always be obtained from unit, not from state
            this.UniqueIndex = unitState.UniqueIndex;
            this.Team = unitState.Team;
            this.Position = unitState.Unit.Position;
            this.BeakAngle = unitState.Unit.BeakAngle;
            this.View = new ViewInfo(unitState.Unit);
            this.PreviousMove = unitState.CurrentMove;
        }

        #endregion

        #region Internal Properties

        internal ChickenUnit Unit
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public int UniqueIndex
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
            internal set;
        }

        public MoveInfo CurrentMove
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
                this.UniqueIndex,
                this.Position,
                this.BeakAngle,
                this.Team,
                this.IsDead);
        }

        public void SetCurrentMove(MoveInfo value)
        {
            if (this.CurrentMove != null)
            {
                throw new InvalidOperationException("Cannot reassign already assigned move.");
            }

            this.CurrentMove = value;
        }

        #endregion
    }
}