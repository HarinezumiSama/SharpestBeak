using System;
using System.Diagnostics;
using System.Drawing;
using Omnifactotum.Annotations;
using SharpestBeak.Model;
using SharpestBeak.Presentation;

namespace SharpestBeak.Configuration;

public sealed class GameEngineSettings
{
    [NotNull]
    private Action<GamePositionEventArgs> _positionCallback;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameEngineSettings"/> class.
    /// </summary>
    public GameEngineSettings(
        Size nominalSize,
        ChickenTeamSettings lightTeamSettings,
        ChickenTeamSettings darkTeamSettings,
        Action<GamePaintEventArgs> paintCallback)
    {
        if (!nominalSize.Width.IsInRange(GameConstants.NominalCellCountRange) || !nominalSize.Height.IsInRange(GameConstants.NominalCellCountRange))
        {
            throw new ArgumentException(
                $"Each nominal board size dimension must be in the range {GameConstants.NominalCellCountRange.Min} to {
                    GameConstants.NominalCellCountRange.Max}.",
                nameof(nominalSize));
        }

        NominalSize = nominalSize;
        LightTeamSettings = lightTeamSettings ?? throw new ArgumentNullException(nameof(lightTeamSettings));
        DarkTeamSettings = darkTeamSettings ?? throw new ArgumentNullException(nameof(darkTeamSettings));
        PaintCallback = paintCallback ?? throw new ArgumentNullException(nameof(paintCallback));
        _positionCallback = UnitPositioningHelper.PositionRandomly;
    }

    public Size NominalSize { get; }

    public ChickenTeamSettings LightTeamSettings { get; }

    public ChickenTeamSettings DarkTeamSettings { get; }

    public Action<GamePaintEventArgs> PaintCallback { get; }

    [NotNull]
    public Action<GamePositionEventArgs> PositionCallback
    {
        [DebuggerStepThrough]
        get => _positionCallback;

        set => _positionCallback = value ?? throw new ArgumentNullException(nameof(value));
    }
}