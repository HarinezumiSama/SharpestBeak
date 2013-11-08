﻿using System;
using System.ComponentModel;

namespace SharpestBeak.Logic.Default
{
    [Description("Predictive aiming")]
    public sealed class PredictiveAimingChickenLogic : SmartChickenLogicBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PredictiveAimingChickenLogic"/> class.
        /// </summary>
        public PredictiveAimingChickenLogic()
            : base(Features.PredictiveShot)
        {
            // Nothing to do
        }

        #endregion
    }
}