using System;
using System.Drawing;
using System.Linq;
using Omnifactotum.Annotations;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak;

public static class UnitPositioningHelper
{
    private static readonly ThreadSafeRandom DefaultRandomGenerator = new();

    public static void PositionRandomly(
        [NotNull] GamePositionEventArgs eventArgs,
        [CanBeNull] Random randomGenerator)
    {
        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        var actualRandomGenerator = randomGenerator ?? DefaultRandomGenerator;

        for (var index = 0; index < eventArgs.UnitStates.Count; index++)
        {
            var unitState = eventArgs.UnitStates[index];

            Point2D position;
            do
            {
                var nominalPosition = new Point(
                    actualRandomGenerator.Next(eventArgs.Data.NominalSize.Width),
                    actualRandomGenerator.Next(eventArgs.Data.NominalSize.Height));
                position = GameHelper.NominalToReal(nominalPosition);
            }
            while (eventArgs.UnitStates.Take(index).Any(
                       item =>
                           eventArgs.GetPosition(item).Position.GetDistance(position) < GameConstants.NominalCellSize));

            var plainAngle = (float)Math.Floor(
                MathHelper.HalfRevolutionDegrees
                - actualRandomGenerator.NextDouble() * MathHelper.RevolutionDegrees);
            var angle = GameAngle.FromDegrees(plainAngle);

            eventArgs.SetPosition(unitState, new DirectionalPosition(position, angle));
        }
    }

    public static void PositionRandomly([NotNull] GamePositionEventArgs eventArgs) => PositionRandomly(eventArgs, null);

    public static void PositionForLineFight([NotNull] GamePositionEventArgs eventArgs)
    {
        if (eventArgs is null)
        {
            throw new ArgumentNullException(nameof(eventArgs));
        }

        var nominalSize = eventArgs.Data.NominalSize;
        var isWiderThanHigher = nominalSize.Width >= nominalSize.Height;

        var lineStart = new Point(0, 0);

        Size lineDirection;
        int lineSize;
        Size rankDirection;
        if (isWiderThanHigher)
        {
            lineDirection = new Size(1, 0);
            lineSize = nominalSize.Width;
            rankDirection = new Size(0, 1);
        }
        else
        {
            lineDirection = new Size(0, 1);
            lineSize = nominalSize.Height;
            rankDirection = new Size(1, 0);
        }

        PositionForLineFightInternal(eventArgs, GameTeam.Light, lineStart, lineDirection, lineSize, rankDirection);

        PositionForLineFightInternal(
            eventArgs,
            GameTeam.Dark,
            isWiderThanHigher
                ? lineStart with { Y = nominalSize.Height - lineStart.Y - 1 }
                : lineStart with { X = nominalSize.Width - lineStart.X - 1 },
            lineDirection,
            lineSize,
            rankDirection.Negate());
    }

    private static void PositionForLineFightInternal(
        [NotNull] GamePositionEventArgs eventArgs,
        GameTeam team,
        Point lineStart,
        Size lineDirection,
        int lineSize,
        Size rankDirection)
    {
        if (team == GameTeam.None)
        {
            throw new ArgumentException("Invalid value.", nameof(team));
        }

        if (lineSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineSize), lineSize, "The value must be positive.");
        }

        var rankStart = lineStart;
        var beakAngle = Vector2D.UnitX.GetAngle(new Vector2D(rankDirection));

        var remainingUnitStates = eventArgs.UnitStates.Where(obj => obj.Team == team).ToArray();
        while (remainingUnitStates.Length > 0)
        {
            var count = Math.Min(remainingUnitStates.Length, lineSize);
            var unitStates = remainingUnitStates.Take(count).ToArray();
            remainingUnitStates = remainingUnitStates.Skip(count).ToArray();

            var step = (float)lineSize / unitStates.Length;

            for (var index = 0; index < unitStates.Length; index++)
            {
                var unitState = unitStates[index];
                var position = rankStart + lineDirection.MultiplyTruncated(step * index);
                var realPosition = GameHelper.NominalToReal(position);
                eventArgs.SetPosition(unitState, new DirectionalPosition(realPosition, beakAngle));
            }

            rankStart += rankDirection;
        }
    }
}