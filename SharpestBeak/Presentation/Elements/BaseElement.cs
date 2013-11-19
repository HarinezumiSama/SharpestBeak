using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements
{
    public abstract class BaseElement
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseElement"/> class.
        /// </summary>
        internal BaseElement(Point2D position)
        {
            this.Position = position;
        }

        #endregion

        #region Public Properties

        public Point2D Position
        {
            get;
            private set;
        }

        #endregion
    }
}