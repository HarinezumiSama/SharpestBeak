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
        public MoveInfo(BeakTurn beakTurn, MoveAction moveAction)
        {
            #region Argument Check

            if (!Enum.IsDefined(typeof(BeakTurn), beakTurn))
            {
                throw new ArgumentOutOfRangeException("beakTurn", beakTurn, "Invalid beak turn.");
            }
            if (!Enum.IsDefined(typeof(MoveAction), moveAction))
            {
                throw new ArgumentOutOfRangeException("moveAction", moveAction, "Invalid move action.");
            }

            #endregion

            this.BeakTurn = beakTurn;
            this.MoveAction = moveAction;

            m_asString = string.Format(
                "{0}. BeakTurn = {1}, MoveAction = {2}",
                GetType().Name,
                this.BeakTurn,
                this.MoveAction);
        }

        #endregion

        #region Public Properties

        public BeakTurn BeakTurn
        {
            get;
            private set;
        }

        public MoveAction MoveAction
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