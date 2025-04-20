using System;
using System.Diagnostics;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.View;

public sealed class ShotViewData : BaseViewData, IDirectionalPosition
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ShotViewData"/> class.
    /// </summary>
    internal ShotViewData(ShotUnit unit)
    {
        if (unit is null)
        {
            throw new ArgumentNullException(nameof(unit));
        }

        UniqueId = unit.UniqueId;
        Position = unit.Position;
        Angle = unit.Angle;
        OwnerUniqueId = unit.Owner.UniqueId;
        Team = unit.Owner.Team;
    }

    public GameObjectId UniqueId { get; }

    public Point2D Position { get; }

    public GameAngle Angle { get; }

    [DebuggerNonUserCode]
    Point2D IDirectionalPosition.Position => Position;

    [DebuggerNonUserCode]
    GameAngle IDirectionalPosition.Angle => Angle;

    internal GameObjectId OwnerUniqueId { get; }

    internal GameTeam Team { get; }

    public override string ToString() => $"[{GetType().Name} #{UniqueId}] Position = {Position}, Angle = {Angle:D}";
}