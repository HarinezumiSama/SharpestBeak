using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    // TODO: [VM] Implement a partial move in a particular direction: (0; 1] (similar to that in BeakTurn)

    /// <summary>
    ///     Specifies the move direction relative to beak direction.
    /// </summary>
    public enum MoveDirection
    {
        None,
        MoveForward,
        MoveBackward,
        StrafeRight,
        StrafeLeft
    }
}