namespace SharpestBeak.Presentation;

public interface ICollidable
{
    bool HasCollision(ICollidable other);
}