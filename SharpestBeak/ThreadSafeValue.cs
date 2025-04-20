using System;
using System.Diagnostics;

namespace SharpestBeak;

public sealed class ThreadSafeValue<T>
{
    private T _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThreadSafeValue{T}"/> class.
    /// </summary>
    public ThreadSafeValue(object @lock, T value = default)
    {
        Lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
        _value = value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ThreadSafeValue{T}"/> class.
    /// </summary>
    public ThreadSafeValue(T value = default)
        : this(new object(), value)
    {
        // Nothing to do
    }

    public object Lock { get; }

    public T Value
    {
        [DebuggerNonUserCode]
        get
        {
            lock (Lock)
            {
                return _value;
            }
        }

        [DebuggerNonUserCode]
        set
        {
            lock (Lock)
            {
                _value = value;
            }
        }
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}