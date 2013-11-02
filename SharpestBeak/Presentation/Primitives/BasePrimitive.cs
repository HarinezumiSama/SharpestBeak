using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives
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

        #region Public Methods

        public void Draw(Graphics graphics, DrawData data)
        {
            #region Argument Check

            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            #endregion

            OnDraw(graphics, data);
        }

        #endregion

        #region Protected Methods

        protected abstract void OnDraw(Graphics graphics, DrawData data);

        #endregion
    }
}