using System;
using System.Linq;

namespace SharpestBeak.UI
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pgGameSettings = new System.Windows.Forms.PropertyGrid();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.playToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.presetToolButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.presetDefaultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.presetSmallMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.presetMediumMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.presetLargeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extraLargeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgGameSettings
            // 
            this.pgGameSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgGameSettings.CommandsVisibleIfAvailable = false;
            this.pgGameSettings.HelpVisible = false;
            this.pgGameSettings.Location = new System.Drawing.Point(13, 28);
            this.pgGameSettings.Name = "pgGameSettings";
            this.pgGameSettings.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.pgGameSettings.Size = new System.Drawing.Size(379, 202);
            this.pgGameSettings.TabIndex = 3;
            this.pgGameSettings.ToolbarVisible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playToolButton,
            this.toolStripSeparator2,
            this.presetToolButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(404, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // playToolButton
            // 
            this.playToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.playToolButton.Image = ((System.Drawing.Image)(resources.GetObject("playToolButton.Image")));
            this.playToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.playToolButton.Name = "playToolButton";
            this.playToolButton.Size = new System.Drawing.Size(33, 22);
            this.playToolButton.Text = "Play";
            this.playToolButton.Click += new System.EventHandler(this.playToolButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // presetToolButton
            // 
            this.presetToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.presetToolButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.presetDefaultMenuItem,
            this.toolStripSeparator1,
            this.presetSmallMenuItem,
            this.presetMediumMenuItem,
            this.presetLargeMenuItem,
            this.extraLargeMenuItem});
            this.presetToolButton.Image = ((System.Drawing.Image)(resources.GetObject("presetToolButton.Image")));
            this.presetToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.presetToolButton.Name = "presetToolButton";
            this.presetToolButton.Size = new System.Drawing.Size(52, 22);
            this.presetToolButton.Text = "Preset";
            // 
            // presetDefaultMenuItem
            // 
            this.presetDefaultMenuItem.Name = "presetDefaultMenuItem";
            this.presetDefaultMenuItem.ShortcutKeyDisplayString = "";
            this.presetDefaultMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.presetDefaultMenuItem.Size = new System.Drawing.Size(171, 22);
            this.presetDefaultMenuItem.Text = "&Default";
            this.presetDefaultMenuItem.Click += new System.EventHandler(this.presetDefaultMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // presetSmallMenuItem
            // 
            this.presetSmallMenuItem.Name = "presetSmallMenuItem";
            this.presetSmallMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.presetSmallMenuItem.Size = new System.Drawing.Size(171, 22);
            this.presetSmallMenuItem.Text = "&Small";
            this.presetSmallMenuItem.Click += new System.EventHandler(this.presetSmallMenuItem_Click);
            // 
            // presetMediumMenuItem
            // 
            this.presetMediumMenuItem.Name = "presetMediumMenuItem";
            this.presetMediumMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.presetMediumMenuItem.Size = new System.Drawing.Size(171, 22);
            this.presetMediumMenuItem.Text = "&Medium";
            this.presetMediumMenuItem.Click += new System.EventHandler(this.presetMediumMenuItem_Click);
            // 
            // presetLargeMenuItem
            // 
            this.presetLargeMenuItem.Name = "presetLargeMenuItem";
            this.presetLargeMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
            this.presetLargeMenuItem.Size = new System.Drawing.Size(171, 22);
            this.presetLargeMenuItem.Text = "&Large";
            this.presetLargeMenuItem.Click += new System.EventHandler(this.presetLargeMenuItem_Click);
            // 
            // extraLargeMenuItem
            // 
            this.extraLargeMenuItem.Name = "extraLargeMenuItem";
            this.extraLargeMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D4)));
            this.extraLargeMenuItem.Size = new System.Drawing.Size(171, 22);
            this.extraLargeMenuItem.Text = "E&xtra Large";
            this.extraLargeMenuItem.Click += new System.EventHandler(this.extraLargeMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 242);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pgGameSettings);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(420, 280);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sharpest Beak: WinForms UI";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgGameSettings;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton presetToolButton;
        private System.Windows.Forms.ToolStripMenuItem presetSmallMenuItem;
        private System.Windows.Forms.ToolStripMenuItem presetDefaultMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem presetMediumMenuItem;
        private System.Windows.Forms.ToolStripMenuItem presetLargeMenuItem;
        private System.Windows.Forms.ToolStripButton playToolButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem extraLargeMenuItem;

    }
}