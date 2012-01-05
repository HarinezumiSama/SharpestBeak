using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        private bool InitializeGameEngine()
        {
            try
            {
                if (this.GameEngine != null)
                {
                    this.GameEngine.DiscreteMoveOccurred -= this.GameEngine_DiscreteMoveOccurred;
                }

                this.GameEngine = new GameEngine(
                    new Size(20, 20),
                    Enumerable.Range(1, 80).Select(item => typeof(RandomChickenLogic)));
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
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.ToString(),
                    "Game Initialization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void MakePrimitiveMove()
        {
            this.GameEngine.MakePrimitiveMove();
            this.pbGame.Invalidate();
        }

        private void MakeManualPrimitiveMove()
        {
            if (this.GameRunning)
            {
                StopGameRun();
            }
            else
            {
                MakePrimitiveMove();
            }
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

        private void StopGameRun()
        {
            this.GameRunning = false;
        }

        private void ToggleGameRun()
        {
            if (this.GameEngine.IsGameFinished)
            {
                StopGameRun();
                return;
            }

            this.GameRunning = !this.GameRunning;
        }

        private void RunTurnTest()
        {
            if (this.GameRunning)
            {
                StopGameRun();
            }
            using (var form = new TurnTestForm())
            {
                form.ShowDialog(this);
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!InitializeGameEngine())
            {
                Application.Exit();
                return;
            }

            ClearStatusLabel();
            UpdateTurnInfo();
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.KeyData)
            {
                case Keys.F1:
                    RunTurnTest();
                    break;

                case Keys.Space:
                    MakeManualPrimitiveMove();
                    break;

                case Keys.F5:
                    ToggleGameRun();
                    break;

                case Keys.F8:
                    StopGameRun();
                    if (!InitializeGameEngine())
                    {
                        Application.Exit();
                        return;
                    }
                    break;
            }
        }

        #endregion

        #region Public Properties

        public GameEngine GameEngine
        {
            get;
            private set;
        }

        public bool GameRunning
        {
            [DebuggerNonUserCode]
            get { return gameTimer.Enabled; }
            [DebuggerNonUserCode]
            set { gameTimer.Enabled = value; }
        }

        #endregion

        #region Event Handlers

        private void pbGame_Paint(object sender, PaintEventArgs e)
        {
            //var size = this.GameEngine.Board.Size;
            //for (int y = 0; y < size.Height; y++)
            //{
            //    for (int x = 0; x < size.Width; x++)
            //    {
            //        var cellPoint = new Point(s_cellSize.Width * x, s_cellSize.Height * y);
            //        var cellRect = new Rectangle(cellPoint, s_fullCellSize);
            //        ControlPaint.DrawFocusRectangle(e.Graphics, cellRect);

            //        Brush backBrush = (x + y) % 2 == 0 ? s_evenCellBrush : s_oddCellBrush;

            //        var chicken = this.GameEngine.Board.GetChickenAtPoint(new Point(x, y));
            //        if (chicken != null)
            //        {
            //            if (this.GameEngine.IsGameFinished)
            //            {
            //                backBrush = s_winBrush;
            //            }
            //            else if (chicken.Logic.CurrentMove != null
            //                && chicken.Logic.CurrentMove.MoveAction == MoveDirection.Peck)
            //            {
            //                backBrush = chicken.KilledBy != null ? s_peckedAttackBrush : s_attackBrush;
            //            }
            //            else if (chicken.KilledBy != null)
            //            {
            //                backBrush = s_peckedBrush;
            //            }
            //        }

            //        if (backBrush != null)
            //        {
            //            var backRect = cellRect;
            //            backRect.Inflate(-1, -1);
            //            e.Graphics.FillRectangle(backBrush, backRect);
            //        }

            //        if (chicken != null)
            //        {
            //            cellPoint.Offset(1, 1);

            //            // TODO: Paint chickens using primitive figures

            //            using (var image = (Bitmap)Resources.ResourceManager.GetObject(
            //                "Chicken" + chicken.BeakAngle.ToString()))
            //            {
            //                image.MakeTransparent(image.GetPixel(0, 0));  // Workaround

            //                var offsetX = (s_cellSize.Width - image.Width) / 2;
            //                var offsetY = (s_cellSize.Height - image.Height) / 2;
            //                cellPoint.Offset(offsetX, offsetY);

            //                e.Graphics.DrawImageUnscaled(image, cellPoint);
            //            }
            //        }
            //    }
            //}
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                StopGameRun();
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
            //var toolTip = string.Empty;

            //if (!this.GameRunning)
            //{
            //    var mousePosition = e.Location;
            //    var size = this.GameEngine.Board.Size;
            //    var cellAddress = new Point(mousePosition.X / s_cellSize.Width, mousePosition.Y / s_cellSize.Height);

            //    var chicken = this.GameEngine.Board.GetChickenAtPoint(cellAddress);
            //    if (chicken != null)
            //    {
            //        toolTip = string.Format(
            //            "[{0} : {1}] Kills: {2}",
            //            chicken.UniqueIndex,
            //            chicken.GetType().Name,
            //            chicken.KillCount);
            //    }
            //}

            //statusLabel.Text = toolTip;
        }

        private void pbGame_MouseLeave(object sender, EventArgs e)
        {
            ClearStatusLabel();
        }

        private void pbGame_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.GameEngine.IsGameFinished)
            {
                StopGameRun();
                return;
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    ToggleGameRun();
                    break;

                case MouseButtons.Right:
                    MakeManualPrimitiveMove();
                    break;

                case MouseButtons.Middle:
                    RunTurnTest();
                    break;
            }
        }

        #endregion
    }
}