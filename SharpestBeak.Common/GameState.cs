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
        internal GameState(GameCommonData data)
        {
            #region Argument Check

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            #endregion

            this.Data = data;
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