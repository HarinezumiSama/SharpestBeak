using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements;

public sealed class ChickenElement : BaseElement, ICollidableElement
{
    private readonly ReadOnlyCollection<ICollidablePrimitive> _roughPrimitives;
    private readonly ReadOnlyCollection<ICollidablePrimitive> _primitives;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenElement"/> class.
    /// </summary>
    internal ChickenElement(Point2D position, GameAngle beakAngle)
        : base(position)
    {
        BeakAngle = beakAngle;

        _roughPrimitives = new ICollidablePrimitive[]
            {
                new CirclePrimitive(position, GameConstants.ChickenUnit.BeakOffset)
            }
            .AsReadOnly();

        var defaultBeakPolygonPoints = new[]
        {
            new Point2D(position.X, position.Y - GameConstants.ChickenUnit.BeakRayOffset),
            new Point2D(position.X + GameConstants.ChickenUnit.BeakOffset, position.Y),
            new Point2D(position.X, position.Y + GameConstants.ChickenUnit.BeakRayOffset)
        };

        var beakPolygonPoints = defaultBeakPolygonPoints.Rotate(Position, beakAngle);

        _primitives = new ICollidablePrimitive[]
            {
                new CirclePrimitive(position, GameConstants.ChickenUnit.BodyCircleRadius),
                new ConvexPolygonPrimitive(beakPolygonPoints)
            }
            .AsReadOnly();
    }

    public GameAngle BeakAngle { get; }

    public override string ToString() => $"{GetType().Name}. Position = {Position}, BeakAngle = {BeakAngle:D}";

    public bool HasRoughPrimitives => true;

    public ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives() => _roughPrimitives;

    public ReadOnlyCollection<ICollidablePrimitive> GetPrimitives() => _primitives;

    public bool HasCollision(ICollidable other) => CollisionDetector.CheckElementCollision(this, other);
}