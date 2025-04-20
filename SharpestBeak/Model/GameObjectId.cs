using System;
using System.Diagnostics;
using System.Globalization;

namespace SharpestBeak.Model;

public readonly struct GameObjectId : IEquatable<GameObjectId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjectId"/> structure.
    /// </summary>
    internal GameObjectId(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The value cannot be negative.");
        }

        Value = value;
    }

    [DebuggerNonUserCode]
    public static GameObjectId None { get; } = new();

    [DebuggerNonUserCode]
    public bool IsValid => Value > 0;

    [DebuggerNonUserCode]
    public bool IsNone => Value == None.Value;

    [DebuggerNonUserCode]
    private int Value { get; }

    public override bool Equals(object obj) => obj is GameObjectId castObj && Equals(castObj);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => $"[{GetType().Name} #{Value}]";

    public string GetValueAsString() => Value.ToString(CultureInfo.InvariantCulture);

    public bool Equals(GameObjectId other) => Value == other.Value;
}