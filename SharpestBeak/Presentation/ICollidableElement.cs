using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpestBeak.Presentation
{
    public interface ICollidableElement : ICollidable
    {
        #region Properties

        bool HasRoughPrimitives
        {
            get;
        }

        #endregion

        #region Methods

        ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives();

        ReadOnlyCollection<ICollidablePrimitive> GetPrimitives();

        #endregion
    }
}