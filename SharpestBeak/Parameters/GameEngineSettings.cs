using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Presentation;

namespace SharpestBeak.Parameters
{
    public sealed class GameEngineSettings
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngineSettings"/> class.
        /// </summary>
        public GameEngineSettings(
            Size nominalSize,
            ChickenTeamRecord lightTeam,
            ChickenTeamRecord darkTeam,
            Action<GamePaintEventArgs> paintCallback)
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
            if (lightTeam == null)
            {
                throw new ArgumentNullException("lightTeam");
            }
            if (darkTeam == null)
            {
                throw new ArgumentNullException("darkTeam");
            }
            if (paintCallback == null)
            {
                throw new ArgumentNullException("paintCallback");
            }

            #endregion

            this.NominalSize = nominalSize;
            this.LightTeam = lightTeam;
            this.DarkTeam = darkTeam;
            this.PaintCallback = paintCallback;
        }

        #endregion

        #region Public Properties

        public Size NominalSize
        {
            get;
            private set;
        }

        public ChickenTeamRecord LightTeam
        {
            get;
            private set;
        }

        public ChickenTeamRecord DarkTeam
        {
            get;
            private set;
        }

        public Action<GamePaintEventArgs> PaintCallback
        {
            get;
            private set;
        }

        public Action<GamePositionEventArgs> PositionCallback
        {
            get;
            set;
        }

        #endregion
    }
}