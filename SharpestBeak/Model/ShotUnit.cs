using System;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Model;

internal sealed class ShotUnit
{
    private ShotElement _cachedElement;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShotUnit"/> class.
    /// </summary>
    internal ShotUnit(ChickenUnit owner, GameObjectId uniqueId)
    {
        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        if (!uniqueId.IsValid)
        {
            throw new ArgumentException("Invalid shot ID.", nameof(uniqueId));
        }

        var beakTipPosition = GameHelper.GetBeakTipPosition(owner.Position, owner.BeakAngle);

        UniqueId = uniqueId;
        Owner = owner;
        Position = GameHelper.GetNewPosition(beakTipPosition, owner.BeakAngle, GameConstants.ShotUnit.Radius);
        Angle = owner.BeakAngle;
        CreationTime = DateTime.Now;
    }

    public GameObjectId UniqueId { get; }

    public ChickenUnit Owner { get; }

    public GameAngle Angle { get; }

    public Point2D Position { get; private set; }

    public Vector2D Movement { get; private set; }

    public Point2D NextPosition { get; private set; }

    public bool Exploded { get; internal set; }

    internal DateTime CreationTime { get; }

    public override string ToString()
        => $"[{GetType().Name} : #{UniqueId}] Position = {Position}, Angle = {Angle:D}, Owner = #{Owner.UniqueId}, CreationTime = {CreationTime:HH:mm:ss.fff}";

    public ShotElement GetElement() => _cachedElement ??= new ShotElement(NextPosition);

    internal void SetMovement(Vector2D movement)
    {
        //// TODO [VM] Temporary argument check
        if (movement.GetLength().IsZero())
        {
            throw new ArgumentException("Shot must always move until exploded.", nameof(movement));
        }

        Movement = movement;
        NextPosition = Position + Movement;

        ResetCachedElement();
    }

    internal void ApplyMovement()
    {
        Position = NextPosition;
        Movement = Vector2D.Zero;

        ResetCachedElement();
    }

    private void ResetCachedElement() => _cachedElement = null;
}