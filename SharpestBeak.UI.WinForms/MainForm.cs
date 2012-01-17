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
            #region Manual Test (Projection)

            {
                //var v1 = new Vector2D(1f, 2f);
                //var v2 = new Vector2D(3f, -2f);
                //var pv = v1.Project(v2);
                //var p = v1.ProjectScalar(v2);
                //var pr = v2.Normalize() * p;
            }

            #endregion

            #region Manual Test (Collision: polygon/polygon)

            {
                //var pg1 = new ConvexPolygonPrimitive(new Point2D(2f, 1f), new Point2D(4f, 2f), new Point2D(1f, 5f));
                //var pg2 = new ConvexPolygonPrimitive(new Point2D(4f, 4f), new Point2D(5f, 6f), new Point2D(3f, 4f));
                //var pg3 = new ConvexPolygonPrimitive(new Point2D(4f, 4f), new Point2D(5f, 6f), new Point2D(0f, 0f));

                //var c1to2 = CollisionDetector.CheckCollision(pg1, pg2);
                //Debug.WriteLine(c1to2);
                //Debug.Assert(!c1to2);

                //var c1to3 = CollisionDetector.CheckCollision(pg1, pg3);
                //Debug.WriteLine(c1to3);
                //Debug.Assert(c1to3);

                //// One polygon is inside another
                //var pg4 = new ConvexPolygonPrimitive(
                //    new Point2D(0f, 0f),
                //    new Point2D(0f, 10f),
                //    new Point2D(10f, 10f),
                //    new Point2D(10f, 0f));
                //var pg5 = new ConvexPolygonPrimitive(
                //    new Point2D(1f, 1f),
                //    new Point2D(1f, 9f),
                //    new Point2D(9f, 9f),
                //    new Point2D(9f, 1f));
                //var c4to5 = CollisionDetector.CheckCollision(pg4, pg5);
                //Debug.WriteLine(c4to5);
                //Debug.Assert(c4to5);
            }

            #endregion

            #region Manual Test (Collision: line/circle)

            {
                //var circle = new CirclePrimitive(new Point2D(1f, 2f), 0.5f);

                //var line1 = new LinePrimitive(new Point2D(1.1f, 2.1f), new Point2D(1.2f, 1.9f));
                //var c1 = CollisionDetector.CheckCollision(circle, line1);
                //Debug.WriteLine(c1);
                //Debug.Assert(c1);

                //var line2 = new LinePrimitive(new Point2D(0f, 3f), new Point2D(2f, 0f));
                //var c2 = CollisionDetector.CheckCollision(circle, line2);
                //Debug.WriteLine(c2);
                //Debug.Assert(c2);

                //var line3 = new LinePrimitive(new Point2D(1f, 1.5f), new Point2D(5f, 1.5f));
                //var c3 = CollisionDetector.CheckCollision(circle, line3);
                //Debug.WriteLine(c3);
                //Debug.Assert(c3);

                //var line4 = new LinePrimitive(new Point2D(0f, 1f), new Point2D(1f, 0f));
                //var c4 = CollisionDetector.CheckCollision(circle, line4);
                //Debug.WriteLine(c4);
                //Debug.Assert(!c4);
            }

            #endregion

            #region Manual Test (Collision: point/line)

            {
                //var line = new LinePrimitive(new Point2D(1f, 1f), new Point2D(2f, 2f));

                //var s1 = CollisionDetector.GetLineSide(line, new Point2D(1.5f, 1.5f));
                //Debug.WriteLine(s1);
                //Debug.Assert(s1 == LineSide.OnLine);

                //var s2 = CollisionDetector.GetLineSide(line, new Point2D(0f, 1f));
                //Debug.WriteLine(s2);
                //Debug.Assert(s2 == LineSide.Left);

                //var s3 = CollisionDetector.GetLineSide(line, new Point2D(1f, 0f));
                //Debug.WriteLine(s3);
                //Debug.Assert(s3 == LineSide.Right);
            }

            #endregion

            #region Manual Test (Collision: point/polygon)

            {
                //var polygon = new ConvexPolygonPrimitive(
                //    new Point2D(0f, 0f),
                //    new Point2D(1f, 0f),
                //    new Point2D(0f, 1f));

                //var p1 = new Point2D(0.5f, 0.5f);
                //var in1 = CollisionDetector.IsPointInPolygon(p1, polygon);
                //Debug.WriteLine(in1);
                //Debug.Assert(in1);

                //var p2 = new Point2D(0f, 1.1f);
                //var in2 = CollisionDetector.IsPointInPolygon(p2, polygon);
                //Debug.WriteLine(in2);
                //Debug.Assert(!in2);
            }

            #endregion

            #region Manual Test (Collision: line/polygon)

            {
                //var polygon = new ConvexPolygonPrimitive(
                //    new Point2D(0f, 0f),
                //    new Point2D(1f, 0f),
                //    new Point2D(0f, 1f));

                //// One of the line's ends is on a polygon edge
                //var line1 = new LinePrimitive(new Point2D(0.5f, 0.5f), new Point2D(1f, 1f));
                //var c1 = CollisionDetector.CheckCollision(line1, polygon);
                //Debug.WriteLine(c1);
                //Debug.Assert(c1);

                //// One of the line's ends is inside polygon
                //var line2 = new LinePrimitive(new Point2D(0.1f, 0.1f), new Point2D(5f, 15f));
                //var c2 = CollisionDetector.CheckCollision(line2, polygon);
                //Debug.WriteLine(c2);
                //Debug.Assert(c2);

                //var line3 = new LinePrimitive(new Point2D(1f, 1f), new Point2D(2f, 3f));
                //var c3 = CollisionDetector.CheckCollision(line3, polygon);
                //Debug.WriteLine(c3);
                //Debug.Assert(!c3);
            }

            #endregion

            #region Manual Test (Collision: circle/polygon)

            {
                //var polygon = new ConvexPolygonPrimitive(
                //    new Point2D(0f, 0f),
                //    new Point2D(1f, 0f),
                //    new Point2D(0f, 1f));

                //// Circle is inside polygon
                //var circle1= new CirclePrimitive(new Point2D(0.2f, 0.2f), 0.1f);
                //var c1 = CollisionDetector.CheckCollision(circle1, polygon);
                //Debug.WriteLine(c1);
                //Debug.Assert(c1);

                //// Polygon is inside circle
                //var circle2 = new CirclePrimitive(new Point2D(0.25f, 0.25f), 5f);
                //var c2 = CollisionDetector.CheckCollision(circle2, polygon);
                //Debug.WriteLine(c2);
                //Debug.Assert(c2);

                //// Regular intersection
                //var circle3 = new CirclePrimitive(new Point2D(1f, 1f), 1f);
                //var c3 = CollisionDetector.CheckCollision(circle3, polygon);
                //Debug.WriteLine(c3);
                //Debug.Assert(c3);
            }

            #endregion

            InitializeComponent();

            var gameBoardSize = Settings.Default.NominalSize;
            var chickenUnitCount = Settings.Default.TeamUnitCount;

            m_gameEngine = new GameEngine(
                this.PaintGame,
                gameBoardSize,
                new ChickenTeamRecord(typeof(RandomChickenLogic), chickenUnitCount),
                new ChickenTeamRecord(typeof(RandomChickenLogic), chickenUnitCount));
            m_gameEngine.GameEnded += this.GameEngine_GameEnded;
        }

        #endregion

        #region Private Methods

        private DialogResult ShowMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(
                this,
                text,
                string.IsNullOrEmpty(caption) ? this.Text : caption + " – " + this.Text,
                buttons,
                icon);
        }

        private void ShowInfoMessage(string text, string caption = null)
        {
            ShowMessage(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowErrorMessage(string text, string caption = null)
        {
            ShowMessage(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void ShowErrorMessage(Exception exception)
        {
            #region Argument Check

            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            #endregion

            ShowErrorMessage(
                string.Format("An error occurred:{0}{0}{1}", Environment.NewLine, exception.ToString()),
                "Error");
        }

        private DialogResult ShowQuestion(
            string text,
            MessageBoxButtons buttons = MessageBoxButtons.YesNo,
            string caption = null)
        {
            return ShowMessage(text, caption, buttons, MessageBoxIcon.Question);
        }

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
            try
            {
                if (m_gameEngine.IsRunning)
                {
                    return;
                }

                var winningTeam = m_gameEngine.WinningTeam;
                if (winningTeam.HasValue)
                {
                    var dr = ShowQuestion(
                        string.Format(
                            "The game has ended. Winning team: {0}.{1}{1}"
                                + "Do you wish to restart the game?",
                            winningTeam.Value,
                            Environment.NewLine));
                    if (dr != DialogResult.Yes)
                    {
                        return;
                    }

                    ResetGame();
                }

                m_totalPaintCount = 0;
                ResetFpsCounter(true);
                m_gameEngine.Start();
                UpdateMoveCountStatus();
                fpsTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
                return;
            }
        }

        private void StopGame()
        {
            try
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
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
                return;
            }
        }

        private void ResetGame()
        {
            StopGame();
            m_gameEngine.Reset();
            m_gameEngine.CallPaint();
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
                    ResetGame();
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

            if (Settings.Default.DrawBoardGrid)
            {
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
            }
            else
            {
                var boardRect = pbGame.Bounds;
                ControlPaint.DrawFocusRectangle(graphics, boardRect);
                var backRect = boardRect;
                backRect.Inflate(-1, -1);
                graphics.FillRectangle(s_oddCellBrush, backRect);
            }

            // TODO: Move to static read-only fields
            var coefficient = (float)s_cellSize / GameConstants.NominalCellSize;
            var uiChickenBodyRadius = GameConstants.ChickenUnit.BodyCircleRadius * coefficient;
            var uiBeakOffset = GameConstants.ChickenUnit.BeakOffset * coefficient;
            var uiBeakRayOffset = GameConstants.ChickenUnit.BeakRayOffset * coefficient;

            foreach (var chickenUnit in m_lastPresentation.Chickens)
            {
                var isTeamA = chickenUnit.Logic.Team == GameTeam.TeamA;
                var brush = isTeamA ? Brushes.LightGreen : Brushes.DarkGreen;

                var uiPosition = chickenUnit.Position * coefficient;

                graphics.FillEllipse(
                    brush,
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

                graphics.FillPolygon(brush, beakPolygonPoints.ToPointF(), FillMode.Winding);

                var rcl = chickenUnit.Logic as RandomChickenLogic;
                if (rcl != null)
                {
                    var targetPointBrush = isTeamA ? Brushes.LightBlue : Brushes.DarkBlue;
                    // TODO: Move to static read-only fields
                    float uiTargetPointRadius = GameConstants.ShotUnit.Radius / 4f;
                    foreach (var targetPoint in rcl.TargetPoints)
                    {
                        var tp = targetPoint * coefficient;
                        graphics.FillRectangle(
                            targetPointBrush,
                            tp.X - uiTargetPointRadius,
                            tp.Y - uiTargetPointRadius,
                            2f * uiTargetPointRadius,
                            2f * uiTargetPointRadius);
                    }
                }
            }

            var uiShotRadius = GameConstants.ShotUnit.Radius * coefficient;
            foreach (var shotUnit in m_lastPresentation.Shots)
            {
                var shotBrush = shotUnit.Owner.Logic.Team == GameTeam.TeamA ? Brushes.Pink : Brushes.DarkRed;

                var uiPosition = shotUnit.Position * coefficient;
                graphics.FillEllipse(
                    shotBrush,
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

        private void GameEngine_GameEnded(object sender, GameEndedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action<object, GameEndedEventArgs>)this.GameEngine_GameEnded, sender, e);
                return;
            }

            m_gameEngine.Stop();

            ShowInfoMessage(string.Format("Winning team: {0}.", e.WinningTeam), "Game Ended");
        }

        #endregion
    }
}