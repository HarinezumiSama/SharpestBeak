using System.ComponentModel;

namespace SharpestBeak.Logic.Default;

[Description("Aiming")]
public sealed class AimingChickenLogic : SmartChickenLogicBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AimingChickenLogic"/> class.
    /// </summary>
    public AimingChickenLogic()
        : base(Features.Default)
    {
        // Nothing to do
    }
}