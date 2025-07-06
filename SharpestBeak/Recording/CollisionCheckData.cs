#nullable enable

using System;
using System.Diagnostics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak.Recording;

[DebuggerDisplay("{ToDebuggerString(),nq}")]
public sealed class CollisionCheckData(int Index, BasePrimitive First, BasePrimitive Second, bool Result)
{
    public int Index { get; } = Index;

    public BasePrimitive First { get; } = First ?? throw new ArgumentNullException(nameof(First));

    public BasePrimitive Second { get; } = Second ?? throw new ArgumentNullException(nameof(Second));

    public bool Result { get; } = Result;

    internal string ToDebuggerString() => $"{{ {First.ToDebuggerString()} ~ {Second.ToDebuggerString()} => {Result} }}";
}