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
        public TrianglePrimitive(GamePoint first, GamePoint second, GamePoint third)
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

        public GamePoint Third
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
            return CollisionDetector.CheckPrimitiveCollision(this, other);
        }

        #endregion
    }
}