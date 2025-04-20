using System.Collections.ObjectModel;

namespace SharpestBeak.Presentation;

public interface ICollidableElement : ICollidable
{
    bool HasRoughPrimitives { get; }

    ReadOnlyCollection<ICollidablePrimitive> GetRoughPrimitives();

    ReadOnlyCollection<ICollidablePrimitive> GetPrimitives();
}