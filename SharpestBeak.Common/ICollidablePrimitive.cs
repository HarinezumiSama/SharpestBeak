using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Elements.Primitives;

namespace SharpestBeak.Common
{
    public interface ICollidablePrimitive : ICollidable
    {
        #region Methods

        bool HasCollision(ICollidablePrimitive other);

        #endregion
    }
}