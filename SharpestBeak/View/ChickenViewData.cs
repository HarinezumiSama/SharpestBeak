﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.View
{
    public sealed class ChickenViewData : BaseViewData, IDirectionalPosition
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
            this.Team = unit.Team;
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
                return this.BeakAngle;
            }
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