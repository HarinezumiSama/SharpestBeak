﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak
{
    public sealed class MoveInfo
    {
        #region Fields

        private static readonly MoveInfo s_empty = new MoveInfo(MoveDirection.None, BeakTurn.None, FireMode.None);

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MoveInfo"/> class.
        /// </summary>
        public MoveInfo(MoveDirection moveDirection, BeakTurn beakTurn, FireMode fireMode)
        {
            #region Argument Check

            if (!Enum.IsDefined(typeof(MoveDirection), moveDirection))
            {
                throw new ArgumentOutOfRangeException("moveDirection", moveDirection, "Invalid move action.");
            }
            if (!Enum.IsDefined(typeof(FireMode), fireMode))
            {
                throw new ArgumentOutOfRangeException("fireMode", fireMode, "Invalid fire mode.");
            }

            #endregion

            this.MoveDirection = moveDirection;
            this.BeakTurn = beakTurn;
            this.FireMode = fireMode;
        }

        #endregion

        #region Public Properties

        public static MoveInfo Empty
        {
            [DebuggerStepThrough]
            get { return s_empty; }
        }

        public MoveDirection MoveDirection
        {
            get;
            private set;
        }

        public BeakTurn BeakTurn
        {
            get;
            private set;
        }

        public FireMode FireMode
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format(
                "{0}. BeakTurn = {1}, MoveDirection = {2}, FireMode = {3}",
                GetType().Name,
                this.BeakTurn,
                this.MoveDirection,
                this.FireMode);
        }

        #endregion
    }
}