using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public abstract class ChickenUnitLogic
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnitLogic"/> class.
        /// </summary>
        protected ChickenUnitLogic()
        {
            // Nothing to do
        }

        #endregion

        #region Protected Properties

        protected bool CanPlay
        {
            [DebuggerNonUserCode]
            get { return this.Board != null && this.Unit != null && !this.IsDead; }
        }

        #endregion

        #region Protected Internal Properties

        protected internal ChickenUnit Unit
        {
            get;
            internal set;
        }

        protected internal GameBoard Board
        {
            get;
            internal set;
        }

        #endregion

        #region Protected Methods

        protected List<ChickenUnit> GetOtherChickens()
        {
            if (this.Board == null || this.Unit == null)
            {
                throw new InvalidOperationException("Not initialized yet.");
            }

            return this.Board.AliveChickens.Where(item => item != this.Unit).ToList();
        }

        protected abstract MoveInfo OnMakeMove();

        #endregion

        #region Public Properties

        public MoveInfo CurrentMove
        {
            get;
            internal set;
        }

        public bool IsDead
        {
            [DebuggerNonUserCode]
            get { return this.Unit == null || this.Unit.IsDead; }
        }

        #endregion

        #region Public Methods

        public MoveInfo MakeMove()
        {
            this.CurrentMove = this.CanPlay ? OnMakeMove() : null;
            return this.CurrentMove;
        }

        #endregion
    }
}