using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements
{
    public sealed class ChickenElement : BaseElement, ICollidableElement
    {
        #region Constants and Fields

        private readonly ReadOnlyCollection<ICollidablePrimitive> _roughPrimitives;
        private readonly ReadOnlyCollection<ICollidablePrimitive> _primitives;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenElement"/> class.
        /// </summary>
        internal ChickenElement(Point2D position, GameAngle beakAngle)
            : base(position)
        {
            this.BeakAngle = beakAngle;

            _roughPrimitives = new List<ICollidablePrimitive>(1)
            {
                new CirclePrimitive(position, GameConstants.ChickenUnit.BeakOffset)
            }
            .AsReadOnly();

            var defaultBeakPolygonPoints = new[]
            {
                new Point2D(position.X, position.Y - GameConstants.ChickenUnit.BeakRayOffset),
                new Point2D(position.X + GameConstants.ChickenUnit.BeakOffset, position.Y),
                new Point2D(position.X, position.Y + GameConstants.ChickenUnit.BeakRayOffset)
            };
            var beakPolygonPoints = defaultBeakPolygonPoints.Rotate(Position, beakAngle);

            _primitives = new List<ICollidablePrimitive>(2)
            {
                new CirclePrimitive(position, GameConstants.ChickenUnit.BodyCircleRadius),
                new ConvexPolygonPrimitive(beakPolygonPoints)
            }
            .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public GameAngle BeakAngle
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0}. Position = {1}, BeakAngle = {2:D}",
                GetType().Name,
                this.Position,
                this.BeakAngle);
        }

        #endregion

        #region ICollidableElement Members

        public bool HasRoughPrimitives
        {
            [DebuggerStepThrough]
            get
            {
                return true;
            }
        }

        public ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives()
        {
            return _roughPrimitives;
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