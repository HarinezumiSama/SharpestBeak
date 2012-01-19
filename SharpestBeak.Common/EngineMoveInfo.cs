using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    internal sealed class EngineMoveInfo
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EngineMoveInfo"/> class.
        /// </summary>
        private EngineMoveInfo(ChickenUnit unit, MoveInfo moveInfo)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }
            if (moveInfo == null)
            {
                throw new ArgumentNullException("moveInfo");
            }

            #endregion

            this.Info = moveInfo;
            this.Unit = unit;
        }

        #endregion

        #region Internal Methods

        internal static EngineMoveInfo Create(ChickenUnitState unitState)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            var move = unitState.CurrentMove;
            return move == null ? null : new EngineMoveInfo(unitState.Unit, move);
        }

        #endregion

        #region Public Properties

        public MoveInfo Info
        {
            get;
            private set;
        }

        public ChickenUnit Unit
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0}. Info = {{{1}}}, Unit = {{{2}}}",
                GetType().Name,
                this.Info,
                this.Unit);
        }

        #endregion
    }
}