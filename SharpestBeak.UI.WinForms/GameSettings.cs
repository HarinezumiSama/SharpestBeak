using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpestBeak.UI.WinForms.Properties;

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
            this.UICellSize = GameForm.DefaultUICellSize;
            this.NominalSize = Settings.Default.NominalSize;
            this.LightTeam = new TeamSettings() { PlayerCount = Settings.Default.TeamUnitCount };
            this.DarkTeam = new TeamSettings() { PlayerCount = Settings.Default.TeamUnitCount };
        }

        #endregion

        #region Public Properties

        public int UICellSize
        {
            get;
            set;
        }

        public Size NominalSize
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TeamSettings LightTeam
        {
            get;
            private set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TeamSettings DarkTeam
        {
            get;
            private set;
        }

        #endregion
    }
}