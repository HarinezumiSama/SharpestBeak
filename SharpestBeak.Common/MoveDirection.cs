using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    // TODO: [VM] Change move logic: direction should relative to chicken beak orientation, not relative to game board
    public enum MoveDirection
    {
        None,
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft
    }
}