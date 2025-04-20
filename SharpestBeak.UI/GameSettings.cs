using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using Omnifactotum.Annotations;
using SharpestBeak.UI.Properties;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI;

public sealed class GameSettings : NotifyPropertyChangedBase
{
    private PositionMode _positionMode;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameSettings"/> class.
    /// </summary>
    public GameSettings()
    {
        NominalSize = new SizeObject(Settings.Default.NominalSize);
        PositionMode = PositionMode.Random;

        LightTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
        DarkTeam = new TeamSettings { PlayerCount = Settings.Default.TeamUnitCount };
    }

    [DisplayName(@"Nominal board size")]
    [ExpandableObject]
    [PropertyOrder(1)]
    public SizeObject NominalSize { get; }

    [DisplayName(@"Position mode")]
    [PropertyOrder(2)]
    public PositionMode PositionMode
    {
        get => _positionMode;

        [UsedImplicitly]
        set
        {
            if (value == _positionMode)
            {
                return;
            }

            _positionMode = value;
            RaisePropertyChanged(obj => obj.PositionMode);
        }
    }

    [DisplayName(@"Light team")]
    [ExpandableObject]
    [PropertyOrder(3)]
    public TeamSettings LightTeam { get; }

    [DisplayName(@"Dark team")]
    [ExpandableObject]
    [PropertyOrder(4)]
    public TeamSettings DarkTeam { get; }

    public string Validate()
    {
        var resultBuilder = new StringBuilder();

        if (!NominalSize.Width.IsInRange(GameConstants.NominalCellCountRange)
            || !NominalSize.Height.IsInRange(GameConstants.NominalCellCountRange))
        {
            resultBuilder
                .AppendFormat(
                    "Each nominal board size dimension must be in the range {0} to {1}.",
                    GameConstants.NominalCellCountRange.Min,
                    GameConstants.NominalCellCountRange.Max)
                .AppendLine();
        }

        LightTeam.ValidateInternal(resultBuilder, "Light team");
        DarkTeam.ValidateInternal(resultBuilder, "Dark team");

        return resultBuilder.Length != 0 ? resultBuilder.ToString() : null;
    }

    // Do not delete ShouldSerializeNominalSize method - called by TypeDescriptor
    private bool ShouldSerializeNominalSize() => false;

    // Do not delete ShouldSerializeLightTeam method - called by TypeDescriptor
    private bool ShouldSerializeLightTeam() => false;

    // Do not delete ShouldSerializeDarkTeam method - called by TypeDescriptor
    private bool ShouldSerializeDarkTeam() => false;

    private void RaisePropertyChanged<TProperty>(Expression<Func<GameSettings, TProperty>> propertyExpression)
    {
        base.RaisePropertyChanged(propertyExpression);
    }
}