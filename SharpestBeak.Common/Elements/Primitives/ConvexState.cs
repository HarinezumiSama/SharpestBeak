using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Elements.Primitives
{
    public enum ConvexState
    {
        Undefined = 0,
        Concave,
        ConvexCounterClockwise,
        ConvexClockwise
    }
}