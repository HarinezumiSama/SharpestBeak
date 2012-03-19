using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak
{
    public sealed class GameEngineData
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngineData"/> class.
        /// </summary>
        internal GameEngineData(Size nominalSize)
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
            this.BoardBorder = new ConvexPolygonPrimitive(
                Point2D.Zero,
                new Point2D(this.RealSize.Width, 0f),
                new Point2D(this.RealSize),
                new Point2D(0f, this.RealSize.Height));
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

        public ConvexPolygonPrimitive BoardBorder
        {
            get;
            private set;
        }

        #endregion
    }
}