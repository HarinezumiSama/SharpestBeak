using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class MoveInfo
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MoveInfo"/> class.
        /// </summary>
        public MoveInfo(BeakTurn beak, MoveAction move)
        {
            this.Beak = beak;
            this.Move = move;
        }

        #endregion

        #region Public Properties

        public BeakTurn Beak
        {
            get;
            private set;
        }

        public MoveAction Move
        {
            get;
            private set;
        }

        #endregion
    }
}