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
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,

        // Hardcoded so far
        Min = Up,
        Max = UpLeft
    }
}