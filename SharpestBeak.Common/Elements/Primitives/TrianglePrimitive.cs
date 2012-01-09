using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class TrianglePrimitive : ICollidablePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TrianglePrimitive"/> class.
        /// </summary>
        public TrianglePrimitive(PointF first, PointF second, PointF third)
        {
            #region Argument Check

            if (first.GetDistance(second).IsZero()
                || first.GetDistance(third).IsZero()
                || second.GetDistance(third).IsZero())
            {
                throw new ArgumentException("Triangle points must be different.");
            }

            #endregion

            this.First = first;
            this.Second = second;
            this.Third = third;
        }

        #endregion

        #region Public Properties

        public PointF First
        {
            get;
            private set;
        }

        public PointF Second
        {
            get;
            private set;
        }

        public PointF Third
        {
            get;
            private set;
        }

        #endregion

        #region ICollidablePrimitive Members

        public bool HasCollision(ICollidablePrimitive other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollidable Members

        public bool HasCollision(ICollidable other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}