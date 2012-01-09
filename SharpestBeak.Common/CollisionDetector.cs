using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace SharpestBeak.Common
{
    public static class CollisionDetector
    {
        #region Internal Methods

        internal static bool CheckElementCollision(ICollidableElement element, ICollidable other)
        {
            #region Argument Check

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            #endregion

            if (element.HasRoughPrimitives)
            {
                var canCollide = false;
                var roughPrimitives = element.GetRoughPrimitives();
                foreach (var roughPrimitive in roughPrimitives)
                {
                    if (roughPrimitive.HasCollision(other))
                    {
                        canCollide = true;
                        break;
                    }
                }

                if (!canCollide)
                {
                    return false;
                }
            }

            var primitives = element.GetPrimitives();
            foreach (var primitive in primitives)
            {
                if (primitive.HasCollision(other))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Public Methods

        public static bool CheckCollision(ICollidable first, ICollidable second)
        {
            #region Argument Check

            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }

            #endregion

            return first.HasCollision(second);
        }

        #endregion
    }
}