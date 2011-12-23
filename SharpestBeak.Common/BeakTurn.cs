using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    /// <summary>
    ///     NOTE: Enumeration constants correspond to offset in clockwise-ordered <see cref="BeakAngle"/> enumeration.
    /// </summary>
    public enum BeakTurn
    {
        CounterClockwise = -1,
        None = 0,
        Clockwise = 1,
    }
}