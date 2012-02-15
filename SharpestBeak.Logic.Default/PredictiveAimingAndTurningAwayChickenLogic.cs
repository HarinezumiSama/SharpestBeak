using System;

namespace SharpestBeak.Logic.Default
{
    public sealed class PredictiveAimingAndTurningAwayChickenLogic : SmartChickenLogicBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PredictiveAimingAndTurningAwayChickenLogic"/> class.
        /// </summary>
        public PredictiveAimingAndTurningAwayChickenLogic()
            : base(Features.PredictiveShot | Features.TurningAway)
        {
            // Nothing to do
        }

        #endregion
    }
}