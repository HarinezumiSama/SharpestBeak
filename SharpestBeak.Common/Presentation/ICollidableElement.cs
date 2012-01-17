using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Presentation
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

        IList<ICollidablePrimitive> GetRoughPrimitives();

        IList<ICollidablePrimitive> GetPrimitives();

        #endregion
    }
}