using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TeamSettings
    {
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
        [RefreshProperties(RefreshProperties.All)]
        [PropertyOrder(1)]
        public LogicInfo Logic
        {
            get;
            set;
        }

        [DisplayName(@"Player count")]
        [RefreshProperties(RefreshProperties.All)]
        [PropertyOrder(2)]
        public int PlayerCount
        {
            get;
            set;
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
    }
}