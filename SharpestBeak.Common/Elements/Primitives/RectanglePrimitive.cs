using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public sealed class RectanglePrimitive : ICollidablePrimitive
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RectanglePrimitive"/> class.
        /// </summary>
        public RectanglePrimitive(GamePoint first, GamePoint second)
        {
            #region Argument Check

            if (first.GetDistance(second).IsZero())
            {
                throw new ArgumentException("Rectangle points must be different.");
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