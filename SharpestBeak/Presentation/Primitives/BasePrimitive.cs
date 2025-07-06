#nullable enable

using System.Diagnostics;
using Newtonsoft.Json;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives;

[DebuggerDisplay("{ToDebuggerString(),nq}")]
public abstract class BasePrimitive : ICollidablePrimitive
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BasePrimitive"/> class.
    /// </summary>
    internal BasePrimitive()
    {
        // Nothing to do
    }

    [JsonIgnore]
    public Point2D BasePoint { get; protected set; }

    public abstract bool HasCollision(ICollidable other);

    public abstract bool HasCollision(ICollidablePrimitive other);

    internal abstract string ToDebuggerString();
}