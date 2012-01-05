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
        internal GameState()
        {
            // TODO:
        }

        #endregion

        #region Public Properties

        // TODO: Remove reference to engine and compute details according to actual unit view
        public GameEngine Engine
        {
            get;
            internal set;
        }

        #endregion
    }
}