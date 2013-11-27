using System;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Model
{
    public struct MovementAndPosition
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MovementAndPosition"/> class.
        /// </summary>
        internal MovementAndPosition(Vector2D movement, Point2D position)
            : this()
        {
            this.Movement = movement;
            this.Position = position;
        }

        #endregion

        #region Public Properties

        public Vector2D Movement
        {
            get;
            private set;
        }

        public Point2D Position
        {
            get;
            private set;
        }

        #endregion
    }
}