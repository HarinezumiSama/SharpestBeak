using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpestBeak.Common.Elements.Primitives;

namespace SharpestBeak.Common.Elements
{
    public sealed class ShotElement : ICollidableElement
    {
        #region Fields

        private readonly IList<ICollidablePrimitive> m_primitives;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotElement"/> class.
        /// </summary>
        public ShotElement(PointF position)
        {
            this.Position = position;

            m_primitives = new List<ICollidablePrimitive>
            {
                new CirclePrimitive(position, GameConstants.ShotUnit.Radius)
            }
            .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public PointF Position
        {
            get;
            private set;
        }

        #endregion

        #region ICollidableElement Members

        public bool HasRoughPrimitives
        {
            get { return false; }
        }

        public IList<ICollidablePrimitive> GetRoughPrimitives()
        {
            return m_primitives;
        }

        public IList<ICollidablePrimitive> GetPrimitives()
        {
            return m_primitives;
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