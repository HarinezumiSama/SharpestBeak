using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public abstract class ChickenUnit
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenUnit"/> class.
        /// </summary>
        protected ChickenUnit()
        {
            // Nothing to do
        }

        #endregion

        #region Protected Properties

        protected internal GameBoard Board
        {
            get;
            internal set;
        }

        protected bool CanPlay
        {
            [DebuggerNonUserCode]
            get { return this.Board != null && !this.IsDead; }
        }

        #endregion

        #region Protected Methods

        protected List<ChickenUnit> GetOtherChickens()
        {
            if (this.Board == null)
            {
                throw new InvalidOperationException();
            }

            return this.Board.AliveChickens.Where(item => item != this).ToList();
        }

        protected abstract MoveInfo OnMakeMove();

        #endregion

        #region Public Properties

        public Point Position
        {
            get;
            internal set;
        }

        public BeakAngle BeakAngle
        {
            get;
            internal set;
        }

        public bool IsPecked
        {
            get;
            internal set;
        }

        public bool IsDead
        {
            get;
            internal set;
        }

        public MoveInfo CurrentMove
        {
            get;
            internal set;
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