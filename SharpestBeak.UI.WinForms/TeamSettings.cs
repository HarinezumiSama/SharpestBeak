using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SharpestBeak.UI.WinForms
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
                lb.Items.AddRange(LogicManager.Instance.LogicTypes.Cast<object>().ToArray());
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
            this.LogicType = LogicManager.Instance.LogicTypes.FirstOrDefault();
        }

        #endregion

        #region Public Properties

        [Editor(typeof(LogicTypeEditor), typeof(UITypeEditor))]
        public Type LogicType
        {
            get;
            set;
        }

        public int PlayerCount
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var logicType = this.LogicType;
            var logic = logicType == null ? "?" : logicType.Name;
            return string.Format("{{{0} x {1}}}", logic, this.PlayerCount);
        }

        #endregion
    }
}