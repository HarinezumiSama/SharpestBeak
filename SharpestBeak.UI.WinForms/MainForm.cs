using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpestBeak.Common;
using SharpestBeak.UI.WinForms.Properties;

namespace SharpestBeak.UI.WinForms
{
    public partial class MainForm : Form
    {
        #region Fields

        private static readonly Size s_cellSize = new Size(16, 16);
        private static readonly Size s_fullCellSize = Size.Add(s_cellSize, new Size(1, 1));

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.GameEngine = new GameEngine(new Size(20, 20), typeof(RandomChicken), typeof(RandomChicken));
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var size = this.GameEngine.Board.Size;
            this.ClientSize = new Size(
                size.Width * s_cellSize.Width + size.Width + 1,
                size.Height * s_cellSize.Height + size.Height + 1);
        }

        #endregion

        #region Public Properties

        public GameEngine GameEngine
        {
            get;
            private set;
        }

        #endregion

        #region Event Handlers

        private void pbGame_Paint(object sender, PaintEventArgs e)
        {
            var size = this.GameEngine.Board.Size;
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var chicken = this.GameEngine.Board.GetChickenAtPoint(new Point(x, y));

                    var cellPoint = new Point(s_cellSize.Width * x, s_cellSize.Height * y);
                    var cellRect = new Rectangle(cellPoint, s_fullCellSize);

                    var borderColor = Color.Black;
                    if (chicken != null)
                    {
                        if (this.GameEngine.IsGameFinished)
                        {
                            borderColor = Color.Green;
                        }
                        else if (chicken.CurrentMove != null && chicken.CurrentMove.Move == MoveAction.Peck)
                        {
                            borderColor = Color.Red;
                        }
                    }
                    ControlPaint.DrawFocusRectangle(e.Graphics, cellRect, borderColor, Color.White);

                    cellPoint.X++;
                    cellPoint.Y++;

                    if (chicken != null)
                    {
                        using (var image = (Bitmap)Resources.ResourceManager.GetObject(
                            "Chicken" + chicken.BeakAngle.ToString()))
                        {
                            e.Graphics.DrawImageUnscaled(image, cellPoint);
                        }
                    }
                }
            }
        }

        private void pbGame_Click(object sender, EventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                gameTimer.Enabled = false;
                return;
            }

            gameTimer.Enabled = !gameTimer.Enabled;
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                return;
            }

            this.GameEngine.MakePrimitiveMove();
            this.pbGame.Invalidate();
        }

        #endregion
    }
}