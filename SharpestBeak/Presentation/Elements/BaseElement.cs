using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements
{
    public abstract class BaseElement
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseElement"/> class.
        /// </summary>
        internal BaseElement()
        {
            // Nothing to do
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

        protected static void DrawElement(Graphics graphics, DrawData data, ICollidableElement element)
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

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            #endregion

            var primitives = element.GetPrimitives();
            primitives.OfType<BasePrimitive>().DoForEach(item => item.Draw(graphics, data));
        }

        protected abstract void OnDraw(Graphics graphics, DrawData data);

        #endregion
    }
}