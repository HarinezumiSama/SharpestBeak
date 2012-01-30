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
        internal StaticData(Size nominalSize)
        {
            #region Argument Check

            if (!nominalSize.Width.IsInRange(GameConstants.NominalCellCountRange)
                || !nominalSize.Height.IsInRange(GameConstants.NominalCellCountRange))
            {
                throw new ArgumentException(
                    string.Format(
                        "Each nominal board size dimension must be in the range {0} to {1}.",
                        GameConstants.NominalCellCountRange.Min,
                        GameConstants.NominalCellCountRange.Max),
                    "nominalSize");
            }

            #endregion

            this.NominalSize = nominalSize;
            this.RealSize = new SizeF(
                GameConstants.NominalCellSize * nominalSize.Width,
                GameConstants.NominalCellSize * nominalSize.Height);
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