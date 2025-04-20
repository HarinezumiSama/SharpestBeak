using System;
using System.Drawing;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;

namespace SharpestBeak;

public sealed class GameEngineData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameEngineData"/> class.
    /// </summary>
    internal GameEngineData(Size nominalSize)
    {
        if (!nominalSize.Width.IsInRange(GameConstants.NominalCellCountRange)
            || !nominalSize.Height.IsInRange(GameConstants.NominalCellCountRange))
        {
            throw new ArgumentException(
                $"Each nominal board size dimension must be in the range {GameConstants.NominalCellCountRange.Min} to {
                    GameConstants.NominalCellCountRange.Max}.",
                nameof(nominalSize));
        }

        NominalSize = nominalSize;

        RealSize = new SizeF(
            GameConstants.NominalCellSize * nominalSize.Width,
            GameConstants.NominalCellSize * nominalSize.Height);

        BoardBorder = new ConvexPolygonPrimitive(
            Point2D.Zero,
            new Point2D(RealSize.Width, 0f),
            new Point2D(RealSize),
            new Point2D(0f, RealSize.Height));
    }

    public Size NominalSize { get; }

    public SizeF RealSize { get; }

    public ConvexPolygonPrimitive BoardBorder { get; }
}