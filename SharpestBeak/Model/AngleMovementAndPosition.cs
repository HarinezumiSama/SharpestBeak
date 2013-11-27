using System;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Model
{
    public struct AngleMovementAndPosition
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AngleMovementAndPosition"/> class.
        /// </summary>
        internal AngleMovementAndPosition(GameAngle movement, GameAngle position)
            : this()
        {
            this.Movement = movement;
            this.Position = position;
        }

        #endregion

        #region Public Properties

        public GameAngle Movement
        {
            get;
            private set;
        }

        public GameAngle Position
        {
            get;
            private set;
        }

        #endregion
    }
}