using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.UI.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    public sealed class GameSettings
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSettings"/> class.
        /// </summary>
        public GameSettings()
        {
            this.NominalSize = Settings.Default.NominalSize;
            this.LightTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
            this.DarkTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
        }

        #endregion

        #region Public Properties

        [DisplayName(@"Nominal board size")]
        [ExpandableObject]
        public Size NominalSize
        {
            get;
            set;
        }

        [DisplayName(@"Light team")]
        [ExpandableObject]
        public TeamSettings LightTeam
        {
            get;
            private set;
        }

        [DisplayName(@"Dark team")]
        [ExpandableObject]
        public TeamSettings DarkTeam
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public string Validate()
        {
            var resultBuilder = new StringBuilder();

            if (!this.NominalSize.Width.IsInRange(GameConstants.NominalCellCountRange)
                || !this.NominalSize.Height.IsInRange(GameConstants.NominalCellCountRange))
            {
                resultBuilder
                    .AppendFormat(
                        "Each nominal board size dimension must be in the range {0} to {1}.",
                        GameConstants.NominalCellCountRange.Min,
                        GameConstants.NominalCellCountRange.Max)
                    .AppendLine();
            }

            this.LightTeam.ValidateInternal(resultBuilder, "Light team");
            this.DarkTeam.ValidateInternal(resultBuilder, "Dark team");

            return resultBuilder.Length != 0 ? resultBuilder.ToString() : null;
        }

        #endregion

        #region Private Methods

        // Do not delete ShouldSerializeNominalSize method - called by TypeDescriptor
        private bool ShouldSerializeNominalSize()
        {
            return false;
        }

        // Do not delete ShouldSerializeLightTeam method - called by TypeDescriptor
        private bool ShouldSerializeLightTeam()
        {
            return false;
        }

        // Do not delete ShouldSerializeDarkTeam method - called by TypeDescriptor
        private bool ShouldSerializeDarkTeam()
        {
            return false;
        }

        #endregion
    }
}