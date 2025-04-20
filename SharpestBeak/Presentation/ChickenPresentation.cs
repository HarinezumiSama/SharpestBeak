using System;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation;

public sealed class ChickenPresentation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChickenPresentation"/> class.
    /// </summary>
    internal ChickenPresentation(GamePresentation gamePresentation, ChickenUnit chickenUnit)
    {
        if (chickenUnit is null)
        {
            throw new ArgumentNullException(nameof(chickenUnit));
        }

        GamePresentation = gamePresentation ?? throw new ArgumentNullException(nameof(gamePresentation));
        UniqueId = chickenUnit.UniqueId;
        Team = chickenUnit.Team;
        KillCount = chickenUnit.KillCount;

        InitialPosition = chickenUnit.Position;
        Movement = chickenUnit.Movement;

        InitialBeakAngle = chickenUnit.BeakAngle;
        BeakMovement = chickenUnit.BeakMovement;
    }

    public GamePresentation GamePresentation { get; }

    public GameObjectId UniqueId { get; }

    public GameTeam Team { get;  }

    public int KillCount { get;  }

    internal Point2D InitialPosition { get; }

    internal Vector2D Movement { get; }

    internal GameAngle InitialBeakAngle { get; }

    internal GameAngle BeakMovement { get; }

    public DirectionalPosition GetCurrentPosition()
    {
        var ratio = GetCurrentRatio();

        var currentMovement = Movement * ratio;
        var currentBeakMovement = BeakMovement * ratio;

        var position = InitialPosition + currentMovement;
        var angle = InitialBeakAngle + currentBeakMovement;

        return new DirectionalPosition(position, angle);
    }

    private float GetCurrentRatio() => GamePresentation.StepStopwatch.StepRatio;
}