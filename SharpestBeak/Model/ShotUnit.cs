using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model
{
    internal sealed class ShotUnit : IDirectionalPosition
    {
        #region Constants and Fields

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
            this.Angle = owner.BeakAngle;
            this.CreationTime = DateTime.Now;
        }

        #endregion

        #region Public Properties

        public GameObjectId UniqueId
        {
            get;
            private set;
        }

        public ChickenUnit Owner
        {
            get;
            private set;
        }

        public GameAngle Angle
        {
            get;
            private set;
        }

        public Point2D Position
        {
            get;
            private set;
        }

        public Vector2D Movement
        {
            get;
            private set;
        }

        public Point2D NextPosition
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
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_cachedElement == null)
            {
                _cachedElement = new ShotElement(this.NextPosition);
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
                return this.Position;  //// TODO [vmcl] Position or NextPosition?
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

        #region Internal Properties

        internal DateTime CreationTime
        {
            get;
            private set;
        }

        #endregion

        #region Internal Methods

        internal void SetMovement(Vector2D movement)
        {
            //// TODO [vmcl] Temporary argument check
            if (movement.GetLength().IsZero())
            {
                throw new ArgumentException("Shot must always move until exploded.", "movement");
            }

            this.Movement = movement;
            this.NextPosition = this.Position + this.Movement;

            ResetCachedElement();
        }

        internal void ApplyMovement()
        {
            this.Position = this.NextPosition;
            this.Movement = Vector2D.Zero;

            ResetCachedElement();
        }

        #endregion

        #region Private Methods

        private void ResetCachedElement()
        {
            _cachedElement = null;
        }

        #endregion
    }
}