using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using SharpestBeak.Common.Elements;

namespace SharpestBeak.Common
{
    public sealed class ChickenUnit
    {
        #region Fields

        private Point2D m_position;
        private GameAngle m_beakAngle;
        private ChickenElement m_cachedElement;

        #endregion

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

            #endregion

            this.Logic = logic;
            this.ShotTimer = new Stopwatch();
        }

        #endregion

        #region Private Methods

        private void ResetCachedElement()
        {
            m_cachedElement = null;
        }

        #endregion

        #region Internal Properties

        internal Thread Thread
        {
            get;
            set;
        }

        internal Stopwatch ShotTimer
        {
            get;
            private set;
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

        public GameAngle BeakAngle
        {
            [DebuggerStepThrough]
            get
            {
                return m_beakAngle;
            }
            set
            {
                if (m_beakAngle != value)
                {
                    m_beakAngle = value;
                    ResetCachedElement();
                }
            }
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
                "[#{0} : {1}] Position = {2}, BeakAngle = {3:D}",
                this.UniqueIndex,
                this.GetType().Name,
                this.Position,
                this.BeakAngle);
        }

        public Point2D GetBeakTipPosition()
        {
            return this.Position
                .OffsetX(GameConstants.ChickenUnit.BeakOffset)
                .Rotate(this.Position, this.BeakAngle);
        }

        public ChickenElement GetElement()
        {
            if (m_cachedElement == null)
            {
                m_cachedElement = new ChickenElement(this.Position, this.BeakAngle);
            }
            return m_cachedElement;
        }

        #endregion
    }
}