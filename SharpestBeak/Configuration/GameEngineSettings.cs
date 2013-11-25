using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Presentation;

namespace SharpestBeak.Configuration
{
    public sealed class GameEngineSettings
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngineSettings"/> class.
        /// </summary>
        public GameEngineSettings(
            Size nominalSize,
            ChickenTeamSettings lightTeam,
            ChickenTeamSettings darkTeam,
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

        public ChickenTeamSettings LightTeam
        {
            get;
            private set;
        }

        public ChickenTeamSettings DarkTeam
        {
            get;
            private set;
        }

        public Action<GamePaintEventArgs> PaintCallback
        {
            get;
            private set;
        }

        //// TODO [vmcl] Use various positioning schemes: eg. perimeter, opposite sides etc.
        public Action<GamePositionEventArgs> PositionCallback
        {
            get;
            set;
        }

        #endregion
    }
}