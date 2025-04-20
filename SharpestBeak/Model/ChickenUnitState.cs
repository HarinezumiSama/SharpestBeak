using System;
using System.Diagnostics;
using SharpestBeak.Physics;
using SharpestBeak.View;

namespace SharpestBeak.Model;

public sealed class ChickenUnitState : IDirectionalPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenUnitState"/> class.
    /// </summary>
    internal ChickenUnitState(ChickenUnit unit, MoveInfo previousMove, MoveInfoStates previousMoveState)
    {
        Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        UniqueId = unit.UniqueId;
        Team = unit.Team;
        IsDead = unit.IsDead;
        Position = unit.Position;
        BeakAngle = unit.BeakAngle;
        PreviousMove = previousMove ?? throw new ArgumentNullException(nameof(previousMove));
        PreviousMoveState = previousMoveState;
        View = new ViewInfo(unit);
        CanShoot = unit.CanShoot();
    }

    public GameObjectId UniqueId { get; }

    public GameTeam Team { get; }

    public bool IsDead { get; }

    public Point2D Position { get; }

    public GameAngle BeakAngle { get; }

    public MoveInfo PreviousMove { get; }

    public MoveInfoStates PreviousMoveState { get; }

    public ViewInfo View { get; }

    public bool CanShoot { get; }

    [DebuggerNonUserCode]
    Point2D IDirectionalPosition.Position => Position;

    [DebuggerNonUserCode]
    GameAngle IDirectionalPosition.Angle => BeakAngle;

    internal ChickenUnit Unit { get; }

    public override string ToString() => $"[{GetType().Name} #{UniqueId}] Position = {Position}, BeakAngle = {BeakAngle:D}, Team = {Team}, IsDead = {IsDead}";
}