using System;
using System.Diagnostics;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.View;

public sealed class ChickenViewData : BaseViewData, IDirectionalPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenViewData"/> class.
    /// </summary>
    internal ChickenViewData(ChickenUnit unit)
    {
        if (unit is null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        UniqueId = unit.UniqueId;
        Position = unit.Position;
        BeakAngle = unit.BeakAngle;
        Team = unit.Team;
    }

    public GameObjectId UniqueId { get; }

    public Point2D Position { get; }

    public GameAngle BeakAngle { get; }

    public GameTeam Team { get; }

    [DebuggerNonUserCode]
    Point2D IDirectionalPosition.Position => Position;

    [DebuggerNonUserCode]
    GameAngle IDirectionalPosition.Angle => BeakAngle;

    public override string ToString() => $"[{GetType().Name} #{UniqueId}] Position = {Position}, BeakAngle = {BeakAngle:D}, Team = {Team}";
}