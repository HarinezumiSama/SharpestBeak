using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
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