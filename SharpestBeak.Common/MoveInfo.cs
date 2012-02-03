using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class MoveInfo
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MoveInfo"/> class.
        /// </summary>
        public MoveInfo(MoveDirection moveDirection, BeakTurn beakTurn, FireMode fireMode)
        {
            #region Argument Check

            if (!Enum.IsDefined(typeof(MoveDirection), moveDirection))
            {
                throw new ArgumentOutOfRangeException("moveDirection", moveDirection, "Invalid move action.");
            }
            if (!Enum.IsDefined(typeof(FireMode), fireMode))
            {
                throw new ArgumentOutOfRangeException("fireMode", fireMode, "Invalid fire mode.");
            }

            #endregion

            this.MoveDirection = moveDirection;
            this.BeakTurn = beakTurn;
            this.FireMode = fireMode;
            this.State = MoveInfoState.None;
        }

        #endregion

        #region Public Properties

        public MoveDirection MoveDirection
        {
            get;
            private set;
        }

        public BeakTurn BeakTurn
        {
            get;
            private set;
        }

        public FireMode FireMode
        {
            get;
            private set;
        }

        public MoveInfoState State
        {
            get;
            internal set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0} [{5}]. BeakTurn = {1}, MoveDirection = {2}, FireMode = {3}, State = {4}",
                GetType().Name,
                this.BeakTurn,
                this.MoveDirection,
                this.FireMode,
                this.State,
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this));
        }

        #endregion
    }
}