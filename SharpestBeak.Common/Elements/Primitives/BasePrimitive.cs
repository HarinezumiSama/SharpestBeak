using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public abstract class BasePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BasePrimitive"/> class.
        /// </summary>
        internal BasePrimitive()
        {
            // Nothing to do
        }

        #endregion

        #region Public Properties

        public Point2D BasePoint
        {
            get;
            protected set;
        }

        #endregion
    }
}