using System;

namespace SharpestBeak
{
    [Flags]
    public enum MoveInfoStates : int
    {
        Handled = 0,
        RejectedBoardCollision = 0x01,
        RejectedOtherUnitCollision = 0x02,

        RejectedMask = unchecked((int)0xFFFFFFFF)
    }
}