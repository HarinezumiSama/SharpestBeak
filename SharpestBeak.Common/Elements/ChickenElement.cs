using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Common.Elements.Primitives;

namespace SharpestBeak.Common.Elements
{
    public sealed class ChickenElement : ICollidableElement
    {
        #region Fields

        private readonly IList<ICollidablePrimitive> m_roughPrimitives;
        private readonly IList<ICollidablePrimitive> m_primitives;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenElement"/> class.
        /// </summary>
        public ChickenElement(Point2D position, GameAngle beakAngle)
        {
            this.Position = position;
            this.BeakAngle = beakAngle;

            m_roughPrimitives = new List<ICollidablePrimitive>
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

            m_primitives = new List<ICollidablePrimitive>
            {
                new CirclePrimitive(position, GameConstants.ChickenUnit.BodyCircleRadius),
                new ConvexPolygonPrimitive(beakPolygonPoints)
            }
            .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public Point2D Position
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
            get { return true; }
        }

        public IList<ICollidablePrimitive> GetRoughPrimitives()
        {
            return m_roughPrimitives;
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