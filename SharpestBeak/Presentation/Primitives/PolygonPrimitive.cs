using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation.Primitives
{
    public class PolygonPrimitive : BasePrimitive
    {
        #region Constants and Fields

        public const int MinVertexCount = 3;

        private readonly ReadOnlyCollection<LinePrimitive> _edges;
        private ConvexState? _convexState;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonPrimitive"/> class.
        /// </summary>
        public PolygonPrimitive(IEnumerable<Point2D> vertices)
        {
            #region Argument Check

            if (vertices == null)
            {
                throw new ArgumentNullException("vertices");
            }

            #endregion

            this.Vertices = vertices.ToArray().AsReadOnly();
            _edges = GetEdges(this.Vertices);
            this.Count = this.Vertices.Count;
            this.BasePoint = this.Vertices.First();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PolygonPrimitive"/> class.
        /// </summary>
        public PolygonPrimitive(params Point2D[] vertices)
            : this((IEnumerable<Point2D>)vertices)
        {
            // Nothing to do
        }

        #endregion

        #region ConvexSign Enumeration

        [Flags]
        private enum ConvexSign
        {
            None = 0,

            Positive = 0x1,
            Negative = 0x2,

            Both = Positive | Negative
        }

        #endregion

        #region Public Properties

        public ReadOnlyCollection<Point2D> Vertices
        {
            get;
            private set;
        }

        public ReadOnlyCollection<LinePrimitive> Edges
        {
            [DebuggerStepThrough]
            get
            {
                return _edges;
            }
        }

        public int Count
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public ConvexState GetConvexState()
        {
            if (!_convexState.HasValue)
            {
                _convexState = GetConvexState(this.Edges);
            }

            return _convexState.Value;
        }

        public bool IsConvex()
        {
            var state = GetConvexState();
            return state == ConvexState.ConvexClockwise || state == ConvexState.ConvexCounterClockwise;
        }

        #endregion

        #region Protected Methods

        protected static ReadOnlyCollection<LinePrimitive> GetEdges(IList<Point2D> vertices)
        {
            #region Argument Check

            if (vertices == null)
            {
                throw new ArgumentNullException("vertices");
            }

            if (vertices.Count < MinVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of vertices in the polygon must be at least {0} while it is {1}.",
                        MinVertexCount,
                        vertices.Count),
                    "vertices");
            }

            #endregion

            var count = vertices.Count;
            var resultProxy = new List<LinePrimitive>(count);
            var currentPoint = vertices[0];
            for (var nextIndex = 1; nextIndex <= count; nextIndex++)
            {
                var nextPoint = nextIndex < count ? vertices[nextIndex] : vertices[0];
                resultProxy.Add(new LinePrimitive(currentPoint, nextPoint));
                currentPoint = nextPoint;
            }

            if (resultProxy.Count != count)
            {
                throw new InvalidOperationException();
            }

            return resultProxy.AsReadOnly();
        }

        protected static ConvexState GetConvexState(IList<LinePrimitive> edges)
        {
            #region Argument Check

            if (edges == null)
            {
                throw new ArgumentNullException("edges");
            }

            if (edges.Count < MinVertexCount)
            {
                throw new ArgumentException(
                    string.Format(
                        "The number of edges in the polygon must be at least {0} while it is {1}.",
                        MinVertexCount,
                        edges.Count),
                    "edges");
            }

            #endregion

            var sign = ConvexSign.None;
            for (var index = 0; index < edges.Count; index++)
            {
                var nextIndex = (index + 1) % edges.Count;

                var edge1 = edges[index].Direction;
                var edge2 = edges[nextIndex].Direction;

                var z = edge1 ^ edge2;
                if (z.IsPositive())
                {
                    sign |= ConvexSign.Positive;
                }
                else if (z.IsNegative())
                {
                    sign |= ConvexSign.Negative;
                }

                if ((sign & ConvexSign.Both) == ConvexSign.Both)
                {
                    return ConvexState.Concave;
                }
            }

            switch (sign)
            {
                case ConvexSign.None:
                    return ConvexState.Undefined;
                case ConvexSign.Positive:
                    return ConvexState.ConvexCounterClockwise;
                case ConvexSign.Negative:
                    return ConvexState.ConvexClockwise;
            }

            throw new InvalidOperationException(
                string.Format("Unexpected convex computation state: {0}.", sign));
        }

        protected override void OnDraw(Graphics graphics, DrawData data)
        {
            graphics.FillPolygon(
                data.Brush,
                this.Vertices.Select(v => v * data.Coefficient).ToPointF(),
                FillMode.Winding);
        }

        #endregion
    }
}