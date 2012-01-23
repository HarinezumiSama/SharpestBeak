using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpestBeak.Common;

namespace SharpestBeak.UI.WinForms
{
    public partial class MainForm : Form
    {
        #region Constants

        private const int c_defaultPlayerCount = 8;

        #endregion

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

                var lightTeamRecord = new ChickenTeamRecord(lightTeam.LogicType, lightTeam.PlayerCount);
                var darkTeamRecord = new ChickenTeamRecord(darkTeam.LogicType, darkTeam.PlayerCount);

                using (var gameForm = new GameForm(
                    m_gameSettings.UICellSize,
                    m_gameSettings.NominalSize,
                    lightTeamRecord,
                    darkTeamRecord))
                {
                    gameForm.ShowDialog(this);
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

        private void btnPlay_Click(object sender, EventArgs e)
        {
            DoPlay();
        }

        #endregion
    }
}