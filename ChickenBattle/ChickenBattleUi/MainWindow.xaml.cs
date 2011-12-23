using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ChickenBattle.Core;
using ChickenBattleUi.Commands;
using ChickenBattleUi.Controls;

namespace ChickenBattleUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Brush[] _brushes = new Brush[] {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green, Brushes.DeepSkyBlue, Brushes.Blue, Brushes.Violet};

        public MainWindow()
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();

            DataContext = viewModel;
            InitializeComponent();

            StartGame(viewModel.GameInfo);
        }

        public void StartGame (GameInfo gameInfo)
        {
            _mainGrid.BeginInit();
            _mainGrid.RowDefinitions.Clear();
            for (int i=0; i< gameInfo.FieldWidth; i++)
            {
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)});
            }

            for (int j = 0; j < gameInfo.FieldHeight; j++)
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            int colorIndex = 0;
            foreach (var chicken in gameInfo.Players)
            {
                ChickenFigure chickenFigure = new ChickenFigure();
                chickenFigure.ChickenBackground = _brushes[colorIndex];
                ApplyChickenProperies(chickenFigure, chicken);

                _mainGrid.Children.Add(chickenFigure);
                colorIndex++;
            }
        }

        private void ApplyChickenProperies (ChickenFigure chickenFigure, IChicken chicken)
        {
            chickenFigure.SetValue(Grid.ColumnProperty, chicken.X);
            chickenFigure.SetValue(Grid.RowProperty, chicken.Y);
            chickenFigure.Angle = chicken.Angle;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Tick += TimerTick;

            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            dispatcherTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel) DataContext;
            int i = 0;
            foreach (var chicken in viewModel.GameInfo.Players)
            {
                chicken.MakeTurn();
                ApplyChickenProperies(_mainGrid.Children[i] as ChickenFigure, chicken);
                i++;
            }
        }
    }
}
