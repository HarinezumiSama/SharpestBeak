using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    public sealed class ChickenUnit
    {
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
            if (logic.Unit != null)
            {
                throw new ArgumentException("The specified logic is already assigned to another unit.", "logic");
            }

            #endregion

            this.Logic = logic;
            logic.Unit = this;
        }

        #endregion

        #region Internal Properties

        internal Thread Thread
        {
            get;
            set;
        }

        #endregion

        #region Public Properties

        public ChickenUnitLogic Logic
        {
            get;
            private set;
        }

        public int UniqueIndex
        {
            get;
            internal set;
        }

        public PointF Position
        {
            get;
            internal set;
        }

        public float BeakAngle
        {
            get;
            internal set;
        }

        public ChickenUnit KilledBy
        {
            get;
            internal set;
        }

        public bool IsDead
        {
            get;
            internal set;
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
                "[#{0} : {1}] Position = {2}, KillCount = {3}",
                this.UniqueIndex,
                this.GetType().Name,
                this.Position,
                this.KillCount);
        }

        #endregion
    }
}