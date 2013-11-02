using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpestBeak.Model
{
    public sealed class GameState
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        internal GameState(GameEngine engine, ICollection<ChickenUnitState> unitStates)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            if (unitStates == null)
            {
                throw new ArgumentNullException("unitStates");
            }

            if (unitStates.Any(item => item == null))
            {
                throw new ArgumentException("The collection contains a null element.", "unitStates");
            }

            #endregion

            this.Data = engine.Data;
            this.UnitStates = unitStates.ToList().AsReadOnly();
        }

        #endregion

        #region Public Properties

        public GameEngineData Data
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