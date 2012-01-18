using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.View
{
    public sealed class ShotViewData : BaseViewData
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotViewData"/> class.
        /// </summary>
        internal ShotViewData(ShotUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            this.UniqueIndex = unit.UniqueIndex;
            this.Position = unit.Position;
            this.Angle = unit.Angle;
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

        public GameAngle Angle
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[{0} #{1}] Position = {2}, Angle = {3:D}",
                this.GetType().Name,
                this.UniqueIndex,
                this.Position,
                this.Angle);
        }

        #endregion
    }
}