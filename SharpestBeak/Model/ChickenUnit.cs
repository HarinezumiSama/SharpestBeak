using System;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model;

internal sealed class ChickenUnit
{
    private ChickenElement _cachedElement;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenUnit"/> class.
    /// </summary>
    internal ChickenUnit(ChickenUnitLogicExecutor logicExecutor)
    {
        LogicExecutor = logicExecutor ?? throw new ArgumentNullException(nameof(logicExecutor));
        Engine = logicExecutor.Engine.EnsureNotNull();
        Team = logicExecutor.Team;

        Reset();
    }

    public ChickenUnitLogicExecutor LogicExecutor { get; }

    public GameEngine Engine { get; }

    public GameObjectId UniqueId { get; internal set; }

    public GameTeam Team { get; }

    public Point2D Position { get; private set; }

    public Vector2D Movement { get; private set; }

    public Point2D NextPosition { get; private set; }

    public GameAngle BeakAngle { get; private set; }

    public GameAngle BeakMovement { get; private set; }

    public GameAngle NextBeakAngle { get; private set; }

    public ChickenUnit KilledBy { get; internal set; }

    public bool IsDead { get; internal set; }

    public int KillCount { get; internal set; }

    public long ShotEngineStepIndex { get; set; }

    public override string ToString() => $"[{ToShortString()}] Position = {Position}, BeakAngle = {BeakAngle:D}, Team = {Team}, IsDead = {IsDead}";

    public string ToShortString() => $"{GetType().Name} #{UniqueId}";

    public ChickenElement GetElement() => _cachedElement ??= new ChickenElement(NextPosition, NextBeakAngle);

    internal void Reset()
    {
        IsDead = false;
        ShotEngineStepIndex = -1;
        KillCount = 0;
        KilledBy = null;
    }

    internal bool CanSee(Point2D point)
    {
        var viewDirection = GameHelper.GetBeakTipPosition(Position, BeakAngle) - Position;
        var pointDirection = point - Position;
        return viewDirection.GetAngle(pointDirection).DegreeValue.Abs() <= GameConstants.ChickenUnit.ViewAngle;
    }

    internal bool CanShoot() => ShotEngineStepIndex < 0 || Engine.MoveCount - ShotEngineStepIndex >= GameConstants.ShotUnit.EngineStepsBetweenShots;

    internal void SetMovement(Vector2D movement, GameAngle beakMovement)
    {
        Movement = movement;
        NextPosition = Position + Movement;

        BeakMovement = beakMovement;
        NextBeakAngle = BeakAngle + BeakMovement;

        ResetCachedElement();
    }

    internal void ApplyMovement()
    {
        Position = NextPosition;
        Movement = Vector2D.Zero;

        BeakAngle = NextBeakAngle;
        BeakMovement = GameAngle.Zero;

        ResetCachedElement();
    }

    internal void ResetPosition(Point2D position, GameAngle beakAngle)
    {
        Position = position;
        Movement = Vector2D.Zero;
        NextPosition = Position;

        BeakAngle = beakAngle;
        BeakMovement = GameAngle.Zero;
        NextBeakAngle = BeakAngle;

        ResetCachedElement();
    }

    private void ResetCachedElement() => _cachedElement = null;
}