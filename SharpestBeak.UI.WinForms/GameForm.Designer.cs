using SharpestBeak.Common;
namespace SharpestBeak.UI.WinForms
{
    partial class GameForm
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
            if (disposing)
            {
                Helper.DisposeAndNull(ref components);
                Helper.DisposeAndNull(ref m_gameBoardBackground);

                m_lightTeamUnitDrawData.DisposeSafely();
                m_lightTeamShotDrawData.DisposeSafely();
                m_darkTeamUnitDrawData.DisposeSafely();
                m_darkTeamShotDrawData.DisposeSafely();
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
            this.components = new System.ComponentModel.Container();
            this.pbGame = new System.Windows.Forms.PictureBox();
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.turnInfoLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fpsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.fpsTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbGame)).BeginInit();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbGame
            // 
            this.pbGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbGame.Location = new System.Drawing.Point(0, 0);
            this.pbGame.Name = "pbGame";
            this.pbGame.Size = new System.Drawing.Size(284, 262);
            this.pbGame.TabIndex = 0;
            this.pbGame.TabStop = false;
            this.pbGame.Paint += new System.Windows.Forms.PaintEventHandler(this.pbGame_Paint);
            this.pbGame.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbGame_MouseClick);
            // 
            // mainToolTip
            // 
            this.mainToolTip.AutomaticDelay = 250;
            this.mainToolTip.AutoPopDelay = 30000;
            this.mainToolTip.InitialDelay = 250;
            this.mainToolTip.ReshowDelay = 50;
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.turnInfoLabel,
            this.fpsLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 238);
            this.statusBar.Name = "statusBar";
            this.statusBar.ShowItemToolTips = true;
            this.statusBar.Size = new System.Drawing.Size(284, 24);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 2;
            // 
            // turnInfoLabel
            // 
            this.turnInfoLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.turnInfoLabel.Name = "turnInfoLabel";
            this.turnInfoLabel.Size = new System.Drawing.Size(74, 19);
            this.turnInfoLabel.Text = "(TurnInfo...)";
            // 
            // fpsLabel
            // 
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(56, 19);
            this.fpsLabel.Text = "(Status...)";
            // 
            // fpsTimer
            // 
            this.fpsTimer.Tick += new System.EventHandler(this.fpsTimer_Tick);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.pbGame);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game – Sharpest Beak";
            ((System.ComponentModel.ISupportInitialize)(this.pbGame)).EndInit();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbGame;
        private System.Windows.Forms.ToolTip mainToolTip;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel fpsLabel;
        private System.Windows.Forms.ToolStripStatusLabel turnInfoLabel;
        private System.Windows.Forms.Timer fpsTimer;
    }
}