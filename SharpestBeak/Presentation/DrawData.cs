using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SharpestBeak.Presentation
{
    public sealed class DrawData : IDisposable
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DrawData"/> class.
        /// </summary>
        public DrawData(Color color, float coefficient)
        {
            #region Argument Check

            if (coefficient.IsNegativeOrZero())
            {
                throw new ArgumentOutOfRangeException(
                    "coefficient",
                    coefficient,
                    "The coefficient must be positive.");
            }

            #endregion

            this.Color = color;
            this.Coefficient = coefficient;

            this.Brush = new SolidBrush(color);
            this.Pen = new Pen(color);
        }

        #endregion

        #region Public Properties

        public Color Color
        {
            get;
            private set;
        }

        public float Coefficient
        {
            get;
            private set;
        }

        public Brush Brush
        {
            get;
            private set;
        }

        public Pen Pen
        {
            get;
            private set;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.Brush.DisposeSafely();
            this.Pen.DisposeSafely();
        }

        #endregion
    }
}