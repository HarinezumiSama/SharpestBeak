using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Presentation.Elements;

public sealed class ShotElement : BaseElement, ICollidableElement
{
    private readonly ReadOnlyCollection<ICollidablePrimitive> _primitives;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShotElement"/> class.
    /// </summary>
    internal ShotElement(Point2D position)
        : base(position)
        => _primitives = new ICollidablePrimitive[]
            {
                new CirclePrimitive(position, GameConstants.ShotUnit.Radius)
            }
            .AsReadOnly();

    public override string ToString() => $"{GetType().Name}. Position = {Position}";

    public bool HasRoughPrimitives => false;

    public ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives() => throw new NotSupportedException();

    public ReadOnlyCollection<ICollidablePrimitive> GetPrimitives() => _primitives;

    public bool HasCollision(ICollidable other) => CollisionDetector.CheckElementCollision(this, other);
}