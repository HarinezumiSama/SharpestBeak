using System;

namespace SharpestBeak.Model;

public sealed class MoveInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MoveInfo"/> class.
    /// </summary>
    public MoveInfo(MoveDirection moveDirection, BeakTurn beakTurn, FireMode fireMode)
    {
        if (moveDirection is null)
        {
            throw new ArgumentNullException(nameof(moveDirection));
        }

        fireMode.EnsureDefined();

        MoveDirection = moveDirection;
        BeakTurn = beakTurn;
        FireMode = fireMode;
    }

    public static MoveInfo Empty { get; } = new(MoveDirection.None, BeakTurn.None, FireMode.None);

    public MoveDirection MoveDirection { get; }

    public BeakTurn BeakTurn { get; }

    public FireMode FireMode { get; }

    public override string ToString() => $"{GetType().Name}: BeakTurn = {{{BeakTurn}}}, MoveDirection = {{{MoveDirection}}}, FireMode = {FireMode}";
}