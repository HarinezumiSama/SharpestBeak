﻿using System;
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
        private static readonly Size s_fullCellSize = s_cellSize + new Size(1, 1);

        private static readonly Brush s_evenCellBrush = new SolidBrush(SystemColors.Window);
        private static readonly Brush s_oddCellBrush = new SolidBrush(ControlPaint.Dark(SystemColors.Window, 0.05f));
        private static readonly Brush s_winBrush = new SolidBrush(Color.Green);
        private static readonly Brush s_attackBrush = new SolidBrush(Color.Red);
        private static readonly Brush s_peckedBrush = new SolidBrush(Color.Yellow);
        private static readonly Brush s_peckedAttackBrush = new SolidBrush(Color.Orange);

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private void InitializeGameEngine()
        {
            if (this.GameEngine != null)
            {
                this.GameEngine.DiscreteMoveOccurred -= this.GameEngine_DiscreteMoveOccurred;
            }

            this.GameEngine = new GameEngine(
                new Size(20, 20),
                Enumerable.Range(1, 80).Select(item => typeof(RandomChicken)));
            this.GameEngine.DiscreteMoveOccurred += this.GameEngine_DiscreteMoveOccurred;

            var boardSize = this.GameEngine.Board.Size;
            var boxSize = new Size(
                boardSize.Width * s_cellSize.Width + 1,
                boardSize.Height * s_cellSize.Height + 1);
            var difference = boxSize - pbGame.ClientSize;
            this.ClientSize = this.ClientSize + difference + new Size(0, statusBar.Height);
            this.CenterToScreen();

            ClearStatusLabel();
            UpdateTurnInfo();

            this.pbGame.Invalidate();
        }

        private void MakePrimitiveMove()
        {
            this.GameEngine.MakePrimitiveMove();
            this.pbGame.Invalidate();
        }

        private void UpdateTurnInfo()
        {
            var text = string.Empty;
            if (this.GameEngine != null)
            {
                var chicken = this.GameEngine.Board.AliveChickens[this.GameEngine.PlayerIndex];
                var chickenInfoText = chicken == null
                    ? "?"
                    : string.Format(
                        "{0}/{1} [#{2}] at {3}",
                        this.GameEngine.PlayerIndex + 1,
                        this.GameEngine.Board.AliveChickens.Count,
                        chicken.UniqueIndex,
                        chicken.Position);
                text = string.Format(
                    "Turn: {0}. Chicken: {1}",
                    this.GameEngine.TurnIndex,
                    chickenInfoText);
            }
            turnInfoLabel.Text = text;
        }

        private void ClearStatusLabel()
        {
            statusLabel.Text = string.Empty;
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitializeGameEngine();

            ClearStatusLabel();
            UpdateTurnInfo();
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

                    Brush backBrush = (x + y) % 2 == 0 ? s_evenCellBrush : s_oddCellBrush;

                    var chicken = this.GameEngine.Board.GetChickenAtPoint(new Point(x, y));
                    if (chicken != null)
                    {
                        if (this.GameEngine.IsGameFinished)
                        {
                            backBrush = s_winBrush;
                        }
                        else if (chicken.CurrentMove != null && chicken.CurrentMove.MoveAction == MoveAction.Peck)
                        {
                            backBrush = chicken.PeckedBy != null ? s_peckedAttackBrush : s_attackBrush;
                        }
                        else if (chicken.PeckedBy != null)
                        {
                            backBrush = s_peckedBrush;
                        }
                    }

                    if (backBrush != null)
                    {
                        var backRect = cellRect;
                        backRect.Inflate(-1, -1);
                        e.Graphics.FillRectangle(backBrush, backRect);
                    }

                    if (chicken != null)
                    {
                        cellPoint.Offset(1, 1);

                        // TODO: Paint chickens using primitive figures

                        using (var image = (Bitmap)Resources.ResourceManager.GetObject(
                            "Chicken" + chicken.BeakAngle.ToString()))
                        {
                            image.MakeTransparent(image.GetPixel(0, 0));  // Workaround

                            var offsetX = (s_cellSize.Width - image.Width) / 2;
                            var offsetY = (s_cellSize.Height - image.Height) / 2;
                            cellPoint.Offset(offsetX, offsetY);

                            e.Graphics.DrawImageUnscaled(image, cellPoint);
                        }
                    }
                }
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                gameTimer.Enabled = false;
                return;
            }

            MakePrimitiveMove();
        }

        private void GameEngine_DiscreteMoveOccurred(object sender, EventArgs e)
        {
            UpdateTurnInfo();
            this.pbGame.Refresh();
        }

        private void pbGame_MouseMove(object sender, MouseEventArgs e)
        {
            var toolTip = string.Empty;

            if (!gameTimer.Enabled)
            {
                var mousePosition = e.Location;
                var size = this.GameEngine.Board.Size;
                var cellAddress = new Point(mousePosition.X / s_cellSize.Width, mousePosition.Y / s_cellSize.Height);

                var chicken = this.GameEngine.Board.GetChickenAtPoint(cellAddress);
                if (chicken != null)
                {
                    toolTip = string.Format(
                        "[{0}:{1}] Kills: {2}",
                        chicken.UniqueIndex,
                        chicken.GetType().Name,
                        chicken.KillCount);
                }
            }

            statusLabel.Text = toolTip;
        }

        private void pbGame_MouseLeave(object sender, EventArgs e)
        {
            ClearStatusLabel();
        }

        private void pbGame_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                gameTimer.Enabled = false;
                return;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    gameTimer.Enabled = !gameTimer.Enabled;
                    return;

                case MouseButtons.Right:
                    if (gameTimer.Enabled)
                    {
                        gameTimer.Enabled = false;
                    }
                    else
                    {
                        MakePrimitiveMove();
                    }
                    return;
            }
        }

        private void MainForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.F8)
            {
                gameTimer.Enabled = false;
                InitializeGameEngine();
            }
        }

        #endregion
    }
}