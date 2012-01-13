using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameState
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        internal GameState(GameEngine engine, IEnumerable<ChickenUnit> chickens)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }
            if (chickens == null)
            {
                throw new ArgumentNullException("chickens");
            }

            #endregion

            this.Data = engine.CommonData;
            this.UnitStates = chickens.Select(item => new ChickenUnitState(item)).ToList().AsReadOnly();
        }

        #endregion

        #region Public Properties

        public GameCommonData Data
        {
            get;
            private set;
        }

        public IList<ChickenUnitState> UnitStates
        {
            get;
            private set;
        }

        #endregion
    }
}