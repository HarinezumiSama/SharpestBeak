using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.UI.Properties;

namespace SharpestBeak.UI.WinForms
{
    public sealed class GameSettings
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSettings"/> class.
        /// </summary>
        public GameSettings()
        {
            this.UICellSize = Settings.Default.UICellSize;
            this.NominalSize = Settings.Default.NominalSize;
            this.LightTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
            this.DarkTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
        }

        #endregion

        #region Public Properties

        [DisplayName(@"View mode")]
        public UIViewMode ViewMode
        {
            get;
            set;
        }

        [DisplayName(@"UI cell size")]
        public int UICellSize
        {
            get;
            set;
        }

        [DisplayName(@"Nominal board size")]
        public Size NominalSize
        {
            get;
            set;
        }

        [DisplayName(@"Light team")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TeamSettings LightTeam
        {
            get;
            private set;
        }

        [DisplayName(@"Dark team")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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

            if (!this.UICellSize.IsInRange(GameForm.UICellSizeRange))
            {
                resultBuilder
                    .AppendFormat(
                        "The UI cell size must be in the range {0} to {1}.",
                        GameForm.UICellSizeRange.Min,
                        GameForm.UICellSizeRange.Max)
                    .AppendLine();
            }

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

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializeUICellSize()
        {
            return false;
        }

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializeNominalSize()
        {
            return false;
        }

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializeLightTeam()
        {
            return false;
        }

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializeDarkTeam()
        {
            return false;
        }

        #endregion
    }
}