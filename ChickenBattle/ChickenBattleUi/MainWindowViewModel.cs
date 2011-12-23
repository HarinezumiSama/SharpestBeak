using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChickenBattle.Core;
using ChickenBattle.Yegorov;
using ChickenBattleUi.Commands;

namespace ChickenBattleUi
{
    sealed class MainWindowViewModel
    {
        public ExitCommand ExitCommand { get; set; }

        public List<IChicken> ChickenList { get; private set; }

        public GameInfo GameInfo { get; set; }

        private void InitCommands()
        {
            ExitCommand = new ExitCommand();
        }

        public MainWindowViewModel()
        {
            InitCommands();

            ChickenList = new List<IChicken>();
            GameInfo = new GameInfo() {FieldHeight = 20, FieldWidth = 20, ChickenHitPoints = 3, Players = ChickenList};

            ChickenList.Add(new CrazyChicken(GameInfo));
            ChickenList.Add(new CrazyChicken(GameInfo));
        }
    }
}
