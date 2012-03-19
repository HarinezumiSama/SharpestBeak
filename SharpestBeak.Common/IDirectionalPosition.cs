using System;

namespace SharpestBeak
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