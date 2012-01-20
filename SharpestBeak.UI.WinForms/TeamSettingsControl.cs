using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SharpestBeak.UI.WinForms
{
    [ToolboxItem(true)]
    public partial class TeamSettingsControl : UserControl
    {
        #region Constructors

        public TeamSettingsControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cbLogic.Items.Clear();
            if (!this.DesignMode)
            {
                this.cbLogic.Items.AddRange(LogicManager.Instance.LogicTypes.Cast<object>().ToArray());
                if (this.cbLogic.Items.Count > 0)
                {
                    this.cbLogic.SelectedItem = this.cbLogic.Items[0];
                }
            }
        }

        #endregion

        #region Public Properties

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            [DebuggerNonUserCode]
            get
            {
                return gbTeam.Text;
            }
            [DebuggerNonUserCode]
            set
            {
                gbTeam.Text = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Type LogicType
        {
            get
            {
                var item = this.cbLogic.SelectedItem as Type;
                return item;
            }
            set
            {
                this.cbLogic.SelectedItem = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int? PlayerCount
        {
            get
            {
                int result;
                return int.TryParse(tbPlayerCount.Text, out result) && result > 0 ? result : (int?)null;
            }
            set
            {
                tbPlayerCount.Text = value.HasValue ? value.ToString() : string.Empty;
            }
        }

        #endregion
    }
}