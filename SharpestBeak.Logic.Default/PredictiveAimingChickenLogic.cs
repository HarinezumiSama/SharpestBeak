using System;

namespace SharpestBeak.Logic.Default
{
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