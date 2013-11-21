using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements
{
    public sealed class ShotElement : BaseElement, ICollidableElement
    {
        #region Constants and Fields

        private readonly ReadOnlyCollection<ICollidablePrimitive> _primitives;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotElement"/> class.
        /// </summary>
        internal ShotElement(Point2D position)
            : base(position)
        {
            _primitives = new List<ICollidablePrimitive>(1)
            {
                new CirclePrimitive(position, GameConstants.ShotUnit.Radius)
            }
            .AsReadOnly();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0}. Position = {1}",
                GetType().Name,
                this.Position);
        }

        #endregion

        #region ICollidableElement Members

        public bool HasRoughPrimitives
        {
            [DebuggerStepThrough]
            get
            {
                return false;
            }
        }

        public ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives()
        {
            throw new NotSupportedException();
        }

        public ReadOnlyCollection<ICollidablePrimitive> GetPrimitives()
        {
            return _primitives;
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