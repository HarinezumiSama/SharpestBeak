using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SharpestBeak.Common.Elements
{
    public sealed class ChickenElement : ICollidableElement
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenElement"/> class.
        /// </summary>
        public ChickenElement(PointF position, GameAngle beakAngle)
        {
            this.Position = position;
            this.BeakAngle = beakAngle;
        }

        #endregion

        #region Public Properties

        public PointF Position
        {
            get;
            private set;
        }

        public GameAngle BeakAngle
        {
            get;
            private set;
        }

        #endregion

        #region ICollidableElement Members

        public bool HasRoughPrimitives
        {
            get { return true; }
        }

        public IList<ICollidablePrimitive> GetRoughPrimitives()
        {
            throw new NotImplementedException();
        }

        public IList<ICollidablePrimitive> GetPrimitives()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollidable Members

        public bool HasCollision(ICollidable other)
        {
            return CollisionDetector.CheckElementCollision(this, other);
        }

        #endregion
    }
}