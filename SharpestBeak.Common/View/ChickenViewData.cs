using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.View
{
    public sealed class ChickenViewData : BaseViewData
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenViewData"/> class.
        /// </summary>
        internal ChickenViewData(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            this.UniqueId = unit.UniqueId;
            this.Position = unit.Position;
            this.BeakAngle = unit.BeakAngle;
            this.Team = unit.Logic.Team;
        }

        #endregion

        #region Public Properties

        public int UniqueId
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

        public GameTeam Team
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[{0} #{1}] Position = {2}, BeakAngle = {3:D}, Team = {4}",
                this.GetType().Name,
                this.UniqueId,
                this.Position,
                this.BeakAngle,
                this.Team);
        }

        #endregion
    }
}