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
            get
            {
                return this.Board != null && !this.IsDead;
            }
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

        public int UniqueIndex
        {
            get;
            internal set;
        }

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

        public ChickenUnit PeckedBy
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

        public int KillCount
        {
            get;
            internal set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "[#{0} : {1}] Position = {2}, KillCount = {3}",
                this.UniqueIndex,
                this.GetType().Name,
                this.Position,
                this.KillCount);
        }

        public MoveInfo MakeMove()
        {
            this.CurrentMove = this.CanPlay ? OnMakeMove() : null;
            return this.CurrentMove;
        }

        #endregion
    }
}