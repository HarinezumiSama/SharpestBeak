﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using SharpestBeak.Configuration;
using SharpestBeak.Model;
using SharpestBeak.Presentation;

namespace SharpestBeak.UI
{
    /// <summary>
    ///     Contains interaction logic for GameWindow.xaml.
    /// </summary>
    public partial class GameWindow : Window
    {
        #region Nested Types

        #region GameObjectData Class

        private abstract class GameObjectData
        {
            #region Constructors

            protected GameObjectData(
                GameObjectId id,
                GameTeam team,
                Point3D position,
                MatrixTransform3D transform)
            {
                #region Argument Check

                if (transform == null)
                {
                    throw new ArgumentNullException("transform");
                }

                #endregion

                this.Id = id;
                this.Team = team;
                this.Position = position;
                this.Transform = transform;
            }

            #endregion

            #region Public Properties

            public GameObjectId Id
            {
                get;
                private set;
            }

            public GameTeam Team
            {
                get;
                private set;
            }

            public Point3D Position
            {
                get;
                set;
            }

            public MatrixTransform3D Transform
            {
                get;
                private set;
            }

            #endregion
        }

        #endregion

        #region ChickenData Class

        private sealed class ChickenData : GameObjectData
        {
            #region Constructors

            public ChickenData(
                GameObjectId id,
                GameTeam team,
                Point3D position,
                double beakAngle,
                ModelVisual3D visual,
                MatrixTransform3D transform)
                : base(id, team, position, transform)
            {
                #region Argument Check

                if (visual == null)
                {
                    throw new ArgumentNullException("visual");
                }

                #endregion

                this.BeakAngle = beakAngle;
                this.Visual = visual;
            }

            #endregion

            #region Public Properties

            public double BeakAngle
            {
                get;
                set;
            }

            public ModelVisual3D Visual
            {
                get;
                private set;
            }

            #endregion
        }

        #endregion

        #region ShotData Class

        private sealed class ShotData : GameObjectData
        {
            #region Constructors

            public ShotData(
                GameObjectId id,
                GameTeam team,
                Point3D position,
                GeometryModel3D model,
                MatrixTransform3D transform)
                : base(id, team, position, transform)
            {
                #region Argument Check

                if (model == null)
                {
                    throw new ArgumentNullException("model");
                }

                #endregion

                this.Model = model;
            }

            #endregion

            #region Public Properties

            public GeometryModel3D Model
            {
                get;
                private set;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private static readonly DiffuseMaterial LightTeamUnitMaterial =
            new DiffuseMaterial(new SolidColorBrush(Colors.LightGreen));

        private static readonly DiffuseMaterial LightTeamShotMaterial =
            new DiffuseMaterial(new SolidColorBrush(Colors.Pink));

        private static readonly DiffuseMaterial DarkTeamUnitMaterial =
            new DiffuseMaterial(new SolidColorBrush(Colors.DarkGreen));

        private static readonly DiffuseMaterial DarkTeamShotMaterial =
            new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed));

        private readonly double m_uiCellSize;
        private readonly double m_nominalSizeCoefficient;
        private readonly MeshGeometry3D m_chickenGeometry;
        private readonly MeshGeometry3D m_shotGeometry;

        private readonly GameEngine m_gameEngine;

        private readonly Dictionary<GameObjectId, ChickenData> m_chickenDatas =
            new Dictionary<GameObjectId, ChickenData>();

        private readonly Dictionary<ModelVisual3D, ChickenData> m_chickenVisualToData =
            new Dictionary<ModelVisual3D, ChickenData>();

        private readonly Dictionary<GameObjectId, ShotData> m_shotDatas =
            new Dictionary<GameObjectId, ShotData>();

        private Point3D m_defaultCameraPosition;
        private Vector3D m_defaultCameraLookDirection;
        private Vector3D m_defaultCameraUpDirection;
        private double m_defaultCameraFieldOfView;

        private GameObjectId? m_followedChickenId;
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

            SaveCameraDefaults();
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

            //var mb = (MeshBuilder)this.Resources["ChickenBuilder"];
            //mb.AddSphere(new Point3D(0d, 0d, 1d), 1d);
            //mb.AddCone(new Point3D(0d, 0d, 1d), new Vector3D(1, 0, 0), 0.5d, 0d, 1.5d, true, true, 20);
            //mb.Scale(0.5d, 0.5d, 0.5d);
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

            // TODO: [3D] Draw target points for random chickens

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

                m_followedChickenId = null;
                SaveCameraDefaults();

                //ClearStatusLabels();
                //UpdateMoveCountStatus();

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

        private void InitializeVisualData(GamePresentation presentation)
        {
            ClearData();

            foreach (var chicken in presentation.Chickens)
            {
                var material = chicken.Team == GameTeam.Light ? LightTeamUnitMaterial : DarkTeamUnitMaterial;
                var matrix = GetChickenTransformMatrix(chicken);
                var transform = new MatrixTransform3D(matrix);
                var position = ConvertEnginePosition(chicken.Element.Position);

                var chickenVisual = new ModelVisual3D()
                {
                    Content = new GeometryModel3D(m_chickenGeometry, material)
                    {
                        Transform = transform
                    }
                };

                var data = new ChickenData(
                    chicken.UniqueId,
                    chicken.Team,
                    position,
                    chicken.Element.BeakAngle.DegreeValue,
                    chickenVisual,
                    transform);
                m_chickenDatas.Add(data.Id, data);
                m_chickenVisualToData.Add(chickenVisual, data);
                this.BoardVisual.Children.Add(chickenVisual);
            }
        }

        private Point3D ConvertEnginePosition(SharpestBeak.Physics.Point2D position)
        {
            return new Point3D(
                position.X / m_nominalSizeCoefficient,
                position.Y / m_nominalSizeCoefficient,
                GameConstants.ChickenUnit.BodyCircleRadius / m_nominalSizeCoefficient);
        }

        private Matrix3D GetChickenTransformMatrix(ChickenPresentation chicken)
        {
            var result = Matrix3D.Identity;

            result.Rotate(new Quaternion(new Vector3D(0d, 0d, 1d), chicken.Element.BeakAngle.DegreeValue));

            var normalizedPosition = chicken.Element.Position / (float)m_nominalSizeCoefficient;
            result.Translate(new Vector3D(normalizedPosition.X, normalizedPosition.Y, 0d));

            return result;
        }

        private Matrix3D GetShotTransformMatrix(ShotPresentation shot)
        {
            var result = Matrix3D.Identity;

            var normalizedPosition = shot.Element.Position / (float)m_nominalSizeCoefficient;
            result.Translate(new Vector3D(normalizedPosition.X, normalizedPosition.Y, 0d));

            return result;
        }

        private void ClearData()
        {
            this.BoardVisual.Children.Clear();
            this.ShotsModelGroup.Children.Clear();

            m_chickenDatas.Clear();
            m_chickenVisualToData.Clear();
            m_shotDatas.Clear();
        }

        private void PaintGame(GamePaintEventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(
                    (Action<GamePaintEventArgs>)this.PaintGame,
                    DispatcherPriority.Render,
                    e);
                return;
            }

            DoPaintGame(e.Presentation);
        }

        private void DoPaintGame(GamePresentation presentation)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(
                    (Action<GamePresentation>)this.DoPaintGame,
                    DispatcherPriority.Render,
                    presentation);
                return;
            }

            if (!m_chickenDatas.Any())
            {
                InitializeVisualData(presentation);
            }

            var deadChickenIds = new HashSet<GameObjectId>(m_chickenDatas.Keys);
            foreach (var chicken in presentation.Chickens)
            {
                var data = m_chickenDatas[chicken.UniqueId];
                data.Position = ConvertEnginePosition(chicken.Element.Position);
                data.BeakAngle = chicken.Element.BeakAngle.DegreeValue;
                data.Transform.Matrix = GetChickenTransformMatrix(chicken);

                deadChickenIds.Remove(data.Id);
            }

            foreach (var deadChickenId in deadChickenIds)
            {
                var data = m_chickenDatas[deadChickenId];
                this.BoardVisual.Children.Remove(data.Visual);
                m_chickenDatas.Remove(deadChickenId);
                m_chickenVisualToData.Remove(data.Visual);
            }

            var explodedShotIds = new HashSet<GameObjectId>(m_shotDatas.Keys);
            foreach (var shot in presentation.Shots)
            {
                var data = m_shotDatas.GetValueOrDefault(shot.UniqueId);
                if (data == null)
                {
                    var material = shot.Owner.Team == GameTeam.Light ? LightTeamShotMaterial : DarkTeamShotMaterial;
                    var matrix = GetShotTransformMatrix(shot);
                    var transform = new MatrixTransform3D(matrix);
                    var position = ConvertEnginePosition(shot.Element.Position);

                    var model = new GeometryModel3D(m_shotGeometry, material)
                    {
                        Transform = transform
                    };

                    data = new ShotData(shot.UniqueId, shot.Owner.Team, position, model, transform);
                    m_shotDatas.Add(data.Id, data);
                    this.ShotsModelGroup.Children.Add(model);
                }
                else
                {
                    var position = ConvertEnginePosition(shot.Element.Position);
                    data.Position = position;

                    data.Transform.Matrix = GetShotTransformMatrix(shot);
                    explodedShotIds.Remove(data.Id);
                }
            }

            foreach (var explodedShotId in explodedShotIds)
            {
                var data = m_shotDatas[explodedShotId];
                this.ShotsModelGroup.Children.Remove(data.Model);
                m_shotDatas.Remove(explodedShotId);
            }

            FollowChicken();
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

            RestoreCameraDefaults();

            ClearData();

            m_gameEngine.Reset();
        }

        private void SaveCameraDefaults()
        {
            m_defaultCameraPosition = this.Camera.Position;
            m_defaultCameraLookDirection = this.Camera.LookDirection;
            m_defaultCameraUpDirection = this.Camera.UpDirection;
            m_defaultCameraFieldOfView = this.Camera.FieldOfView;
        }

        private void RestoreCameraDefaults()
        {
            m_followedChickenId = null;

            this.Camera.Position = m_defaultCameraPosition;
            this.Camera.LookDirection = m_defaultCameraLookDirection;
            this.Camera.UpDirection = m_defaultCameraUpDirection;
            this.Camera.FieldOfView = m_defaultCameraFieldOfView;
            this.Camera.Transform = null;
        }

        private void StartFollowingChicken(GameObjectId chickenId)
        {
            m_followedChickenId = chickenId;

            var data = m_chickenDatas.GetValueOrDefault(m_followedChickenId.Value);
            if (data == null)
            {
                RestoreCameraDefaults();
                return;
            }

            this.Camera.Position = new Point3D(
                GameConstants.ChickenUnit.BodyCircleRadius / m_nominalSizeCoefficient,
                0d,
                0d);
            this.Camera.LookDirection = new Vector3D(1d, 0d, 0d);
            this.Camera.UpDirection = new Vector3D(0d, 0d, 1d);
            this.Camera.FieldOfView = GameConstants.ChickenUnit.ViewAngle * 2d;
            this.Camera.Transform = null;

            FollowChicken();
        }

        private void FollowChicken()
        {
            if (!m_followedChickenId.HasValue)
            {
                return;
            }

            var data = m_chickenDatas.GetValueOrDefault(m_followedChickenId.Value);
            if (data == null)
            {
                RestoreCameraDefaults();
                return;
            }

            var transformMatrix = Matrix3D.Identity;
            transformMatrix.Rotate(new Quaternion(new Vector3D(0d, 0d, 1d), data.BeakAngle));
            transformMatrix.Translate(data.Position.ToVector3D());

            var transform = this.Camera.Transform as MatrixTransform3D;
            if (transform == null)
            {
                transform = new MatrixTransform3D();
                this.Camera.Transform = transform;
            }

            transform.Matrix = transformMatrix;
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

                case Key.C:
                    e.Handled = true;
                    RestoreCameraDefaults();
                    break;
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Event Handlers

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var presentation = m_gameEngine.GetPresentation();
            DoPaintGame(presentation);
        }

        private void GameEngine_GameEnded(object sender, GameEndedEventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
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
            CompositionTarget.Rendering += this.CompositionTarget_Rendering;

            if (!InitializeGameUI())
            {
                System.Windows.Forms.Application.Exit();
                return;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_gameEngine.IsRunning)
            {
                StopGame();
            }
            else
            {
                StartGame();
            }
        }

        private void MainViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePoint = e.GetPosition(this.MainViewport);

            var hitTestResult = VisualTreeHelper.HitTest(this.MainViewport, mousePoint)
                as RayMeshGeometry3DHitTestResult;
            if (hitTestResult == null)
            {
                return;
            }

            var visual = hitTestResult.VisualHit as ModelVisual3D;
            if (visual == null)
            {
                return;
            }

            var data = m_chickenVisualToData.GetValueOrDefault(visual);
            if (data == null)
            {
                return;
            }

            e.Handled = true;
            StartFollowingChicken(data.Id);
        }

        #endregion
    }
}