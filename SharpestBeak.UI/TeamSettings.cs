﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SharpestBeak.UI
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TeamSettings
    {
        #region Nested Types

        #region LogicTypeEditor Class

        internal sealed class LogicTypeEditor : UITypeEditor
        {
            #region Public Methods

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override bool IsDropDownResizable
            {
                get
                {
                    return true;
                }
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var wfes = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (wfes == null)
                {
                    return base.EditValue(context, provider, value);
                }

                var lb = new ListBox();
                lb.Items.AddRange(LogicManager.Instance.Logics.Cast<object>().ToArray());
                lb.SelectedItem = value;
                lb.SelectionMode = SelectionMode.One;
                lb.Click += (sender, e) => wfes.CloseDropDown();
                wfes.DropDownControl(lb);
                return lb.SelectedItem;
            }

            #endregion
        }

        #endregion

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

        #region Private Methods

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializeLogicType()
        {
            return false;
        }

        // Do not delete this method - called by TypeDescriptor
        private bool ShouldSerializePlayerCount()
        {
            return false;
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
                throw new ArgumentException("The value can be neither empty string nor null.", "prefix");
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

        #region Public Properties

        [DisplayName(@"Logic")]
        [ItemsSource(typeof(LogicInfoItemSource))]
        [RefreshProperties(RefreshProperties.All)]
        public LogicInfo Logic
        {
            get;
            set;
        }

        [DisplayName(@"Player count")]
        [RefreshProperties(RefreshProperties.All)]
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
    }
}