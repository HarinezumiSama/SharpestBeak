using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Elements;

namespace SharpestBeak.Common
{
    public sealed class GamePresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePresentation"/> class.
        /// </summary>
        internal GamePresentation(GameEngine engine)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            #endregion

            this.CommonData = engine.CommonData;
            this.Chickens = new List<ChickenUnit>(engine.AliveChickensDirect).AsReadOnly();
            this.Shots = new List<ShotUnit>(engine.ShotUnitsDirect).AsReadOnly();
        }

        #endregion

        #region Public Properties

        public GameCommonData CommonData
        {
            get;
            private set;
        }

        // TODO: [VM] Return ChickenUnitView or something like that
        public IList<ChickenUnit> Chickens
        {
            get;
            private set;
        }

        // TODO: [VM] Return ShotUnitView or something like that
        public IList<ShotUnit> Shots
        {
            get;
            private set;
        }

        #endregion
    }
}