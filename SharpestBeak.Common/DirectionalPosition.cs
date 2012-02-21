using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpestBeak.Common
{
    public struct DirectionalPosition : IDirectionalPosition
    {
        #region Fields

        private readonly Point2D m_position;
        private readonly GameAngle m_angle;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectionalPosition"/> structure.
        /// </summary>
        public DirectionalPosition(Point2D position, GameAngle angle)
        {
            m_position = position;
            m_angle = angle;
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

            m_position = directionalPosition.Position;
            m_angle = directionalPosition.Angle;
        }

        #endregion

        #region IDirectionalPosition Members

        public Point2D Position
        {
            [DebuggerStepThrough]
            get
            {
                return m_position;
            }
        }

        public GameAngle Angle
        {
            [DebuggerStepThrough]
            get
            {
                return m_angle;
            }
        }

        #endregion
    }
}