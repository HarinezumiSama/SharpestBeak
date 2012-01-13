using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameCommonData
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameCommonData"/> class.
        /// </summary>
        internal GameCommonData(Size size)
        {
            #region Argument Check

            if (size.Width < GameConstants.MinNominalCellCount || size.Height < GameConstants.MinNominalCellCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The board size cannot be less than {0} nominal cells in each dimension.",
                        GameConstants.MinNominalCellCount),
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