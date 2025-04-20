using System;
using System.Collections.Generic;
using Omnifactotum.Annotations;

namespace SharpestBeak;

public readonly struct ValueRange<T>
    where T : struct, IComparable<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueRange{T}"/> structure.
    /// </summary>
    public ValueRange(T min, T max)
    {
        if (Comparer<T>.Default.Compare(min, max) > 0)
        {
            throw new ArgumentException($"The minimum ({min}) is greater than maximum ({max}).");
        }

        Min = min;
        Max = max;
    }

    public T Min { get; }

    public T Max { get; }

    public override string ToString() => $"[{Min} .. {Max}]";

    /// <summary>
    ///     Determines whether this range contains the specified value.
    /// </summary>
    /// <param name="value">
    ///     The value to check.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if this range contains the specified value; otherwise, <see langword="false"/>.
    /// </returns>
    [Pure]
    public bool Contains(T value) => value.IsInRange(this);
}