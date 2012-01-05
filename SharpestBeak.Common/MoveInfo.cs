using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class MoveInfo
    {
        #region Fields

        private readonly string m_asString;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MoveInfo"/> class.
        /// </summary>
        public MoveInfo(MoveDirection moveAction, BeakTurn beakTurn, FireMode fireMode)
        {
            #region Argument Check

            if (!Enum.IsDefined(typeof(MoveDirection), moveAction))
            {
                throw new ArgumentOutOfRangeException("moveAction", moveAction, "Invalid move action.");
            }
            if (!Enum.IsDefined(typeof(BeakTurn), beakTurn))
            {
                throw new ArgumentOutOfRangeException("beakTurn", beakTurn, "Invalid beak turn.");
            }
            if (!Enum.IsDefined(typeof(FireMode), fireMode))
            {
                throw new ArgumentOutOfRangeException("fireMode", fireMode, "Invalid fire mode.");
            }

            #endregion

            this.MoveAction = moveAction;
            this.BeakTurn = beakTurn;
            this.FireMode = fireMode;

            m_asString = string.Format(
                "{0}. BeakTurn = {1}, MoveAction = {2}, FireMode = {3}",
                GetType().Name,
                this.BeakTurn,
                this.MoveAction,
                this.FireMode);
        }

        #endregion

        #region Public Properties

        public MoveDirection MoveAction
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

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return m_asString;
        }

        #endregion

    }
}