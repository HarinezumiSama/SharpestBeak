﻿using System;
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

        private static readonly int s_cellSize = 64;
        private static readonly int s_fullCellSize = s_cellSize + 1;

        private static readonly Brush s_evenCellBrush = new SolidBrush(SystemColors.Window);
        private static readonly Brush s_oddCellBrush = new SolidBrush(ControlPaint.Dark(SystemColors.Window, 0.05f));

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
                this.GameEngine = new GameEngine(
                    this.PaintGame,
                    new Size(5, 5),
                    Enumerable.Range(1, 2).Select(item => typeof(RandomChickenLogic)));

                var boardSize = this.GameEngine.NominalSize;
                var boxSize = new Size(
                    boardSize.Width * s_cellSize + 1,
                    boardSize.Height * s_cellSize + 1);
                var difference = boxSize - pbGame.ClientSize;
                this.ClientSize = this.ClientSize + difference + new Size(0, statusBar.Height);
                this.CenterToScreen();

                ClearStatusLabels();
                UpdateMoveCountStatus();

                PaintGame(null);  // TODO
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

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

        private void PaintGame(GamePaintEventArgs e)
        {
            if (this.GameEngine.MoveCount % 25 == 0)
            {
                UpdateMoveCountStatus();
            }
            pbGame.Invalidate();
        }

        private void UpdateMoveCountStatus()
        {
            turnInfoLabel.Text = string.Format("Move count: {0}", this.GameEngine.MoveCount);
            turnInfoLabel.Invalidate();
        }

        private void ClearStatusLabels()
        {
            statusLabel.Text = string.Empty;
            turnInfoLabel.Text = string.Empty;
        }

        private void RunTurnTest()
        {
            using (var form = new TurnTestForm())
            {
                form.ShowDialog(this);
            }
        }

        private void StartGame()
        {
            if (!this.GameEngine.IsRunning)
            {
                this.GameEngine.Start();
                UpdateMoveCountStatus();
            }
        }

        private void StopGame()
        {
            if (this.GameEngine.IsRunning)
            {
                this.GameEngine.Stop();
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ClearStatusLabels();
            if (!InitializeGameEngine())
            {
                Application.Exit();
                return;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopGame();
            base.OnFormClosing(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.KeyData)
            {
                case Keys.F1:
                    StopGame();
                    RunTurnTest();
                    break;

                case Keys.F5:
                    if (this.GameEngine.IsRunning)
                    {
                        StopGame();
                    }
                    else
                    {
                        StartGame();
                    }
                    break;

                case Keys.F8:
                    StopGame();
                    this.GameEngine.Reset();
                    PaintGame(null); // TODO
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

        #endregion

        #region Event Handlers

        private void pbGame_Paint(object sender, PaintEventArgs e)
        {
            var size = this.GameEngine.NominalSize;
            var graphics = e.Graphics;

            // Drawing grid (for debug only... probably)
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var cellPoint = new Point(s_cellSize * x, s_cellSize * y);
                    var cellRect = new Rectangle(cellPoint, new Size(s_fullCellSize, s_fullCellSize));
                    ControlPaint.DrawFocusRectangle(graphics, cellRect);

                    Brush backBrush = (x + y) % 2 == 0 ? s_evenCellBrush : s_oddCellBrush;

                    if (backBrush != null)
                    {
                        var backRect = cellRect;
                        backRect.Inflate(-1, -1);
                        graphics.FillRectangle(backBrush, backRect);
                    }
                }
            }

            foreach (var unit in this.GameEngine.AliveChickens)
            {
                var coefficient = (float)s_cellSize / Constants.LargeCellSize;
                var uiPosition = unit.Position.Scale(coefficient);
                var uiRadius = Constants.ChickenUnit.BodyCircleRadius * coefficient;

                graphics.FillEllipse(
                    Brushes.Green,
                    uiPosition.X - uiRadius,
                    uiPosition.Y - uiRadius,
                    2 * uiRadius,
                    2 * uiRadius);

                var uiBeakOffset = Constants.ChickenUnit.BeakOffset * coefficient;
                var uiBeakRayOffset = Constants.ChickenUnit.BeakRayOffset * coefficient;
                var defaultBeakPolygonPoints = new[]
                {
                    new PointF(uiPosition.X, uiPosition.Y - uiBeakRayOffset),
                    new PointF(uiPosition.X + uiBeakOffset, uiPosition.Y),
                    new PointF(uiPosition.X, uiPosition.Y + uiBeakRayOffset)
                };
                //var defaultBeakPoint = uiPosition + new SizeF(uiBeakOffset, uiBeakOffset);
                //var beakPoint = defaultBeakPoint.RotatePoint(uiPosition, unit.BeakAngle);
                var beakPolygonPoints = defaultBeakPolygonPoints.RotatePoints(uiPosition, unit.BeakAngle);

                graphics.FillPolygon(Brushes.Green, beakPolygonPoints, FillMode.Winding);
            }
        }

        private void pbGame_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (this.GameEngine.IsRunning)
                    {
                        StopGame();
                    }
                    else
                    {
                        StartGame();
                    }
                    break;
            }
        }

        #endregion
    }
}