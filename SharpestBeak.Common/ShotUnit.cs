using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Common.Presentation.Elements;

namespace SharpestBeak.Common
{
    internal sealed class ShotUnit
    {
        #region Fields

        private Point2D m_position;
        private ShotElement m_cachedElement;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotUnit"/> class.
        /// </summary>
        internal ShotUnit(ChickenUnit owner, int uniqueId)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (uniqueId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "uniqueId",
                    uniqueId,
                    "The unique ID must be positive.");
            }

            #endregion

            var beakTipPosition = GameHelper.GetBeakTipPosition(owner.Position, owner.BeakAngle);

            this.UniqueId = uniqueId;
            this.Owner = owner;
            this.Position = GameHelper.GetNewPosition(beakTipPosition, owner.BeakAngle, GameConstants.ShotUnit.Radius);
            this.Angle = owner.BeakAngle;
            this.CreationTime = DateTime.Now;
        }

        #endregion

        #region Private Methods

        private void ResetCachedElement()
        {
            m_cachedElement = null;
        }

        #endregion

        #region Internal Properties

        internal DateTime CreationTime
        {
            get;
            private set;
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
            [DebuggerStepThrough]
            get
            {
                return m_position;
            }
            set
            {
                if (m_position != value)
                {
                    m_position = value;
                    ResetCachedElement();
                }
            }
        }

        public GameAngle Angle
        {
            get;
            private set;
        }

        public ChickenUnit Owner
        {
            get;
            private set;
        }

        public bool Exploded
        {
            get;
            internal set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[{0} : #{1}] Position = {2}, Angle = {3:D}, Owner = #{4}, CreationTime = {5:HH:mm:ss.fff}",
                this.GetType().Name,
                this.UniqueId,
                this.Position,
                this.Angle,
                this.Owner.UniqueId,
                this.CreationTime);
        }

        public ShotElement GetElement()
        {
            if (m_cachedElement == null)
            {
                m_cachedElement = new ShotElement(this.Position);
            }
            return m_cachedElement;
        }

        #endregion
    }
}