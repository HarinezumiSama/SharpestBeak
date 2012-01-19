﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Presentation.Elements;

namespace SharpestBeak.Common.Presentation
{
    public class ChickenPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenPresentation"/> class.
        /// </summary>
        internal ChickenPresentation(ChickenUnit chickenUnit)
        {
            #region Argument Check

            if (chickenUnit == null)
            {
                throw new ArgumentNullException("chickenUnit");
            }

            #endregion

            this.Element = chickenUnit.GetElement();
            this.State = new ChickenUnitState(chickenUnit);
        }

        #endregion

        #region Public Properties

        public ChickenElement Element
        {
            get;
            private set;
        }

        public ChickenUnitState State
        {
            get;
            private set;
        }

        #endregion
    }
}