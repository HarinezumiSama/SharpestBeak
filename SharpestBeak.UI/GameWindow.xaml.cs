using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class GameWindow
    {
        #region Nested Types

        #region GameObjectData Class

        private abstract class GameObjectData
        {
            #region Constructors

            protected GameObjectData(
                GameObjectId id,
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
                Point3D position,
                double beakAngle,
                ModelVisual3D visual,
                MatrixTransform3D transform)
                : base(id, position, transform)
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
                Point3D position,
                GeometryModel3D model,
                MatrixTransform3D transform)
                : base(id, position, transform)
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

        private readonly double _uiCellSize;
        private readonly double _nominalSizeCoefficient;
        private readonly MeshGeometry3D _chickenGeometry;
        private readonly MeshGeometry3D _shotGeometry;

        private readonly GameEngine _gameEngine;

        private readonly Dictionary<GameObjectId, ChickenData> _chickenDatas =
            new Dictionary<GameObjectId, ChickenData>();

        private readonly Dictionary<ModelVisual3D, ChickenData> _chickenVisualToData =
            new Dictionary<ModelVisual3D, ChickenData>();

        private readonly Dictionary<GameObjectId, ShotData> _shotDatas =
            new Dictionary<GameObjectId, ShotData>();

        private Point3D _defaultCameraPosition;
        private Vector3D _defaultCameraLookDirection;
        private Vector3D _defaultCameraUpDirection;
        private double _defaultCameraFieldOfView;

        private GameObjectId? _followedChickenId;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameWindow"/> class.
        /// </summary>
        private GameWindow()
        {
            InitializeComponent();

            _uiCellSize = 1d;
            _nominalSizeCoefficient = GameConstants.NominalCellSize;

            _chickenGeometry = CreateChickenGeometry(_nominalSizeCoefficient);
            _shotGeometry = CreateShotGeometry(_nominalSizeCoefficient);

            SaveCameraDefaults();

            this.CopyrightTextVisual.Text = App.Current.FullProductDescription;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameWindow"/> class
        ///     using the specified parameters.
        /// </summary>
        public GameWindow(System.Drawing.Size nominalSize, ChickenTeamRecord lightTeam, ChickenTeamRecord darkTeam)
            : this()
        {
            var settings = new GameEngineSettings(nominalSize, lightTeam, darkTeam, this.PaintGame);
            _gameEngine = new GameEngine(settings);
            _gameEngine.GameEnded += this.GameEngine_GameEnded;

            this.Title = string.Format(
                "{0} [{1}x{2}] [L: {3}x {4}  -vs-  D: {5}x {6}]",
                this.Title,
                nominalSize.Width,
                nominalSize.Height,
                lightTeam.UnitCount,
                lightTeam.Type.Name,
                darkTeam.UnitCount,
                darkTeam.Type.Name);
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
                    @"The value must be positive.");
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
                    @"The value must be positive.");
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

            //// TODO: [3D] Draw target points for random chickens

            try
            {
                var boardSize = _gameEngine.Data.NominalSize;
                var evenCellsMeshBuilder = new MeshBuilder();
                var oddCellsMeshBuilder = new MeshBuilder();

                for (var x = 0; x < boardSize.Width; x++)
                {
                    for (var y = 0; y < boardSize.Height; y++)
                    {
                        var builder = (x + y) % 2 == 0 ? evenCellsMeshBuilder : oddCellsMeshBuilder;

                        builder.AddBox(
                            new Rect3D(
                                x * _uiCellSize,
                                y * _uiCellSize,
                                -BoardThickness,
                                _uiCellSize,
                                _uiCellSize,
                                BoardThickness));
                    }
                }

                this.BoardEvenCellModel.Geometry = evenCellsMeshBuilder.ToMesh();
                this.BoardOddCellModel.Geometry = oddCellsMeshBuilder.ToMesh();

                var boardCenter = new Vector3D(
                    boardSize.Width * _uiCellSize / 2,
                    boardSize.Height * _uiCellSize / 2,
                    0d);
                this.Camera.Position += boardCenter;
                this.Camera.LookDirection = boardCenter.ToPoint3D() - this.Camera.Position;

                _followedChickenId = null;
                SaveCameraDefaults();

                ////ClearStatusLabels();
                ////UpdateMoveCountStatus();

                _gameEngine.CallPaint();
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
                var currentPosition = chicken.GetCurrentPosition();

                var matrix = GetChickenTransformMatrix(chicken);
                var transform = new MatrixTransform3D(matrix);
                var position = ConvertEnginePosition(currentPosition.Position);

                var chickenVisual = new ModelVisual3D
                {
                    Content = new GeometryModel3D(_chickenGeometry, material)
                    {
                        Transform = transform
                    }
                };

                var data = new ChickenData(
                    chicken.UniqueId,
                    position,
                    currentPosition.Angle.DegreeValue,
                    chickenVisual,
                    transform);
                _chickenDatas.Add(data.Id, data);
                _chickenVisualToData.Add(chickenVisual, data);
                this.BoardVisual.Children.Add(chickenVisual);
            }
        }

        private Point3D ConvertEnginePosition(Physics.Point2D position)
        {
            return new Point3D(
                position.X / _nominalSizeCoefficient,
                position.Y / _nominalSizeCoefficient,
                GameConstants.ChickenUnit.BodyCircleRadius / _nominalSizeCoefficient);
        }

        private Matrix3D GetChickenTransformMatrix(ChickenPresentation chicken)
        {
            var result = Matrix3D.Identity;

            var currentPosition = chicken.GetCurrentPosition();

            result.Rotate(new Quaternion(new Vector3D(0d, 0d, 1d), currentPosition.Angle.DegreeValue));

            var normalizedPosition = currentPosition.Position / (float)_nominalSizeCoefficient;
            result.Translate(new Vector3D(normalizedPosition.X, normalizedPosition.Y, 0d));

            return result;
        }

        private Matrix3D GetShotTransformMatrix(ShotPresentation shot)
        {
            var result = Matrix3D.Identity;

            var currentPosition = shot.GetCurrentPosition();

            var normalizedPosition = currentPosition / (float)_nominalSizeCoefficient;
            result.Translate(new Vector3D(normalizedPosition.X, normalizedPosition.Y, 0d));

            return result;
        }

        private void ClearData()
        {
            this.BoardVisual.Children.Clear();
            this.ShotsModelGroup.Children.Clear();

            _chickenDatas.Clear();
            _chickenVisualToData.Clear();
            _shotDatas.Clear();
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

            PaintGameNoThreadCheck(e.Presentation);
        }

        private void PaintGameNoThreadCheck(GamePresentation presentation)
        {
            if (!_chickenDatas.Any())
            {
                InitializeVisualData(presentation);
            }

            var deadChickenIds = new HashSet<GameObjectId>(_chickenDatas.Keys);
            foreach (var chicken in presentation.Chickens)
            {
                var data = _chickenDatas[chicken.UniqueId];
                var currentPosition = chicken.GetCurrentPosition();
                data.Position = ConvertEnginePosition(currentPosition.Position);
                data.BeakAngle = currentPosition.Angle.DegreeValue;
                data.Transform.Matrix = GetChickenTransformMatrix(chicken);

                deadChickenIds.Remove(data.Id);
            }

            foreach (var deadChickenId in deadChickenIds)
            {
                var data = _chickenDatas[deadChickenId];
                this.BoardVisual.Children.Remove(data.Visual);
                _chickenDatas.Remove(deadChickenId);
                _chickenVisualToData.Remove(data.Visual);
            }

            var explodedShotIds = new HashSet<GameObjectId>(_shotDatas.Keys);
            foreach (var shot in presentation.Shots)
            {
                var currentPosition = shot.GetCurrentPosition();

                var data = _shotDatas.GetValueOrDefault(shot.UniqueId);
                if (data == null)
                {
                    var material = shot.OwnerTeam == GameTeam.Light ? LightTeamShotMaterial : DarkTeamShotMaterial;
                    var matrix = GetShotTransformMatrix(shot);
                    var transform = new MatrixTransform3D(matrix);
                    var position = ConvertEnginePosition(currentPosition);

                    var model = new GeometryModel3D(_shotGeometry, material)
                    {
                        Transform = transform
                    };

                    data = new ShotData(shot.UniqueId, position, model, transform);
                    _shotDatas.Add(data.Id, data);
                    this.ShotsModelGroup.Children.Add(model);
                }
                else
                {
                    var position = ConvertEnginePosition(currentPosition);
                    data.Position = position;

                    data.Transform.Matrix = GetShotTransformMatrix(shot);
                    explodedShotIds.Remove(data.Id);
                }
            }

            foreach (var explodedShotId in explodedShotIds)
            {
                var data = _shotDatas[explodedShotId];
                this.ShotsModelGroup.Children.Remove(data.Model);
                _shotDatas.Remove(explodedShotId);
            }

            FollowChicken();
        }

        private void StartGame()
        {
            try
            {
                if (_gameEngine.IsRunning)
                {
                    return;
                }

                var winningTeam = _gameEngine.WinningTeam;
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

                _gameEngine.Start();
                ////UpdateMoveCountStatus();
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

        private void StopGame()
        {
            try
            {
                if (_gameEngine == null || !_gameEngine.IsRunning)
                {
                    return;
                }

                _gameEngine.Stop();
                ////UpdateMoveCountStatus();
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

        private void ResetGame()
        {
            StopGame();

            RestoreCameraDefaults();

            ClearData();

            _gameEngine.Reset();
        }

        private void SaveCameraDefaults()
        {
            _defaultCameraPosition = this.Camera.Position;
            _defaultCameraLookDirection = this.Camera.LookDirection;
            _defaultCameraUpDirection = this.Camera.UpDirection;
            _defaultCameraFieldOfView = this.Camera.FieldOfView;
        }

        private void RestoreCameraDefaults()
        {
            _followedChickenId = null;

            this.Camera.Position = _defaultCameraPosition;
            this.Camera.LookDirection = _defaultCameraLookDirection;
            this.Camera.UpDirection = _defaultCameraUpDirection;
            this.Camera.FieldOfView = _defaultCameraFieldOfView;
            this.Camera.Transform = null;
        }

        private void StartFollowingChicken(GameObjectId chickenId)
        {
            _followedChickenId = chickenId;

            var data = _chickenDatas.GetValueOrDefault(_followedChickenId.Value);
            if (data == null)
            {
                RestoreCameraDefaults();
                return;
            }

            this.Camera.Position = new Point3D(
                GameConstants.ChickenUnit.BodyCircleRadius / _nominalSizeCoefficient,
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
            if (!_followedChickenId.HasValue)
            {
                return;
            }

            var data = _chickenDatas.GetValueOrDefault(_followedChickenId.Value);
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
                        if (_gameEngine.IsRunning)
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
            var presentation = _gameEngine.GetPresentation();
            PaintGameNoThreadCheck(presentation);
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

            _gameEngine.Stop();
            _gameEngine.CallPaint();

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
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_gameEngine.IsRunning)
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

            var data = _chickenVisualToData.GetValueOrDefault(visual);
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