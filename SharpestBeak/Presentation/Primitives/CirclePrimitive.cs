using System;
using System.Diagnostics;
using Newtonsoft.Json;
using SharpestBeak.Physics;
using SharpestBeak.Recording;

namespace SharpestBeak.Presentation.Primitives;

public sealed class CirclePrimitive : BasePrimitive
{
    private float _radiusSqr = float.MinValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CirclePrimitive"/> class.
    /// </summary>
    public CirclePrimitive(Point2D center, float radius)
    {
        if (radius.IsNegativeOrZero())
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "Circle radius must be positive.");
        }

        Center = center;
        Radius = radius;
        BasePoint = center;
    }

    public Point2D Center { get; }

    public float Radius { get; }

    [DebuggerNonUserCode]
    [JsonIgnore]
    public float RadiusSquared
    {
        get
        {
            if (_radiusSqr <= 0f)
            {
                _radiusSqr = Radius.Sqr();
            }

            return _radiusSqr;
        }
    }

    public override bool HasCollision(ICollidable other) => CollisionDetector.CheckPrimitiveCollision(this, other);

    public override bool HasCollision(ICollidablePrimitive other)
    {
        if (other is null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        var result = other switch
        {
            CirclePrimitive otherCircle => CollisionDetector.CheckCircleToCircleCollision(this, otherCircle),
            LinePrimitive line => CollisionDetector.CheckLineToCircleCollision(line, this),
            ConvexPolygonPrimitive polygon => CollisionDetector.CheckCircleToPolygonCollision(this, polygon),
            _ => throw new ArgumentException($"Unexpected object type {other.GetType().GetFullName().ToUIString()}.", nameof(other))
        };

        CollisionCheckRecorder.RecordCollisionCheck(this, other, result);
        return result;
    }
}