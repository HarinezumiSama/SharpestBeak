﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Presentation
{
    public interface ICollidable
    {
        #region Methods

        bool HasCollision(ICollidable other);

        #endregion
    }
}