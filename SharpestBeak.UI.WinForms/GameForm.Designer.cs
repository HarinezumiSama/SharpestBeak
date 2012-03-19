using SharpestBeak;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.pbGame = new System.Windows.Forms.PictureBox();
            this.mainToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.turnInfoLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.lightTeamLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.darkTeamLabel = new System.Windows.Forms.ToolStripStatusLabel();
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
            this.pbGame.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.pbGame.Name = "pbGame";
            this.pbGame.Size = new System.Drawing.Size(295, 214);
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
            this.lightTeamLabel,
            this.darkTeamLabel,
            this.fpsLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 190);
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusBar.ShowItemToolTips = true;
            this.statusBar.Size = new System.Drawing.Size(295, 24);
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
            // lightTeamLabel
            // 
            this.lightTeamLabel.Name = "lightTeamLabel";
            this.lightTeamLabel.Size = new System.Drawing.Size(54, 19);
            this.lightTeamLabel.Text = "Light: XX";
            // 
            // darkTeamLabel
            // 
            this.darkTeamLabel.Name = "darkTeamLabel";
            this.darkTeamLabel.Size = new System.Drawing.Size(51, 19);
            this.darkTeamLabel.Text = "Dark: XX";
            // 
            // fpsLabel
            // 
            this.fpsLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(60, 19);
            this.fpsLabel.Text = "(Status...)";
            // 
            // fpsTimer
            // 
            this.fpsTimer.Tick += new System.EventHandler(this.fpsTimer_Tick);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 214);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.pbGame);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
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
        private System.Windows.Forms.ToolStripStatusLabel lightTeamLabel;
        private System.Windows.Forms.ToolStripStatusLabel darkTeamLabel;
    }
}