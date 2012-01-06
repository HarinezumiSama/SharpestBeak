using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SharpestBeak.Common
{
    public sealed class ShotUnit
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotUnit"/> class.
        /// </summary>
        internal ShotUnit(ChickenUnit owner)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            #endregion

            this.Owner = owner;
            this.Position = owner.Position;
            this.Angle = owner.BeakAngle;
        }

        #endregion

        #region Public Properties

        public PointF Position
        {
            get;
            private set;
        }

        public float Angle
        {
            get;
            private set;
        }

        public ChickenUnit Owner
        {
            get;
            private set;
        }

        #endregion
    }
}