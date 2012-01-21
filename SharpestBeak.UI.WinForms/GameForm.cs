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
using SharpestBeak.Common.Diagnostics;
using SharpestBeak.Common.Presentation;
using SharpestBeak.Logic.Default;
using SharpestBeak.UI.WinForms.Properties;

namespace SharpestBeak.UI.WinForms
{
    public partial class GameForm : Form
    {
        #region Fields

        private static readonly int s_cellSize = 48;
        private static readonly int s_fullCellSize = s_cellSize + 1;

        private static readonly float s_uiCoefficient = (float)s_cellSize / GameConstants.NominalCellSize;

        private static readonly Brush s_evenCellBrush = new SolidBrush(SystemColors.Window);
        private static readonly Brush s_oddCellBrush = new SolidBrush(ControlPaint.Dark(SystemColors.Window, 0.05f));

        private static readonly DrawData s_teamAUnitDrawData = new DrawData(Color.LightGreen, s_uiCoefficient);
        private static readonly DrawData s_teamBUnitDrawData = new DrawData(Color.DarkGreen, s_uiCoefficient);

        private static readonly DrawData s_teamAShotDrawData = new DrawData(Color.Pink, s_uiCoefficient);
        private static readonly DrawData s_teamBShotDrawData = new DrawData(Color.DarkRed, s_uiCoefficient);

        private static readonly float s_uiTargetPointRadius = GameConstants.ShotUnit.Radius / 4f;

        private readonly GameEngine m_gameEngine;
        private readonly object m_lastPresentationLock = new object();
        private GamePresentation m_lastPresentation;
        private ulong m_totalPaintCount;
        private Bitmap m_gameBoardBackground;

        private float m_fps;
        private int m_fpsPaintCount;
        private readonly Stopwatch m_fpsStopwatch = new Stopwatch();

        #endregion

        #region Constructors

        private GameForm()
        {
            InitializeComponent();
        }

        public GameForm(Size nominalSize, ChickenTeamRecord lightTeam, ChickenTeamRecord darkTeam)
            : this()
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

            #region Manual Test (Vector2D.GetAngle)

            {
                //var v1 = new Vector2D(1f, 1f);

                //var v2 = new Vector2D(0f, 1f);
                //var angle1to2 = v1.GetAngle(v2);
                //Debug.WriteLine(angle1to2);
                //Debug.Assert(angle1to2.DegreeValue == 45f);

                //var v3 = new Vector2D(1f, 0f);
                //var angle1to3 = v1.GetAngle(v3);
                //Debug.WriteLine(angle1to3);
                //Debug.Assert(angle1to3.DegreeValue == -45f);

                //var v4 = new Vector2D(-1f, -1f);
                //var angle1to4 = v1.GetAngle(v4);
                //Debug.WriteLine(angle1to4);
                //Debug.Assert(angle1to4.DegreeValue == 180f);

                //var v5 = new Vector2D(-1f, 0f);
                //var angle1to5 = v1.GetAngle(v5);
                //Debug.WriteLine(angle1to5);
                //Debug.Assert(angle1to5.DegreeValue == 135f);
                //var angle5to1 = v5.GetAngle(v1);
                //Debug.WriteLine(angle5to1);
                //Debug.Assert(angle5to1.DegreeValue == -135f);

                //var v6 = new Vector2D(2f, 2f);
                //var angle1to6 = v1.GetAngle(v6);
                //Debug.WriteLine(angle1to6);
                //Debug.Assert(angle1to6.DegreeValue == 0f);
            }

            #endregion

            m_gameEngine = new GameEngine(this.PaintGame, nominalSize, lightTeam, darkTeam);
            m_gameEngine.GameEnded += this.GameEngine_GameEnded;
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
                if (this.ParentForm != null)
                {
                    this.CenterToParent();
                }
                else
                {
                    this.CenterToScreen();
                }

                ClearStatusLabels();
                UpdateMoveCountStatus();

                DebugHelper.CallAndMeasure(this.ResetGameBoardBackground);

                m_gameEngine.CallPaint();
            }
            catch (Exception ex)
            {
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
                return false;
            }

            return true;
        }

        private void ResetGameBoardBackground()
        {
            Helper.DisposeAndNull(ref m_gameBoardBackground);

            try
            {
                var size = m_gameEngine.CommonData.NominalSize;

                m_gameBoardBackground = new Bitmap(s_cellSize * size.Width, s_cellSize * size.Height);

                using (var graphics = Graphics.FromImage(m_gameBoardBackground))
                {
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
                        var boardRect = new Rectangle(Point.Empty, m_gameBoardBackground.Size);
                        ControlPaint.DrawFocusRectangle(graphics, boardRect);
                        var backRect = boardRect;
                        backRect.Inflate(-1, -1);
                        graphics.FillRectangle(s_oddCellBrush, backRect);
                    }
                }
            }
            catch (Exception)
            {
                Helper.DisposeAndNull(ref m_gameBoardBackground);
                throw;
            }
        }

        private void PaintGame(GamePaintEventArgs e)
        {
            lock (m_lastPresentationLock)
            {
                m_lastPresentation = e.Presentation;
            }

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
                    var dr = this.ShowQuestion(
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
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
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
                if (ex.IsThreadAbort())
                {
                    throw;
                }

                this.ShowErrorMessage(ex);
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
            GamePresentation lastPresentation;
            lock (m_lastPresentationLock)
            {
                lastPresentation = m_lastPresentation;
            }
            if (lastPresentation == null)
            {
                return;
            }

            var size = lastPresentation.CommonData.NominalSize;
            var graphics = e.Graphics;

            graphics.DrawImageUnscaled(m_gameBoardBackground.EnsureNotNull(), Point.Empty);

            foreach (var chickenUnit in lastPresentation.Chickens)
            {
                var drawData = chickenUnit.State.Team == GameTeam.Light ? s_teamAUnitDrawData : s_teamBUnitDrawData;

                chickenUnit.Element.Draw(graphics, drawData);
            }

            // TODO: [VM] Allow logic to provide some extra data for drawing (think up how to do it secure)
            foreach (var teamLogic in m_gameEngine.Teams)
            {
                var rcl = teamLogic as RandomChickenLogic;
                if (rcl != null)
                {
                    var targetPointBrush = teamLogic.Team == GameTeam.Light ? Brushes.LightBlue : Brushes.DarkBlue;
                    foreach (var targetPoint in rcl.TargetPoints)
                    {
                        var tp = targetPoint * s_uiCoefficient;
                        graphics.FillRectangle(
                            targetPointBrush,
                            tp.X - s_uiTargetPointRadius,
                            tp.Y - s_uiTargetPointRadius,
                            2f * s_uiTargetPointRadius,
                            2f * s_uiTargetPointRadius);
                    }
                }
            }

            var uiShotRadius = GameConstants.ShotUnit.Radius * s_uiCoefficient;
            foreach (var shotUnit in lastPresentation.Shots)
            {
                var drawData = shotUnit.Owner.State.Team == GameTeam.Light ? s_teamAShotDrawData : s_teamBShotDrawData;
                shotUnit.Element.Draw(graphics, drawData);
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
            m_gameEngine.CallPaint();

            this.ShowInfoMessage(string.Format("Winning team: {0}.", e.WinningTeam), "Game Ended");
        }

        #endregion
    }
}