using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Omnifactotum.Annotations;
using SharpestBeak.Model;
using SharpestBeak.Presentation;

namespace SharpestBeak.Configuration
{
    public sealed class GameEngineSettings
    {
        #region Constants and Fields

        [NotNull]
        private Action<GamePositionEventArgs> _positionCallback;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngineSettings"/> class.
        /// </summary>
        public GameEngineSettings(
            Size nominalSize,
            ChickenTeamSettings lightTeamSettings,
            ChickenTeamSettings darkTeamSettings,
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

            if (lightTeamSettings == null)
            {
                throw new ArgumentNullException("lightTeamSettings");
            }

            if (darkTeamSettings == null)
            {
                throw new ArgumentNullException("darkTeamSettings");
            }

            if (paintCallback == null)
            {
                throw new ArgumentNullException("paintCallback");
            }

            #endregion

            this.NominalSize = nominalSize;
            this.LightTeamSettings = lightTeamSettings;
            this.DarkTeamSettings = darkTeamSettings;
            this.PaintCallback = paintCallback;
            _positionCallback = UnitPositioningHelper.PositionRandomly;
        }

        #endregion

        #region Public Properties

        public Size NominalSize
        {
            get;
            private set;
        }

        public ChickenTeamSettings LightTeamSettings
        {
            get;
            private set;
        }

        public ChickenTeamSettings DarkTeamSettings
        {
            get;
            private set;
        }

        public Action<GamePaintEventArgs> PaintCallback
        {
            get;
            private set;
        }

        [NotNull]
        public Action<GamePositionEventArgs> PositionCallback
        {
            [DebuggerStepThrough]
            get
            {
                return _positionCallback;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _positionCallback = value;
            }
        }

        #endregion
    }
}