using System;

namespace SharpestBeak.Physics
{
    public interface IDirectionalPosition
    {
        #region Properties

        Point2D Position
        {
            get;
        }

        GameAngle Angle
        {
            get;
        }

        #endregion
    }
}