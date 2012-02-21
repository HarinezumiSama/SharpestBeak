using System;

namespace SharpestBeak.Common
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