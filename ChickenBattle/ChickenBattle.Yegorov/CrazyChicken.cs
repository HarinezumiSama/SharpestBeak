using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChickenBattle.Core;

namespace ChickenBattle.Yegorov
{
    public class CrazyChicken : ChickenBase
    {
        private Random _random = new Random();

        public CrazyChicken(GameInfo gameInfo) : base(gameInfo)
        {
        }

        protected override void OnMakeTurn()
        {
            switch (_random.Next(7))
            {
                case 0:
                    MoveLeft();
                    break;
                case 1:
                    MoveRight();
                    break;
                case 2: 
                    MoveDown();
                    break;
                case 3:
                    MoveUp();
                    break;
                case 4:
                    RotateClockwise();
                    break;
                case 5:
                    RotateCounterclockwise();
                    break;
                case 6:
                    InflictDamage();
                    break;
            }
        }

        public override string AiName
        {
            get { return "Yegoroff's crazy chicken"; }
        }
    }
}
