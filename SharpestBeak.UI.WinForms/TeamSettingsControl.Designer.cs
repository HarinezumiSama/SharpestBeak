namespace SharpestBeak.UI.WinForms
{
    partial class TeamSettingsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbTeam = new System.Windows.Forms.GroupBox();
            this.tbPlayerCount = new System.Windows.Forms.TextBox();
            this.countLabel = new System.Windows.Forms.Label();
            this.lblLightLogic = new System.Windows.Forms.Label();
            this.cbLogic = new System.Windows.Forms.ComboBox();
            this.gbTeam.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTeam
            // 
            this.gbTeam.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTeam.Controls.Add(this.tbPlayerCount);
            this.gbTeam.Controls.Add(this.countLabel);
            this.gbTeam.Controls.Add(this.lblLightLogic);
            this.gbTeam.Controls.Add(this.cbLogic);
            this.gbTeam.Location = new System.Drawing.Point(0, 0);
            this.gbTeam.Name = "gbTeam";
            this.gbTeam.Size = new System.Drawing.Size(275, 77);
            this.gbTeam.TabIndex = 0;
            this.gbTeam.TabStop = false;
            this.gbTeam.Text = "Team...";
            // 
            // tbPlayerCount
            // 
            this.tbPlayerCount.Location = new System.Drawing.Point(107, 47);
            this.tbPlayerCount.Name = "tbPlayerCount";
            this.tbPlayerCount.Size = new System.Drawing.Size(126, 20);
            this.tbPlayerCount.TabIndex = 3;
            // 
            // countLabel
            // 
            this.countLabel.AutoSize = true;
            this.countLabel.Location = new System.Drawing.Point(6, 50);
            this.countLabel.Name = "countLabel";
            this.countLabel.Size = new System.Drawing.Size(69, 13);
            this.countLabel.TabIndex = 2;
            this.countLabel.Text = "Player count:";
            // 
            // lblLightLogic
            // 
            this.lblLightLogic.AutoSize = true;
            this.lblLightLogic.Location = new System.Drawing.Point(6, 23);
            this.lblLightLogic.Name = "lblLightLogic";
            this.lblLightLogic.Size = new System.Drawing.Size(36, 13);
            this.lblLightLogic.TabIndex = 0;
            this.lblLightLogic.Text = "Logic:";
            // 
            // cbLogic
            // 
            this.cbLogic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLogic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLogic.FormattingEnabled = true;
            this.cbLogic.Location = new System.Drawing.Point(107, 20);
            this.cbLogic.Name = "cbLogic";
            this.cbLogic.Size = new System.Drawing.Size(162, 21);
            this.cbLogic.Sorted = true;
            this.cbLogic.TabIndex = 1;
            // 
            // TeamSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbTeam);
            this.MaximumSize = new System.Drawing.Size(999999, 77);
            this.MinimumSize = new System.Drawing.Size(275, 77);
            this.Name = "TeamSettingsControl";
            this.Size = new System.Drawing.Size(275, 77);
            this.gbTeam.ResumeLayout(false);
            this.gbTeam.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbTeam;
        private System.Windows.Forms.Label lblLightLogic;
        private System.Windows.Forms.ComboBox cbLogic;
        private System.Windows.Forms.TextBox tbPlayerCount;
        private System.Windows.Forms.Label countLabel;
    }
}
