using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpestBeak.Presentation
{
    public interface ICollidable
    {
        #region Methods

        bool HasCollision(ICollidable other);

        #endregion
    }
}