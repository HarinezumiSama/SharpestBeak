using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class LogicMoveResult
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicMoveResult"/> class.
        /// </summary>
        internal LogicMoveResult(int capacity)
        {
            this.InnerMap = new Dictionary<ChickenUnit, MoveInfo>(capacity);
        }

        #endregion

        #region Internal Properties

        internal Dictionary<ChickenUnit, MoveInfo> InnerMap
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public void Set(ChickenUnitState unitState, MoveInfo move)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            if (move == null)
            {
                this.InnerMap.Remove(unitState.Unit);
            }
            else
            {
                this.InnerMap[unitState.Unit] = move;
            }
        }

        public MoveInfo Get(ChickenUnitState unitState)
        {
            #region Argument Check

            if (unitState == null)
            {
                throw new ArgumentNullException("unitState");
            }

            #endregion

            return this.InnerMap.GetValueOrDefault(unitState.Unit);
        }

        #endregion
    }
}