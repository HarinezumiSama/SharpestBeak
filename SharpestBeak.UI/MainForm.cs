using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharpestBeak.Configuration;
using SharpestBeak.UI.Properties;

namespace SharpestBeak.UI.WinForms
{
    public partial class MainForm : Form
    {
        #region Fields

        private readonly GameSettings m_gameSettings = new GameSettings();

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void DoPlay()
        {
            try
            {
                var lightTeam = m_gameSettings.LightTeam;
                var darkTeam = m_gameSettings.DarkTeam;

                var validationMessage = m_gameSettings.Validate();
                if (!validationMessage.IsNullOrEmpty())
                {
                    this.ShowErrorMessage(validationMessage);
                    return;
                }

                var lightTeamRecord = new ChickenTeamRecord(lightTeam.Logic.Type, lightTeam.PlayerCount);
                var darkTeamRecord = new ChickenTeamRecord(darkTeam.Logic.Type, darkTeam.PlayerCount);

                switch (m_gameSettings.ViewMode)
                {
                    case UIViewMode.WinForms2D:
                        {
                            using (var gameForm = new GameForm(
                               m_gameSettings.UICellSize,
                               m_gameSettings.NominalSize,
                               lightTeamRecord,
                               darkTeamRecord))
                            {
                                gameForm.ShowDialog(this);
                            }
                        }
                        break;

                    case UIViewMode.Wpf3D:
                        {
                            var gameWindow = new GameWindow(
                                m_gameSettings.NominalSize,
                                lightTeamRecord,
                                darkTeamRecord);
                            gameWindow.ShowDialog();
                        }
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
            }
        }

        private void SetPreset(int uiCellSize, Size nominalSize, int playerCount)
        {
            m_gameSettings.UICellSize = uiCellSize;
            m_gameSettings.NominalSize = nominalSize;
            m_gameSettings.DarkTeam.PlayerCount = playerCount;
            m_gameSettings.LightTeam.PlayerCount = playerCount;
            pgGameSettings.Refresh();
        }

        private void SetPreset(int uiCellSize, int nominalSizeDimension, int playerCount)
        {
            SetPreset(uiCellSize, new Size(nominalSizeDimension, nominalSizeDimension), playerCount);
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            pgGameSettings.SelectedObject = m_gameSettings;
            pgGameSettings.ExpandAllGridItems();
            this.Width = Screen.FromControl(this).WorkingArea.Width / 2;
            this.CenterToScreen();

            var logicManagerErrors = LogicManager.Instance.Errors;
            if (!string.IsNullOrEmpty(logicManagerErrors))
            {
                MessageBox.Show(
                    this,
                    string.Format(
                        "There were errors loading logic types:{0}{0}{1}",
                        Environment.NewLine,
                        logicManagerErrors),
                    this.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.F5)
            {
                e.Handled = true;
                DoPlay();
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Event Handlers

        private void playToolButton_Click(object sender, EventArgs e)
        {
            DoPlay();
        }

        private void presetDefaultMenuItem_Click(object sender, EventArgs e)
        {
            var settings = Settings.Default;
            SetPreset(settings.UICellSize, settings.NominalSize, settings.TeamUnitCount);
        }

        private void presetSmallMenuItem_Click(object sender, EventArgs e)
        {
            SetPreset(GameForm.UICellSizeRange.Max, GameConstants.NominalCellCountRange.Min, 1);
        }

        private void presetMediumMenuItem_Click(object sender, EventArgs e)
        {
            SetPreset(GameForm.UICellSizeRange.Max / 2, 8, 4);
        }

        private void presetLargeMenuItem_Click(object sender, EventArgs e)
        {
            SetPreset(48, new Size(24, 16), 16);
        }

        private void extraLargeMenuItem_Click(object sender, EventArgs e)
        {
            SetPreset(GameForm.UICellSizeRange.Min, new Size(48, 24), 32);
        }

        #endregion
    }
}