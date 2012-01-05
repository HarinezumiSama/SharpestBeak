using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class EngineMoveInfo : MoveInfo
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineMoveInfo"/> class.
        /// </summary>
        private EngineMoveInfo(ChickenUnit unit, MoveInfo moveInfo)
            : base(moveInfo.MoveDirection, moveInfo.BeakTurn, moveInfo.FireMode)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            this.Unit = unit;
        }

        #endregion

        #region Internal Methods

        internal static EngineMoveInfo Create(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            var move = unit.Logic.CurrentMove;
            return move == null ? null : new EngineMoveInfo(unit, move);
        }

        #endregion

        #region Public Properties

        public ChickenUnit Unit
        {
            get;
            private set;
        }

        #endregion
    }
}