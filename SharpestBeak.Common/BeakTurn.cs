using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    // TODO: [VM] Replace with float number in range -1.0 to +1.0 so that a unit can rotate its beak more exactly
    public enum BeakTurn
    {
        Clockwise = -1,
        None = 0,
        CounterClockwise = 1
    }
}