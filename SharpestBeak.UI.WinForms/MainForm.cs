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

        private static readonly int s_cellSize = 64;
        private static readonly int s_fullCellSize = s_cellSize + 1;

        private static readonly Brush s_evenCellBrush = new SolidBrush(SystemColors.Window);
        private static readonly Brush s_oddCellBrush = new SolidBrush(ControlPaint.Dark(SystemColors.Window, 0.05f));

        private GamePresentation m_lastPresentation;
        private ulong m_paintCount;

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

                var boardSize = this.GameEngine.CommonData.NominalSize;
                var boxSize = new Size(
                    boardSize.Width * s_cellSize + 1,
                    boardSize.Height * s_cellSize + 1);
                var difference = boxSize - pbGame.ClientSize;
                this.ClientSize = this.ClientSize + difference + new Size(0, statusBar.Height);
                this.CenterToScreen();

                ClearStatusLabels();
                UpdateMoveCountStatus();

                this.GameEngine.CallPaint();
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
            m_lastPresentation = e.Presentation;

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
                m_paintCount = 0;
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
                    this.GameEngine.CallPaint();
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
            if (m_lastPresentation == null)
            {
                return;
            }

            var size = m_lastPresentation.CommonData.NominalSize;
            var graphics = e.Graphics;

            // Drawing grid (for debug only... probably)
            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var cellPoint = new Point(s_cellSize * x, s_cellSize * y);
                    var cellRect = new Rectangle(cellPoint, new Size(s_fullCellSize, s_fullCellSize));
                    ControlPaint.DrawFocusRectangle(graphics, cellRect);

                    var backBrush = (x + y) % 2 == 0 ? s_evenCellBrush : s_oddCellBrush;
                    var backRect = cellRect;
                    backRect.Inflate(-1, -1);
                    graphics.FillRectangle(backBrush, backRect);
                }
            }

            var coefficient = (float)s_cellSize / GameConstants.NominalCellSize;
            var uiChickenBodyRadius = GameConstants.ChickenUnit.BodyCircleRadius * coefficient;
            var uiBeakOffset = GameConstants.ChickenUnit.BeakOffset * coefficient;
            var uiBeakRayOffset = GameConstants.ChickenUnit.BeakRayOffset * coefficient;

            foreach (var chickenUnit in m_lastPresentation.Chickens)
            {
                var uiPosition = chickenUnit.Position.Scale(coefficient);

                graphics.FillEllipse(
                    Brushes.Green,
                    uiPosition.X - uiChickenBodyRadius,
                    uiPosition.Y - uiChickenBodyRadius,
                    2f * uiChickenBodyRadius,
                    2f * uiChickenBodyRadius);

                var defaultBeakPolygonPoints = new[]
                {
                    new PointF(uiPosition.X, uiPosition.Y - uiBeakRayOffset),
                    new PointF(uiPosition.X + uiBeakOffset, uiPosition.Y),
                    new PointF(uiPosition.X, uiPosition.Y + uiBeakRayOffset)
                };
                var beakPolygonPoints = defaultBeakPolygonPoints.Rotate(uiPosition, chickenUnit.BeakAngle);

                graphics.FillPolygon(Brushes.Green, beakPolygonPoints, FillMode.Winding);

                var rcl = chickenUnit.Logic as RandomChickenLogic;
                if (rcl != null)
                {
                    var tp = rcl.TargetPoint.Scale(coefficient);
                    graphics.FillEllipse(Brushes.DarkBlue, tp.X - 5, tp.Y - 5, 10, 10);
                }
            }

            var uiShotRadius = GameConstants.ShotUnit.Radius * coefficient;
            foreach (var shotUnit in m_lastPresentation.Shots)
            {
                var uiPosition = shotUnit.Position.Scale(coefficient);
                graphics.FillEllipse(
                    Brushes.Red,
                    uiPosition.X - uiShotRadius,
                    uiPosition.Y - uiShotRadius,
                    2f * uiShotRadius,
                    2f * uiShotRadius);
            }

            m_paintCount++;
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