#nullable enable

using System;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Recording;

public sealed class CollisionCheckData(BasePrimitive first, BasePrimitive second, bool result)
{
    public BasePrimitive First { get; } = first ?? throw new ArgumentNullException(nameof(first));

    public BasePrimitive Second { get; } = second ?? throw new ArgumentNullException(nameof(second));

    public bool Result { get; } = result;
}