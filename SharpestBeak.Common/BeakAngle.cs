using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    /// <summary>
    ///     NOTE 1: The order of elements MUST correspond to clockwise direction.
    ///     NOTE 2: The elements' values must be consecutive.
    /// </summary>
    public enum BeakAngle
    {
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,

        // Hardcoded so far
        Min = Up,
        Max = UpLeft
    }
}