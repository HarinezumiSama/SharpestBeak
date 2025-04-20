using System;
using SharpestBeak.Model;

namespace SharpestBeak.UI;

public sealed class LogicInfo : IEquatable<LogicInfo>
{
    private readonly string _asString;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogicInfo"/> class.
    /// </summary>
    internal LogicInfo(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var caption = ChickenUnitLogic.GetCaption(type);
        if (caption.IsNullOrWhiteSpace())
        {
            throw new ArgumentException($"Logic '{type.FullName}' has empty caption.", nameof(type));
        }

        Type = type;
        Caption = caption;
        _asString = $"{caption} ({type.Name})";
    }

    public Type Type { get; }

    public string Caption { get; }

    public override bool Equals(object obj) => Equals(obj as LogicInfo);

    public override int GetHashCode() => Type.GetHashCode();

    public override string ToString() => _asString;

    public bool Equals(LogicInfo other) => other is not null && Type == other.Type;
}