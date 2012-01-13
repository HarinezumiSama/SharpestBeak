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
using SharpestBeak.Common.Elements;
using SharpestBeak.Common.Elements.Primitives;
using SharpestBeak.UI.WinForms.Properties;

namespace SharpestBeak.UI.WinForms
{
    public partial class MainForm : Form
    {
        #region Fields

        private static readonly Size s_gameBoardSize = new Size(5, 5);
        private static readonly int s_chickenUnitCount = 2;

        private static readonly int s_cellSize = 48;
        private static readonly int s_fullCellSize = s_cellSize + 1;

        private static readonly Brush s_evenCellBrush = new SolidBrush(SystemColors.Window);
        private static readonly Brush s_oddCellBrush = new SolidBrush(ControlPaint.Dark(SystemColors.Window, 0.05f));

        private readonly GameEngine m_gameEngine;
        private GamePresentation m_lastPresentation;
        private ulong m_totalPaintCount;

        private float m_fps;
        private int m_fpsPaintCount;
        private readonly Stopwatch m_fpsStopwatch = new Stopwatch();

        #endregion

        #region Constructors

        public MainForm()
        {
            #region Manual Test 1

            //var v1 = new Vector2D(1f, 2f);
            //var v2 = new Vector2D(3f, -2f);
            //var pv = v1.Project(v2);
            //var p = v1.ProjectScalar(v2);
            //var pr = v2.Normalize() * p;

            #endregion

            #region Manual Test 2

            //var pg1 = new ConvexPolygonPrimitive(new Point2D(2, 1), new Point2D(4, 2), new Point2D(1, 5));
            //var pg2 = new ConvexPolygonPrimitive(new Point2D(4, 4), new Point2D(5, 6), new Point2D(3, 4));
            //var pg3 = new ConvexPolygonPrimitive(new Point2D(4, 4), new Point2D(5, 6), new Point2D(0, 0));
            //var c1 = CollisionDetector.CheckCollision(pg1, pg2);
            //Debug.WriteLine(c1);
            //var c2 = CollisionDetector.CheckCollision(pg1, pg3);
            //Debug.WriteLine(c1);

            #endregion

            InitializeComponent();

            m_gameEngine = new GameEngine(
                this.PaintGame,
                s_gameBoardSize,
                new ChickenLogicRecord(typeof(RandomChickenLogic), s_chickenUnitCount));
        }

        #endregion

        #region Private Methods

        private bool InitializeGameUI()
        {
            try
            {
                var boardSize = m_gameEngine.CommonData.NominalSize;
                var boxSize = new Size(
                    boardSize.Width * s_cellSize + 1,
                    boardSize.Height * s_cellSize + 1);
                var difference = boxSize - pbGame.ClientSize;
                this.ClientSize = this.ClientSize + difference + new Size(0, statusBar.Height);
                this.CenterToScreen();

                ClearStatusLabels();
                UpdateMoveCountStatus();

                m_gameEngine.CallPaint();
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

            if (m_gameEngine.MoveCount % 25 == 0)
            {
                UpdateMoveCountStatus();
            }
            pbGame.Invalidate();
        }

        private void UpdateMoveCountStatus()
        {
            //turnInfoLabel.Text = string.Format(
            //    "Move count: {0}, collision checks: {1}",
            //    m_gameEngine.MoveCount,
            //    CollisionDetector.CallCount);
            turnInfoLabel.Text = string.Format("Move count: {0}", m_gameEngine.MoveCount);

            turnInfoLabel.Invalidate();
        }

        private void ClearStatusLabels()
        {
            fpsLabel.Text = string.Empty;
            turnInfoLabel.Text = string.Empty;
        }

        private void RunTurnTest()
        {
            using (var form = new TurnTestForm())
            {
                form.ShowDialog(this);
            }
        }

        private void ResetFpsCounter(bool fullReset)
        {
            if (fullReset)
            {
                m_fps = -1f;
            }
            m_fpsPaintCount = 0;
            m_fpsStopwatch.Restart();
        }

        private void StartGame()
        {
            if (m_gameEngine.IsRunning)
            {
                return;
            }

            m_totalPaintCount = 0;
            ResetFpsCounter(true);
            m_gameEngine.Start();
            UpdateMoveCountStatus();
            fpsTimer.Enabled = true;
        }

        private void StopGame()
        {
            if (!m_gameEngine.IsRunning)
            {
                return;
            }

            fpsTimer.Enabled = false;
            m_gameEngine.Stop();
            m_fpsStopwatch.Stop();
            fpsLabel.Text = string.Empty;
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ClearStatusLabels();
            if (!InitializeGameUI())
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
                    if (m_gameEngine.IsRunning)
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
                    m_gameEngine.Reset();
                    m_gameEngine.CallPaint();
                    break;
            }
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
                var uiPosition = chickenUnit.Position * coefficient;

                graphics.FillEllipse(
                    Brushes.Green,
                    uiPosition.X - uiChickenBodyRadius,
                    uiPosition.Y - uiChickenBodyRadius,
                    2f * uiChickenBodyRadius,
                    2f * uiChickenBodyRadius);

                var defaultBeakPolygonPoints = new[]
                {
                    new Point2D(uiPosition.X, uiPosition.Y - uiBeakRayOffset),
                    new Point2D(uiPosition.X + uiBeakOffset, uiPosition.Y),
                    new Point2D(uiPosition.X, uiPosition.Y + uiBeakRayOffset)
                };
                var beakPolygonPoints = defaultBeakPolygonPoints.Rotate(uiPosition, chickenUnit.BeakAngle);

                graphics.FillPolygon(Brushes.Green, beakPolygonPoints.ToPointF(), FillMode.Winding);

                var rcl = chickenUnit.Logic as RandomChickenLogic;
                if (rcl != null)
                {
                    foreach (var targetPoint in rcl.TargetPoints)
                    {
                        var tp = targetPoint * coefficient;
                        graphics.FillEllipse(Brushes.DarkBlue, tp.X - 5, tp.Y - 5, 10, 10);
                    }
                }
            }

            var uiShotRadius = GameConstants.ShotUnit.Radius * coefficient;
            foreach (var shotUnit in m_lastPresentation.Shots)
            {
                var uiPosition = shotUnit.Position * coefficient;
                graphics.FillEllipse(
                    Brushes.Red,
                    uiPosition.X - uiShotRadius,
                    uiPosition.Y - uiShotRadius,
                    2f * uiShotRadius,
                    2f * uiShotRadius);
            }

            m_totalPaintCount++;
            m_fpsPaintCount++;

            var fpsElapsedSeconds = m_fpsStopwatch.Elapsed.TotalSeconds;
            if (fpsElapsedSeconds >= 1d)
            {
                m_fps = (float)(m_fpsPaintCount / fpsElapsedSeconds);
                ResetFpsCounter(false);
            }
        }

        private void pbGame_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (m_gameEngine.IsRunning)
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

        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            if (m_fps >= 0f)
            {
                fpsLabel.Text = string.Format("{0:N1} FPS", m_fps);
            }
        }

        #endregion
    }
}