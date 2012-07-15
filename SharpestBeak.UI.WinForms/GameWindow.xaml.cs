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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using SharpestBeak.Configuration;
using SharpestBeak.Model;
using SharpestBeak.Presentation;

namespace SharpestBeak.UI.WinForms
{
    /// <summary>
    ///     Contains interaction logic for GameWindow.xaml.
    /// </summary>
    public partial class GameWindow : Window
    {
        #region Fields

        private static readonly Color LightTeamUnitColor = Colors.LightGreen;
        private static readonly Color LightTeamShotColor = Colors.Pink;

        private static readonly Color DarkTeamUnitColor = Colors.DarkGreen;
        private static readonly Color DarkTeamShotColor = Colors.DarkRed;

        private readonly double m_uiCellSize;
        private readonly double m_nominalSizeCoefficient;
        private readonly MeshGeometry3D m_chickenGeometry;
        private readonly MeshGeometry3D m_shotGeometry;

        private readonly GameEngine m_gameEngine;

        //private readonly object m_lastPresentationLock = new object();
        //private GamePresentation m_lastPresentation;

        private GameTeam? m_winningTeam;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameWindow"/> class.
        /// </summary>
        private GameWindow()
        {
            InitializeComponent();

            m_uiCellSize = 1d;
            m_nominalSizeCoefficient = GameConstants.NominalCellSize;

            m_chickenGeometry = CreateChickenGeometry(m_nominalSizeCoefficient);
            m_shotGeometry = CreateShotGeometry(m_nominalSizeCoefficient);

            this.TestChickenModel.Geometry = m_chickenGeometry;
            this.TestChickenBrush.Color = LightTeamUnitColor;

            this.TestShotModel.Geometry = m_shotGeometry;
            this.TestShotBrush.Color = LightTeamShotColor;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameWindow"/> class
        ///     using the specified parameters.
        /// </summary>
        public GameWindow(System.Drawing.Size nominalSize, ChickenTeamRecord lightTeam, ChickenTeamRecord darkTeam)
            : this()
        {
            var settings = new GameEngineSettings(nominalSize, lightTeam, darkTeam, this.PaintGame);
            m_gameEngine = new GameEngine(settings);
            m_gameEngine.GameEnded += this.GameEngine_GameEnded;

            this.Title = string.Format(
                "{0} [{1}x{2}] [L: {3}x {4}  -vs-  D: {5}x {6}]",
                this.Title,
                nominalSize.Width,
                nominalSize.Height,
                lightTeam.UnitCount,
                lightTeam.Type.Name,
                darkTeam.UnitCount,
                darkTeam.Type.Name);

            var mb = (MeshBuilder)this.Resources["ChickenBuilder"];
            mb.AddSphere(new Point3D(0d, 0d, 1d), 1d);
            mb.AddCone(new Point3D(0d, 0d, 1d), new Vector3D(1, 0, 0), 0.5d, 0d, 1.5d, true, true, 20);
            mb.Scale(0.5d, 0.5d, 0.5d);
        }

        #endregion

        #region Private Methods

        private static MeshGeometry3D CreateChickenGeometry(double nominalSizeCoefficient)
        {
            #region Argument Check

            if (nominalSizeCoefficient <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "nominalSizeCoefficient",
                    nominalSizeCoefficient,
                    "The value must be positive.");
            }

            #endregion

            var bodyCircleRadius = GameConstants.ChickenUnit.BodyCircleRadius / nominalSizeCoefficient;
            var beakOffset = GameConstants.ChickenUnit.BeakOffset / nominalSizeCoefficient;
            var beakRayOffset = GameConstants.ChickenUnit.BeakRayOffset / nominalSizeCoefficient;

            var meshBuilder = new MeshBuilder();
            meshBuilder.AddSphere(
                new Point3D(0d, 0d, bodyCircleRadius),
                bodyCircleRadius);
            meshBuilder.AddCone(
                new Point3D(0d, 0d, bodyCircleRadius),
                new Point3D(beakOffset, 0d, bodyCircleRadius),
                beakRayOffset,
                true,
                20);
            return meshBuilder.ToMesh(true);
        }

        private static MeshGeometry3D CreateShotGeometry(double nominalSizeCoefficient)
        {
            #region Argument Check

            if (nominalSizeCoefficient <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "nominalSizeCoefficient",
                    nominalSizeCoefficient,
                    "The value must be positive.");
            }

            #endregion

            var bodyCircleRadius = GameConstants.ChickenUnit.BodyCircleRadius / nominalSizeCoefficient;
            var shotRadius = GameConstants.ShotUnit.Radius / nominalSizeCoefficient;

            var meshBuilder = new MeshBuilder();
            meshBuilder.AddSphere(
                new Point3D(0d, 0d, bodyCircleRadius),
                shotRadius);
            return meshBuilder.ToMesh(true);
        }

        private bool InitializeGameUI()
        {
            const double BoardThickness = 1d / 4d;

            try
            {
                var boardSize = m_gameEngine.Data.NominalSize;
                var evenCellsMeshBuilder = new MeshBuilder();
                var oddCellsMeshBuilder = new MeshBuilder();

                for (int x = 0; x < boardSize.Width; x++)
                {
                    for (int y = 0; y < boardSize.Height; y++)
                    {
                        var builder = (x + y) % 2 == 0 ? evenCellsMeshBuilder : oddCellsMeshBuilder;

                        builder.AddBox(
                            new Rect3D(
                                x * m_uiCellSize,
                                y * m_uiCellSize,
                                -BoardThickness,
                                m_uiCellSize,
                                m_uiCellSize,
                                BoardThickness));
                    }
                }

                this.BoardEvenCellModel.Geometry = evenCellsMeshBuilder.ToMesh();
                this.BoardOddCellModel.Geometry = oddCellsMeshBuilder.ToMesh();

                var boardCenter = new Vector3D(
                    boardSize.Width * m_uiCellSize / 2,
                    boardSize.Height * m_uiCellSize / 2,
                    0d);
                this.Camera.Position += boardCenter;
                this.Camera.LookDirection = boardCenter.ToPoint3D() - this.Camera.Position;

                this.TestChickenModel.Transform = new TranslateTransform3D(boardCenter);
                this.TestShotModel.Transform = new TranslateTransform3D(boardCenter + new Vector3D(1d, 0d, 0d));

                //ClearStatusLabels();
                //UpdateMoveCountStatus();

                //DebugHelper.CallAndMeasure(this.ResetGameBoardBackground);

                m_gameEngine.CallPaint();
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
                return false;
            }

            return true;
        }

        private void PaintGame(GamePaintEventArgs e)
        {
            // TODO: PaintGame!

            ////lock (m_lastPresentationLock)
            ////{
            ////    m_lastPresentation = e.Presentation;
            ////}

            ////this.MainViewport.InvalidateVisual();
        }

        private void StartGame()
        {
            try
            {
                if (m_gameEngine.IsRunning)
                {
                    return;
                }

                var winningTeam = m_gameEngine.WinningTeam;
                if (winningTeam.HasValue)
                {
                    var mbr = this.ShowQuestion(
                        string.Format(
                            "The game has ended. Winning team: {0}.{1}{1}"
                                + "Do you wish to reset the game?",
                            winningTeam.Value,
                            Environment.NewLine));
                    if (mbr != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    ResetGame();
                    return;
                }

                m_winningTeam = null;
                m_gameEngine.Start();
                //UpdateMoveCountStatus();
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
                return;
            }
        }

        private void StopGame()
        {
            try
            {
                if (!m_gameEngine.IsRunning)
                {
                    return;
                }

                m_gameEngine.Stop();
                //UpdateMoveCountStatus();
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
                return;
            }
        }

        private void ResetGame()
        {
            StopGame();
            m_winningTeam = null;

            m_gameEngine.Reset();
            m_gameEngine.CallPaint();
        }

        #endregion

        #region Protected Methods

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopGame();
            base.OnClosing(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                    {
                        e.Handled = true;
                        Close();
                    }
                    else if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        e.Handled = true;
                        StopGame();
                    }
                    break;

                case Key.Space:
                case Key.F5:
                    {
                        e.Handled = true;
                        if (m_gameEngine.IsRunning)
                        {
                            StopGame();
                        }
                        else
                        {
                            StartGame();
                        }
                    }
                    break;

                case Key.F8:
                    e.Handled = true;
                    ResetGame();
                    break;
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Event Handlers

        private void GameEngine_GameEnded(object sender, GameEndedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(
                    (Action<object, GameEndedEventArgs>)this.GameEngine_GameEnded,
                    DispatcherPriority.Normal,
                    sender,
                    e);
                return;
            }

            m_winningTeam = e.WinningTeam;

            m_gameEngine.Stop();
            m_gameEngine.CallPaint();

            var winningLogicName = e.WinningLogic == null ? "None" : e.WinningLogic.GetType().Name;

            this.ShowInfoMessage(
                string.Format("Winning team: {0} ({1}).", e.WinningTeam, winningLogicName),
                "Game Ended");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!InitializeGameUI())
            {
                System.Windows.Forms.Application.Exit();
                return;
            }
        }

        #endregion
    }
}