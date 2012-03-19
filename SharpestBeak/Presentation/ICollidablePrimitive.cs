using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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