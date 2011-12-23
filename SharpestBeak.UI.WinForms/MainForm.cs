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

        private static readonly Size s_cellSize = new Size(24, 24);
        private static readonly Size s_fullCellSize = Size.Add(s_cellSize, new Size(1, 1));

        private static readonly Brush s_winBrush = new SolidBrush(Color.Green);
        private static readonly Brush s_attackBrush = new SolidBrush(Color.Red);

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.GameEngine = new GameEngine(
                new Size(10, 10),
                Enumerable.Range(1, 20).Select(item => typeof(RandomChicken)));
            this.GameEngine.DiscreteMoveOccurred += this.GameEngine_DiscreteMoveOccurred;
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var size = this.GameEngine.Board.Size;
            this.ClientSize = new Size(
                size.Width * s_cellSize.Width + 1,
                size.Height * s_cellSize.Height + 1);
            this.CenterToScreen();
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
                    var cellPoint = new Point(s_cellSize.Width * x, s_cellSize.Height * y);
                    var cellRect = new Rectangle(cellPoint, s_fullCellSize);
                    ControlPaint.DrawFocusRectangle(e.Graphics, cellRect);

                    var chicken = this.GameEngine.Board.GetChickenAtPoint(new Point(x, y));
                    if (chicken != null)
                    {
                        Brush backBrush = null;
                        if (this.GameEngine.IsGameFinished)
                        {
                            backBrush = s_winBrush;
                        }
                        else if (chicken.CurrentMove != null && chicken.CurrentMove.Move == MoveAction.Peck)
                        {
                            backBrush = s_attackBrush;
                        }

                        if (backBrush != null)
                        {
                            var backRect = cellRect;
                            backRect.Inflate(-1, -1);
                            e.Graphics.FillRectangle(backBrush, backRect);
                        }
                    }

                    if (chicken != null)
                    {
                        cellPoint.Offset(1, 1);
                        using (var image = (Bitmap)Resources.ResourceManager.GetObject(
                            "Chicken" + chicken.BeakAngle.ToString()))
                        {
                            var offsetX = (s_cellSize.Width - image.Width) / 2;
                            var offsetY = (s_cellSize.Height - image.Height) / 2;
                            cellPoint.Offset(offsetX, offsetY);

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

        private void GameEngine_DiscreteMoveOccurred(object sender, EventArgs e)
        {
            this.pbGame.Invalidate();
        }

        #endregion
    }
}