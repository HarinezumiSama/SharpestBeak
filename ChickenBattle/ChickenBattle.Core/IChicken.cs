using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChickenBattle.Core
{
    public interface IChicken
    {
        // chicken logic should be contained here
        void MakeTurn();

        // clockwise, ↑ is 0
        int Angle { get; }
        
        // X-coord
        int X { get; }

        // Y-coord 
        int Y { get; }

        int HitPoints { get; set; }

        string AiName { get; }
    }
}
