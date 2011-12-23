using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChickenBattle.Core
{
    public class GameInfo
    {
        public int FieldWidth { get; set; }
        public int FieldHeight { get; set; }
        public int ChickenHitPoints { get; set; }
        public IEnumerable<IChicken> Players { get; set; }
    }
}
