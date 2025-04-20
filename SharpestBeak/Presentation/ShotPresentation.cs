using System;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation;

public sealed class ShotPresentation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ShotPresentation"/> class.
    /// </summary>
    internal ShotPresentation(GamePresentation gamePresentation, ShotUnit shotUnit)
    {
        if (shotUnit is null)
        {
            throw new ArgumentNullException(nameof(shotUnit));
        }

        GamePresentation = gamePresentation ?? throw new ArgumentNullException(nameof(gamePresentation));
        UniqueId = shotUnit.UniqueId;
        OwnerTeam = shotUnit.Owner.Team;

        InitialPosition = shotUnit.Position;
        Movement = shotUnit.Movement;
    }

    public GamePresentation GamePresentation { get; }

    public GameObjectId UniqueId { get; }

    public GameTeam OwnerTeam { get; }

    internal Point2D InitialPosition { get; }

    internal Vector2D Movement { get; }

    public Point2D GetCurrentPosition()
    {
        var ratio = GetCurrentRatio();
        var currentMovement = Movement * ratio;

        return InitialPosition + currentMovement;
    }

    private float GetCurrentRatio() => GamePresentation.StepStopwatch.StepRatio;
}