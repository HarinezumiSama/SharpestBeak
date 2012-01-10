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
        public LinePrimitive(GamePoint first, GamePoint second)
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

        public GamePoint First
        {
            get;
            private set;
        }

        public GamePoint Second
        {
            get;
            private set;
        }

        #endregion

        #region ICollidablePrimitive Members

        public bool HasCollision(ICollidablePrimitive other)
        {
            #region Argument Check

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            var otherLine = other as LinePrimitive;
            if (otherLine != null)
            {
                return CollisionDetector.CheckLineToLineCollision(this, otherLine);
            }

            throw new NotSupportedException();
        }

        #endregion

        #region ICollidable Members

        public bool HasCollision(ICollidable other)
        {
            return CollisionDetector.CheckPrimitiveCollision(this, other);
        }

        #endregion
    }
}