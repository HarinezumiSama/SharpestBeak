using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SharpestBeak.Configuration;
using SharpestBeak.UI.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Size = System.Drawing.Size;

namespace SharpestBeak.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            this.Title = App.Current.FullProductName;
            this.Width = SystemParameters.WorkArea.Width * 3f / 4f;
        }

        #endregion

        #region Private Methods

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var properties = this.GameSettingPropertyGrid.Properties.OfType<PropertyItem>().ToArray();
            properties.DoForEach(item => item.IsExpanded = true);

            var logicManagerErrors = LogicManager.Instance.Errors;
            if (!string.IsNullOrEmpty(logicManagerErrors))
            {
                MessageBox.Show(
                    this,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "There were errors loading logic types:{0}{0}{1}",
                        Environment.NewLine,
                        logicManagerErrors),
                    this.Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void DoPlay()
        {
            try
            {
                var lightTeam = this.CurrentGameSettings.LightTeam;
                var darkTeam = this.CurrentGameSettings.DarkTeam;

                var validationMessage = this.CurrentGameSettings.Validate();
                if (!validationMessage.IsNullOrEmpty())
                {
                    this.ShowErrorMessage(validationMessage);
                    return;
                }

                var lightTeamRecord = new ChickenTeamSettings(lightTeam.Logic.Type, lightTeam.PlayerCount);
                var darkTeamRecord = new ChickenTeamSettings(darkTeam.Logic.Type, darkTeam.PlayerCount);

                var gameWindow = new GameWindow(
                    this.CurrentGameSettings.NominalSize.ToSize(),
                    lightTeamRecord,
                    darkTeamRecord)
                {
                    Owner = this
                };

                gameWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
            }
        }

        private void SetPreset(Size nominalSize, int playerCount)
        {
            this.CurrentGameSettings.NominalSize.SetSize(nominalSize);
            this.CurrentGameSettings.DarkTeam.PlayerCount = playerCount;
            this.CurrentGameSettings.LightTeam.PlayerCount = playerCount;

            this.GameSettingPropertyGrid.Update();
        }

        private void SetPreset(int nominalSizeDimension, int playerCount)
        {
            SetPreset(new Size(nominalSizeDimension, nominalSizeDimension), playerCount);
        }

        private void CanExecuteDefaultPreset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteDefaultPreset(object sender, ExecutedRoutedEventArgs e)
        {
            var settings = Settings.Default;
            SetPreset(settings.NominalSize, settings.TeamUnitCount);
        }

        private void CanExecuteSmallPreset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteSmallPreset(object sender, ExecutedRoutedEventArgs e)
        {
            SetPreset(GameConstants.NominalCellCountRange.Min, 1);
        }

        private void CanExecuteMediumPreset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteMediumPreset(object sender, ExecutedRoutedEventArgs e)
        {
            SetPreset(8, 4);
        }

        private void CanExecuteLargePreset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteLargePreset(object sender, ExecutedRoutedEventArgs e)
        {
            SetPreset(new Size(24, 16), 16);
        }

        private void CanExecuteExtraLargePreset(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteExtraLargePreset(object sender, ExecutedRoutedEventArgs e)
        {
            SetPreset(new Size(48, 24), 32);
        }

        private void CanExecutePlay(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecutePlay(object sender, ExecutedRoutedEventArgs e)
        {
            DoPlay();
        }

        private void CanExecuteExit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExecuteExit(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}