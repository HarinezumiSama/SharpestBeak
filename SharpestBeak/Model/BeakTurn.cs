using System;

namespace SharpestBeak.Model;

[Serializable]
public readonly struct BeakTurn : IEquatable<BeakTurn>, IComparable<BeakTurn>
{
    public const float MinValue = -1f;
    public const float MaxValue = 1f;

    public static readonly ValueRange<float> ValueRange = new(MinValue, MaxValue);

    public static readonly BeakTurn FullyClockwise = new(MinValue);
    public static readonly BeakTurn None = new();
    public static readonly BeakTurn FullyCounterClockwise = new(MaxValue);

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeakTurn"/> class.
    /// </summary>
    public BeakTurn(float value)
    {
        if (!value.IsInRange(ValueRange))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"The value must be in the range {ValueRange.Min} to {ValueRange.Max}.");
        }

        Value = value;
    }

    public float Value { get; }

    public override bool Equals(object obj) => obj is BeakTurn turn && Equals(turn);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => $"{GetType().Name}. Value = {Value}";

    //// ReSharper disable once CompareOfFloatsByEqualityOperator
    public bool Equals(BeakTurn other) => Value == other.Value;

    public int CompareTo(BeakTurn other) => Value.CompareTo(other.Value);
}