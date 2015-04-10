using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SharpestBeak.Physics
{
    public struct DirectionalPosition : IDirectionalPosition
    {
        #region Constants and Fields

        private readonly Point2D _position;
        private readonly GameAngle _angle;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectionalPosition"/> structure.
        /// </summary>
        public DirectionalPosition(Point2D position, GameAngle angle)
        {
            _position = position;
            _angle = angle;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectionalPosition"/> class.
        /// </summary>
        public DirectionalPosition(IDirectionalPosition directionalPosition)
        {
            #region Argument Check

            if (directionalPosition == null)
            {
                throw new ArgumentNullException("directionalPosition");
            }

            #endregion

            _position = directionalPosition.Position;
            _angle = directionalPosition.Angle;
        }

        #endregion

        #region IDirectionalPosition Members

        public Point2D Position
        {
            [DebuggerStepThrough]
            get
            {
                return _position;
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

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[ {0}; {1} ]", this.Position, this.Angle);
        }

        #endregion
    }
}