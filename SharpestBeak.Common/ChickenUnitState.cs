using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            this.Position = unit.Position;
            this.BeakAngle = unit.BeakAngle;
            //this.PreviousMove = unit....  // TODO: Obtain PreviousMove
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

        public MoveInfo CurrentMove
        {
            get;
            set;
        }

        // TODO: Compute details according to actual unit view

        #endregion
    }
}