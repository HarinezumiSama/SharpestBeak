using System;

namespace SharpestBeak.Model
{
    [Flags]
    public enum MoveInfoStates
    {
        Handled = 0,
        RejectedBoardCollision = 0x01,
        RejectedOtherUnitCollision = 0x02,

        RejectedMask = unchecked((int)0xFFFFFFFF)
    }
}