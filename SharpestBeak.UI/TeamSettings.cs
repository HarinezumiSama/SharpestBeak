using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TeamSettings : NotifyPropertyChangedBase
    {
        #region Constants and Fields

        private LogicInfo _logic;
        private int _playerCount;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamSettings"/> class.
        /// </summary>
        public TeamSettings()
        {
            this.Logic = LogicManager.Instance.Logics.FirstOrDefault();
        }

        #endregion

        #region Public Properties

        [DisplayName(@"Logic")]
        [ItemsSource(typeof(LogicInfoItemSource))]
        [PropertyOrder(1)]
        public LogicInfo Logic
        {
            [DebuggerStepThrough]
            get
            {
                return _logic;
            }

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
            get
            {
                return _playerCount;
            }

            set
            {
                if (value == _playerCount)
                {
                    return;
                }

                if (!GameConstants.TeamPlayerUnitCountRange.Belongs(value))
                {
                    throw new ArgumentOutOfRangeException(
                        "value",
                        value,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"The value is out of the valid range {0}.",
                            GameConstants.TeamPlayerUnitCountRange));
                }

                _playerCount = value;
                RaisePropertyChanged(obj => obj.PlayerCount);
                RaisePropertyChanged(obj => obj.AsString);
            }
        }

        [Browsable(false)]
        public string AsString
        {
            get
            {
                return ToString();
            }
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var logicType = this.Logic;
            var logic = logicType == null ? "?" : logicType.Caption;
            return string.Format("{{{0}x {1}}}", this.PlayerCount, logic);
        }

        #endregion

        #region Internal Methods

        internal void ValidateInternal(StringBuilder messageBuilder, string prefix)
        {
            #region Argument Check

            if (messageBuilder == null)
            {
                throw new ArgumentNullException("messageBuilder");
            }

            if (string.IsNullOrEmpty(prefix))
            {
                throw new ArgumentException(@"The value can be neither empty string nor null.", "prefix");
            }

            #endregion

            if (this.Logic == null)
            {
                messageBuilder
                    .AppendFormat("{0}: logic must be specified.", prefix)
                    .AppendLine();
            }

            if (this.PlayerCount <= 0)
            {
                messageBuilder
                    .AppendFormat("{0}: player count must be positive.", prefix)
                    .AppendLine();
            }
        }

        #endregion

        #region Private Methods

        private void RaisePropertyChanged<TProperty>(Expression<Func<TeamSettings, TProperty>> propertyExpression)
        {
            base.RaisePropertyChanged(propertyExpression);
        }

        #endregion
    }
}