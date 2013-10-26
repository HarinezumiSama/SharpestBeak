using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements
{
    public sealed class ShotElement : BaseElement, ICollidableElement
    {
        #region Fields

        private readonly IList<ICollidablePrimitive> _primitives;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotElement"/> class.
        /// </summary>
        public ShotElement(Point2D position)
        {
            this.Position = position;

            _primitives = new List<ICollidablePrimitive>(1)
            {
                new CirclePrimitive(position, GameConstants.ShotUnit.Radius)
            }
            .AsReadOnly();
        }

        #endregion

        #region Protected Methods

        protected override void OnDraw(Graphics graphics, DrawData data)
        {
            DrawElement(graphics, data, this);
        }

        #endregion

        #region Public Properties

        public Point2D Position
        {
            get;
            private set;
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

        public IList<ICollidablePrimitive> GetRoughPrimitives()
        {
            throw new NotSupportedException();
        }

        public IList<ICollidablePrimitive> GetPrimitives()
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