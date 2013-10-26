using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation
{
    public interface ICollidablePrimitive : ICollidable
    {
        #region Properties

        Point2D BasePoint
        {
            get;
        }

        #endregion

        #region Methods

        bool HasCollision(ICollidablePrimitive other);

        #endregion
    }
}