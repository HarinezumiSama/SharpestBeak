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
        internal GameState(GameEngine engine)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            #endregion

            this.Data = engine.CommonData;
        }

        #endregion

        #region Public Properties

        public GameCommonData Data
        {
            get;
            private set;
        }

        // TODO: Compute details according to actual unit view

        #endregion

    }
}