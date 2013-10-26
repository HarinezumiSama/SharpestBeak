﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model
{
    internal sealed class ShotUnit : IDirectionalPosition
    {
        #region Fields

        private Point2D _position;
        private readonly GameAngle _angle;
        private ShotElement _cachedElement;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotUnit"/> class.
        /// </summary>
        internal ShotUnit(ChickenUnit owner, GameObjectId uniqueId)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            if (!uniqueId.IsValid)
            {
                throw new ArgumentException("Invalid shot ID.", "uniqueId");
            }

            #endregion

            var beakTipPosition = GameHelper.GetBeakTipPosition(owner.Position, owner.BeakAngle);

            this.UniqueId = uniqueId;
            this.Owner = owner;
            this.Position = GameHelper.GetNewPosition(beakTipPosition, owner.BeakAngle, GameConstants.ShotUnit.Radius);
            _angle = owner.BeakAngle;
            this.CreationTime = DateTime.Now;
        }

        #endregion

        #region Private Methods

        private void ResetCachedElement()
        {
            _cachedElement = null;
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

        public GameObjectId UniqueId
        {
            get;
            private set;
        }

        public Point2D Position
        {
            [DebuggerStepThrough]
            get
            {
                return _position;
            }

            set
            {
                if (_position != value)
                {
                    _position = value;
                    ResetCachedElement();
                }
            }
        }

        public GameAngle Angle
        {
            [DebuggerStepThrough]
            get
            {
                return _angle;
            }
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
            if (_cachedElement == null)
            {
                _cachedElement = new ShotElement(this.Position);
            }

            return _cachedElement;
        }

        #endregion

        #region IDirectionalPosition Members

        Point2D IDirectionalPosition.Position
        {
            [DebuggerStepThrough]
            get
            {
                return _position;
            }
        }

        GameAngle IDirectionalPosition.Angle
        {
            [DebuggerStepThrough]
            get
            {
                return _angle;
            }
        }

        #endregion
    }
}