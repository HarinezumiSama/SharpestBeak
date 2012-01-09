using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpestBeak.Common.Elements;

namespace SharpestBeak.Common
{
    public sealed class ShotUnit
    {
        #region Fields

        private static readonly object s_instanceCountLock = new object();
        private static ulong s_instanceCount;

        #endregion

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

            this.UniqueIndex = GetUniqueIndex();
            this.Owner = owner;
            this.Position = owner.GetBeakTipPosition();
            this.Angle = owner.BeakAngle;
            this.CreationTime = DateTime.Now;
        }

        #endregion

        #region Private Methods

        private static ulong GetUniqueIndex()
        {
            lock (s_instanceCountLock)
            {
                s_instanceCount++;
                return s_instanceCount;
            }
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

        public ulong UniqueIndex
        {
            get;
            private set;
        }

        public GamePoint Position
        {
            get;
            internal set;
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
                this.UniqueIndex,
                this.Position,
                this.Angle,
                this.Owner.UniqueIndex,
                this.CreationTime);
        }

        public ShotElement GetElement()
        {
            return new ShotElement(this.Position);
        }

        #endregion
    }
}