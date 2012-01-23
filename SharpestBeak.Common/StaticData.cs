using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class StaticData
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StaticData"/> class.
        /// </summary>
        internal StaticData(Size size)
        {
            #region Argument Check

            if (!size.Width.IsInRange(GameConstants.NominalCellCountRange)
                || !size.Height.IsInRange(GameConstants.NominalCellCountRange))
            {
                throw new ArgumentException(
                    string.Format(
                        "Each board size dimension must be in the range {0} to {1}.",
                        GameConstants.NominalCellCountRange.Min,
                        GameConstants.NominalCellCountRange.Max),
                    "size");
            }

            #endregion

            this.NominalSize = size;
            this.RealSize = new SizeF(
                GameConstants.NominalCellSize * size.Width,
                GameConstants.NominalCellSize * size.Height);
        }

        #endregion

        #region Public Properties

        public Size NominalSize
        {
            get;
            private set;
        }

        public SizeF RealSize
        {
            get;
            private set;
        }

        #endregion
    }
}