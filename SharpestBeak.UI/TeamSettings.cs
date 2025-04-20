using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Omnifactotum.Annotations;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI;

[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class TeamSettings : NotifyPropertyChangedBase
{
    private LogicInfo _logic;
    private int _playerCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TeamSettings"/> class.
    /// </summary>
    public TeamSettings() => Logic = LogicManager.Instance.Logics.FirstOrDefault();

    [DisplayName(@"Logic")]
    [ItemsSource(typeof(LogicInfoItemSource))]
    [PropertyOrder(1)]
    public LogicInfo Logic
    {
        [DebuggerStepThrough]
        get => _logic;

        [UsedImplicitly]
        set
        {
            if (ReferenceEquals(value, _logic))
            {
                return;
            }

            _logic = value;
            RaisePropertyChanged(obj => obj.Logic);
            RaisePropertyChanged(obj => obj.AsString);
        }
    }

    [DisplayName(@"Player count")]
    [PropertyOrder(2)]
    public int PlayerCount
    {
        [DebuggerStepThrough]
        get => _playerCount;

        set
        {
            if (value == _playerCount)
            {
                return;
            }

            if (!GameConstants.TeamPlayerUnitCountRange.Contains(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    $@"The value is out of the valid range {GameConstants.TeamPlayerUnitCountRange}.");
            }

            _playerCount = value;
            RaisePropertyChanged(obj => obj.PlayerCount);
            RaisePropertyChanged(obj => obj.AsString);
        }
    }

    [Browsable(false)]
    public string AsString => ToString();

    public override string ToString()
    {
        var logicType = Logic;
        var logic = logicType is null ? "?" : logicType.Caption;
        return $"{{{PlayerCount}x {logic}}}";
    }

    internal void ValidateInternal(StringBuilder messageBuilder, string prefix)
    {
        if (messageBuilder is null)
        {
            throw new ArgumentNullException(nameof(messageBuilder));
        }

        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentException(@"The value can be neither empty string nor null.", nameof(prefix));
        }

        if (Logic is null)
        {
            messageBuilder
                .AppendFormat("{0}: logic must be specified.", prefix)
                .AppendLine();
        }

        if (PlayerCount <= 0)
        {
            messageBuilder
                .AppendFormat("{0}: player count must be positive.", prefix)
                .AppendLine();
        }
    }

    private void RaisePropertyChanged<TProperty>(Expression<Func<TeamSettings, TProperty>> propertyExpression) => base.RaisePropertyChanged(propertyExpression);
}