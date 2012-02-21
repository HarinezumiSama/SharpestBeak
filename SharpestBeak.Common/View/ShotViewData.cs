using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpestBeak.Common.View
{
    public sealed class ShotViewData : BaseViewData, IDirectionalPosition
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

            this.UniqueId = unit.UniqueId;
            this.Position = unit.Position;
            this.Angle = unit.Angle;
            this.OwnerUniqueId = unit.Owner.UniqueId;
            this.Team = unit.Owner.Team;
        }

        #endregion

        #region Internal Properties

        internal GameObjectId OwnerUniqueId
        {
            get;
            private set;
        }

        internal GameTeam Team
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public GameObjectId UniqueId
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
                this.UniqueId,
                this.Position,
                this.Angle);
        }

        #endregion

        #region IDirectionalPosition Members

        Point2D IDirectionalPosition.Position
        {
            [DebuggerStepThrough]
            get
            {
                return this.Position;
            }
        }

        GameAngle IDirectionalPosition.Angle
        {
            [DebuggerStepThrough]
            get
            {
                return this.Angle;
            }
        }

        #endregion
    }
}