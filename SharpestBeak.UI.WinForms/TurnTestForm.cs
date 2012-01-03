using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SharpestBeak.UI.WinForms
{
    public partial class TurnTestForm : Form
    {
        #region Nested Types

        #region ChickenData Class

        private sealed class ChickenData
        {
            #region Fields

            private PointF? m_savedBodyCenter;
            private float? m_savedBeakAngle;

            #endregion

            #region Constructors

            internal ChickenData(Brush brush)
            {
                #region Argument Check

                if (brush == null)
                {
                    throw new ArgumentNullException("brush");
                }

                #endregion

                this.Brush = brush;
            }

            #endregion

            #region Public Properties

            public PointF BodyCenter
            {
                get;
                set;
            }

            public float BeakAngle
            {
                get;
                set;
            }

            public Brush Brush
            {
                get;
                private set;
            }

            #endregion

            #region Public Methods

            public Region GetRegion()
            {
                using (var path = new GraphicsPath(FillMode.Winding))
                {
                    path.AddEllipse(
                        new RectangleF(
                            this.BodyCenter.X - c_chickenBodyRadius,
                            this.BodyCenter.Y - c_chickenBodyRadius,
                            2 * c_chickenBodyRadius,
                            2 * c_chickenBodyRadius));

                    var defaultBeakPolygonPoints = new[]
                    {
                        new PointF(this.BodyCenter.X - c_chickenBodyRadius / 2f, this.BodyCenter.Y),
                        new PointF(this.BodyCenter.X, this.BodyCenter.Y - c_chickenBeakOffset),
                        new PointF(this.BodyCenter.X + c_chickenBodyRadius / 2f, this.BodyCenter.Y)
                    };
                    var beakPolygonPoints = RotatePoints(defaultBeakPolygonPoints, this.BodyCenter, this.BeakAngle);
                    path.AddPolygon(beakPolygonPoints);

                    return new Region(path);
                }
            }

            public void SaveState()
            {
                m_savedBodyCenter = this.BodyCenter;
                m_savedBeakAngle = this.BeakAngle;
            }

            public void RestoreState()
            {
                if (m_savedBodyCenter.HasValue)
                {
                    this.BodyCenter = m_savedBodyCenter.Value;
                }
                if (m_savedBeakAngle.HasValue)
                {
                    this.BeakAngle = m_savedBeakAngle.Value;
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const int c_cellSize = 100;
        private const float c_cellSizeF = (float)c_cellSize;
        private const float c_chickenBodyRadius = c_cellSizeF / 4f;
        private const float c_chickenBeakOffset = c_cellSizeF * 3f / 8f;

        private const int c_stepCount = 300;
        private const float c_stepCountF = (float)c_stepCount;

        // This constant is subject to change fore testing purposes
        private const int c_speedA = 3;

        #endregion

        #region Fields

        private static readonly int s_cellCount = Math.Max(2, c_speedA);

        private readonly ChickenData m_chickenA = new ChickenData(Brushes.DarkRed)
        {
            BodyCenter = new PointF(c_cellSizeF / 2, c_cellSizeF / 2),
            BeakAngle = 90f
        };
        private readonly ChickenData m_chickenB = new ChickenData(Brushes.DarkGreen)
        {
            BodyCenter = new PointF(c_cellSizeF + c_cellSizeF / 2, c_cellSizeF + c_cellSizeF / 2),
            BeakAngle = 0f
        };

        private int m_stepIndex;
        private int m_substepIndexA;

        #endregion

        #region Constructors

        public TurnTestForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Private Methods

        private static PointF[] RotatePoints(IEnumerable<PointF> values, PointF center, float degreeAngle)
        {
            var degree = degreeAngle / 180d * Math.PI;
            var cos = Math.Cos(degree);
            var sin = Math.Sin(degree);

            return values
                .Select(
                    value =>
                    {
                        var newX = center.X + (value.X - center.X) * cos - (value.Y - center.Y) * sin;
                        var newY = center.Y + (value.X - center.X) * sin + (value.Y - center.Y) * cos;
                        return new PointF((float)newX, (float)newY);
                    })
                .ToArray();
        }

        private void DrawChicken(Graphics graphics, ChickenData chicken)
        {
            using (var region = chicken.GetRegion())
            {
                graphics.FillRegion(chicken.Brush, region);
            }
        }

        private void DrawGrid(Graphics graphics)
        {
            for (int y = 0; y < s_cellCount; y++)
            {
                for (int x = 0; x < s_cellCount; x++)
                {
                    var rect = new Rectangle(x * c_cellSize, y * c_cellSize, c_cellSize, c_cellSize);
                    ControlPaint.DrawFocusRectangle(graphics, rect);

                    if ((x == 0 && y == 1) || (x > 1 && y > 0))
                    {
                        using (var brush = new HatchBrush(HatchStyle.DottedDiamond, Color.WhiteSmoke))
                        {
                            graphics.FillRectangle(brush, rect);
                        }
                    }
                }
            }
        }

        private bool ChickensIntersect()
        {
            using (var regionA = m_chickenA.GetRegion())
            {
                using (var regionB = m_chickenB.GetRegion())
                {
                    regionA.Intersect(regionB);
                    using (var graphics = this.CreateGraphics())
                    {
                        return !regionA.IsEmpty(graphics);
                    }
                }
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var size = Math.Max(2, c_cellSize * s_cellCount);
            this.ClientSize = new Size(size, size);
            if (this.Parent == null)
            {
                this.CenterToScreen();
            }
            else
            {
                this.CenterToParent();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawGrid(e.Graphics);
            DrawChicken(e.Graphics, m_chickenA);
            DrawChicken(e.Graphics, m_chickenB);
        }

        #endregion

        #region Event Handlers

        private void TurnTestForm_Click(object sender, EventArgs e)
        {
            moveTimer.Enabled = m_stepIndex < c_stepCount && !moveTimer.Enabled;
        }

        private void moveTimer_Tick(object sender, EventArgs e)
        {
            m_stepIndex++;
            if (m_stepIndex > c_stepCount)
            {
                moveTimer.Enabled = false;
                return;
            }

            m_substepIndexA++;

            m_chickenA.SaveState();
            if (m_substepIndexA * c_speedA < c_stepCount)
            {
                m_chickenA.BodyCenter = new PointF(
                    m_chickenA.BodyCenter.X + c_cellSizeF / c_stepCountF * c_speedA,
                    m_chickenA.BodyCenter.Y);
                m_chickenA.BeakAngle += 90f / c_stepCountF * c_speedA;
            }
            else
            {
                m_chickenA.BodyCenter = new PointF(
                    m_chickenA.BodyCenter.X,
                    m_chickenA.BodyCenter.Y + c_cellSizeF / c_stepCountF * c_speedA);
            }
            if (ChickensIntersect())
            {
                m_chickenA.RestoreState();
            }

            m_chickenB.SaveState();
            m_chickenB.BodyCenter = new PointF(
                m_chickenB.BodyCenter.X,
                m_chickenB.BodyCenter.Y - c_cellSizeF / c_stepCountF);
            m_chickenB.BeakAngle -= 90f / c_stepCountF;
            if (ChickensIntersect())
            {
                m_chickenB.RestoreState();
            }

            this.Invalidate();
        }

        #endregion
    }
}