using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class LinePrimitive : ICollidablePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinePrimitive"/> class.
        /// </summary>
        public LinePrimitive(PointF first, PointF second)
        {
            #region Argument Check

            if (first.GetDistance(second).IsZero())
            {
                throw new ArgumentException("Line endpoints cannot be the same.", "second");
            }

            #endregion

            this.First = first;
            this.Second = second;
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