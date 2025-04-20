namespace SharpestBeak.Physics;

public interface IDirectionalPosition
{
    Point2D Position { get; }

    GameAngle Angle { get; }
}