using System;
using System.Diagnostics;
using Newtonsoft.Json;
using SharpestBeak.Physics;
using SharpestBeak.Recording;

namespace SharpestBeak.Presentation.Primitives;

public sealed class LinePrimitive : BasePrimitive
{
    private Vector2D? _direction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LinePrimitive"/> class.
    /// </summary>
    public LinePrimitive(Point2D start, Point2D end)
    {
        if (start.GetDistance(end).IsZero())
        {
            throw new ArgumentException($"Line endpoints cannot be the same ({start} and {end}).", nameof(end));
        }

        Start = start;
        End = end;
        BasePoint = start;
    }

    public Point2D Start { get; }

    public Point2D End { get; }

    [DebuggerNonUserCode]
    [JsonIgnore]
    public Vector2D Direction => _direction ??= End - Start;

    public LineSide GetLineSide(Point2D point) => CollisionDetector.GetLineSide(this, point);

    public override bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);

    public override bool HasCollision(ICollidablePrimitive other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        var result = other switch
        {
            LinePrimitive line => CollisionDetector.CheckLineToLineCollision(this, line),
            CirclePrimitive circle => CollisionDetector.CheckLineToCircleCollision(this, circle),
            ConvexPolygonPrimitive polygon => CollisionDetector.CheckLineToPolygonCollision(this, polygon),
            _ => throw new ArgumentException($"Unexpected object type {other.GetType().GetFullName().ToUIString()}.", nameof(other))
        };

        CollisionCheckRecorder.RecordCollisionCheck(this, other, result);
        return result;
    }
}