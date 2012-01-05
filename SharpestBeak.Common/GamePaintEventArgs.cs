using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GamePaintEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePaintEventArgs"/> class.
        /// </summary>
        internal GamePaintEventArgs(GamePresentation presentation)
        {
            #region Argument Check

            if (presentation == null)
            {
                throw new ArgumentNullException("presentation");
            }

            #endregion

            this.Presentation = presentation;
        }

        #endregion

        #region Public Properties

        public GamePresentation Presentation
        {
            get;
            private set;
        }

        #endregion
    }
}