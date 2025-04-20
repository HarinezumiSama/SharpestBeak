using SharpestBeak.Physics;

namespace SharpestBeak.Presentation;

public interface ICollidablePrimitive : ICollidable
{
    Point2D BasePoint { get; }

    bool HasCollision(ICollidablePrimitive other);
}