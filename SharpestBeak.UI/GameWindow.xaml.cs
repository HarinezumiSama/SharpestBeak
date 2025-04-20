using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using SharpestBeak.Configuration;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation;
using Size = System.Drawing.Size;

namespace SharpestBeak.UI;

/// <summary>
///     Contains interaction logic for GameWindow.xaml.
/// </summary>
public partial class GameWindow
{
    private static readonly Color LightTeamColor = Colors.Yellow;
    private static readonly Color DarkTeamColor = Color.Multiply(LightTeamColor, 0.3f).WithAlpha(byte.MaxValue);

    private static readonly Color LightTeamCaptionColor = Colors.Lime;

    private static readonly Color DarkTeamCaptionColor =
        Color.Multiply(LightTeamCaptionColor, 0.5f).WithAlpha(byte.MaxValue);

    private static readonly Brush LightTeamBrush = new SolidColorBrush(LightTeamColor);
    private static readonly Brush DarkTeamBrush = new SolidColorBrush(DarkTeamColor);

    private static readonly Material LightTeamUnitMaterial = new DiffuseMaterial(LightTeamBrush);
    private static readonly Material DarkTeamUnitMaterial = new DiffuseMaterial(DarkTeamBrush);

    private static readonly Material LightTeamShotMaterial =
        new DiffuseMaterial(new SolidColorBrush(Colors.Red));

    private static readonly Material DarkTeamShotMaterial =
        new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed));

    private readonly double _uiCellSize;
    private readonly double _nominalSizeCoefficient;
    private readonly MeshGeometry3D _chickenGeometry;
    private readonly MeshGeometry3D _shotGeometry;

    private readonly GameEngine _gameEngine;

    private readonly Dictionary<GameObjectId, ChickenData> _chickenDatas = new();
    private readonly Dictionary<ModelVisual3D, ChickenData> _chickenVisualToData = new();
    private readonly Dictionary<GameObjectId, ShotData> _shotDatas = new();

    private Point3D _defaultCameraPosition;
    private Vector3D _defaultCameraLookDirection;
    private Vector3D _defaultCameraUpDirection;
    private double _defaultCameraFieldOfView;

    private GameObjectId? _followedChickenId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameWindow"/> class
    ///     using the specified parameters.
    /// </summary>
    public GameWindow(
        Size nominalSize,
        ChickenTeamSettings lightTeam,
        ChickenTeamSettings darkTeam,
        PositionMode positionMode)
        : this()
    {
        Action<GamePositionEventArgs> positionCallback = positionMode switch
        {
            PositionMode.Random => UnitPositioningHelper.PositionRandomly,
            PositionMode.LineFight => UnitPositioningHelper.PositionForLineFight,
            _ => throw positionMode.CreateEnumValueNotImplementedException()
        };

        var settings = new GameEngineSettings(nominalSize, lightTeam, darkTeam, PaintGame)
        {
            PositionCallback = positionCallback
        };

        _gameEngine = new GameEngine(settings);
        _gameEngine.GameEnded += GameEngine_GameEnded;

        Title = $"{Title} [{nominalSize.Width}x{nominalSize.Height}] [L: {lightTeam.UnitCount}x {lightTeam.Type.Name}  -vs-  D: {
            darkTeam.UnitCount}x {darkTeam.Type.Name}]";
    }

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

        CopyrightTextVisual.Text = App.Current.FullProductDescription;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        StopGame();
        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        //// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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

    private static MeshGeometry3D CreateChickenGeometry(double nominalSizeCoefficient)
    {
        if (nominalSizeCoefficient <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nominalSizeCoefficient), nominalSizeCoefficient, @"The value must be positive.");
        }

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
        if (nominalSizeCoefficient <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nominalSizeCoefficient), nominalSizeCoefficient, @"The value must be positive.");
        }

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

        //// TODO: [3D] Draw target points for random chickens - ?
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

            BoardEvenCellModel.Geometry = evenCellsMeshBuilder.ToMesh();
            BoardOddCellModel.Geometry = oddCellsMeshBuilder.ToMesh();

            var boardCenter = new Vector3D(
                boardSize.Width * _uiCellSize / 2,
                boardSize.Height * _uiCellSize / 2,
                0d);

            Camera.Position += boardCenter;
            Camera.LookDirection = boardCenter.ToPoint3D() - Camera.Position;

            _followedChickenId = null;
            SaveCameraDefaults();

            ////ClearStatusLabels();
            ////UpdateMoveCountStatus();
            _gameEngine.CallPaint();
        }
        catch (Exception ex)
        {
            if (ex.IsFatal())
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

            var billboardVisual = new BillboardTextVisual3D
            {
                Text = "(ID)",
                FontWeight = FontWeights.ExtraBold,
                FontSize = 9,
                Foreground = new SolidColorBrush(
                    chicken.Team == GameTeam.Light ? LightTeamCaptionColor : DarkTeamCaptionColor),
                Background = new SolidColorBrush(Colors.Gray.WithAlpha(128)),
                BorderBrush = new SolidColorBrush(Colors.Red.WithAlpha(128)),
                BorderThickness = new Thickness(1)
            };

            var data = new ChickenData(
                chicken.UniqueId,
                position,
                currentPosition.Angle.DegreeValue,
                chickenVisual,
                transform,
                billboardVisual);

            _chickenDatas.Add(data.Id, data);
            _chickenVisualToData.Add(chickenVisual, data);
            BoardVisual.Children.Add(chickenVisual);
            BoardVisual.Children.Add(billboardVisual);
        }
    }

    private Point3D ConvertEnginePosition(Point2D position, double offsetZ = 0)
        => new(
            position.X / _nominalSizeCoefficient,
            position.Y / _nominalSizeCoefficient,
            GameConstants.ChickenUnit.BodyCircleRadius / _nominalSizeCoefficient + offsetZ);

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
        BoardVisual.Children.Clear();
        ShotsModelGroup.Children.Clear();

        _chickenDatas.Clear();
        _chickenVisualToData.Clear();
        _shotDatas.Clear();
    }

    private void PaintGame(GamePaintEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(
                (Action<GamePaintEventArgs>)PaintGame,
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

        ////var lightMaxKillCount =
        ////    presentation.Chickens.Where(obj => obj.Team == GameTeam.Light).Max(obj => obj.KillCount);
        ////var darkMaxKillCount =
        ////    presentation.Chickens.Where(obj => obj.Team == GameTeam.Dark).Max(obj => obj.KillCount);
        var deadChickenIds = new HashSet<GameObjectId>(_chickenDatas.Keys);
        foreach (var chicken in presentation.Chickens)
        {
            var data = _chickenDatas[chicken.UniqueId];
            var currentPosition = chicken.GetCurrentPosition();
            data.Position = ConvertEnginePosition(currentPosition.Position);
            data.BeakAngle = currentPosition.Angle.DegreeValue;
            data.Transform.Matrix = GetChickenTransformMatrix(chicken);

            data.BillboardVisual.Position = ConvertEnginePosition(
                currentPosition.Position,
                GameConstants.ChickenUnit.BodyCircleRadius / _nominalSizeCoefficient * 1.5);

            data.BillboardVisual.Text = string.Format(
                CultureInfo.InvariantCulture,
                "#{0}: {1}",
                chicken.UniqueId.GetValueAsString(),
                chicken.KillCount);

            data.BillboardVisual.BorderThickness = new Thickness(1, chicken.KillCount + 1, 1, 1);

            deadChickenIds.Remove(data.Id);
        }

        foreach (var deadChickenId in deadChickenIds)
        {
            var data = _chickenDatas[deadChickenId];
            BoardVisual.Children.Remove(data.Visual);
            BoardVisual.Children.Remove(data.BillboardVisual);
            _chickenDatas.Remove(deadChickenId);
            _chickenVisualToData.Remove(data.Visual);
        }

        var explodedShotIds = new HashSet<GameObjectId>(_shotDatas.Keys);
        foreach (var shot in presentation.Shots)
        {
            var currentPosition = shot.GetCurrentPosition();

            var data = _shotDatas.GetValueOrDefault(shot.UniqueId);
            if (data is null)
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
                ShotsModelGroup.Children.Add(model);
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
            ShotsModelGroup.Children.Remove(data.Model);
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
            if (ex.IsFatal())
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
            if (_gameEngine is not { IsRunning: true })
            {
                return;
            }

            _gameEngine.Stop();

            ////UpdateMoveCountStatus();
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

    private void ResetGame()
    {
        StopGame();

        RestoreCameraDefaults();

        ClearData();

        _gameEngine.Reset();
    }

    private void SaveCameraDefaults()
    {
        _defaultCameraPosition = Camera.Position;
        _defaultCameraLookDirection = Camera.LookDirection;
        _defaultCameraUpDirection = Camera.UpDirection;
        _defaultCameraFieldOfView = Camera.FieldOfView;
    }

    private void RestoreCameraDefaults()
    {
        _followedChickenId = null;

        Camera.Position = _defaultCameraPosition;
        Camera.LookDirection = _defaultCameraLookDirection;
        Camera.UpDirection = _defaultCameraUpDirection;
        Camera.FieldOfView = _defaultCameraFieldOfView;
        Camera.Transform = null;
    }

    private void StartFollowingChicken(GameObjectId chickenId)
    {
        _followedChickenId = chickenId;

        var data = _chickenDatas.GetValueOrDefault(_followedChickenId.Value);
        if (data is null)
        {
            RestoreCameraDefaults();
            return;
        }

        Camera.Position = new Point3D(
            GameConstants.ChickenUnit.BodyCircleRadius / _nominalSizeCoefficient,
            0d,
            0d);

        Camera.LookDirection = new Vector3D(1d, 0d, 0d);
        Camera.UpDirection = new Vector3D(0d, 0d, 1d);
        Camera.FieldOfView = GameConstants.ChickenUnit.ViewAngle * 2d;
        Camera.Transform = null;

        FollowChicken();
    }

    private void FollowChicken()
    {
        if (!_followedChickenId.HasValue)
        {
            return;
        }

        var data = _chickenDatas.GetValueOrDefault(_followedChickenId.Value);
        if (data is null)
        {
            RestoreCameraDefaults();
            return;
        }

        var transformMatrix = Matrix3D.Identity;
        transformMatrix.Rotate(new Quaternion(new Vector3D(0d, 0d, 1d), data.BeakAngle));
        transformMatrix.Translate(data.Position.ToVector3D());

        if (Camera.Transform is not MatrixTransform3D transform)
        {
            transform = new MatrixTransform3D();
            Camera.Transform = transform;
        }

        transform.Matrix = transformMatrix;
    }

    private void CompositionTarget_Rendering(object sender, EventArgs e)
    {
        var presentation = _gameEngine.GetPresentation();
        PaintGameNoThreadCheck(presentation);
    }

    private void GameEngine_GameEnded(object sender, GameEndedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(
                (Action<object, GameEndedEventArgs>)GameEngine_GameEnded,
                DispatcherPriority.Normal,
                sender,
                e);

            return;
        }

        _gameEngine.Stop();
        _gameEngine.CallPaint();

        var winningLogicName = e.WinningLogic is null ? "None" : e.WinningLogic.GetType().Name;

        this.ShowInfoMessage($"Winning team: {e.WinningTeam} ({winningLogicName}).", "Game Ended");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CompositionTarget.Rendering += CompositionTarget_Rendering;

        if (!InitializeGameUI())
        {
            System.Windows.Forms.Application.Exit();
        }
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        CompositionTarget.Rendering -= CompositionTarget_Rendering;
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
        var mousePoint = e.GetPosition(MainViewport);

        if (VisualTreeHelper.HitTest(MainViewport, mousePoint) is not RayMeshGeometry3DHitTestResult hitTestResult)
        {
            return;
        }

        if (hitTestResult.VisualHit is not ModelVisual3D visual)
        {
            return;
        }

        var data = _chickenVisualToData.GetValueOrDefault(visual);
        if (data is null)
        {
            return;
        }

        e.Handled = true;
        StartFollowingChicken(data.Id);
    }

    private abstract class GameObjectData
    {
        protected GameObjectData(
            GameObjectId id,
            Point3D position,
            MatrixTransform3D transform)
        {
            Id = id;
            Position = position;
            Transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public GameObjectId Id { get; }

        public Point3D Position { get; set; }

        public MatrixTransform3D Transform { get; }
    }

    private sealed class ChickenData : GameObjectData
    {
        public ChickenData(
            GameObjectId id,
            Point3D position,
            double beakAngle,
            ModelVisual3D visual,
            MatrixTransform3D transform,
            BillboardTextVisual3D billboardVisual)
            : base(id, position, transform)
        {
            BeakAngle = beakAngle;
            Visual = visual ?? throw new ArgumentNullException(nameof(visual));
            BillboardVisual = billboardVisual ?? throw new ArgumentNullException(nameof(billboardVisual));
        }

        public double BeakAngle { get; set; }

        public ModelVisual3D Visual { get; }

        public BillboardTextVisual3D BillboardVisual { get; }
    }

    private sealed class ShotData : GameObjectData
    {
        public ShotData(
            GameObjectId id,
            Point3D position,
            GeometryModel3D model,
            MatrixTransform3D transform)
            : base(id, position, transform)
            => Model = model ?? throw new ArgumentNullException(nameof(model));

        public GeometryModel3D Model { get; }
    }
}