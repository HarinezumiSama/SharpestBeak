﻿using System;
using System.Collections.Generic;
using System.Linq;
using Omnifactotum;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;
using SharpestBeak.View;

namespace SharpestBeak.Logic.Default;

//// TODO: [vmcl] Implement and use logic settings instead of inheritance
//// [vmcl] For instance, (a) each logic may have virtual Setup(ChickenUnitLogicSettings) method; and
//// [vmcl] (b) each logic may be marked with a custom attribute specifying which settings class to use.
public abstract class SmartChickenLogicBase : ChickenUnitLogic
{
    private readonly Features _features;
    private readonly Dictionary<GameObjectId, HashSet<MoveDirection>> _blockedDirectionMap = new();

    private float _forceFireDistance;
    private float _forceFireDistanceSquared;
    private float _tooCloseBorderDistance;
    private float _rotateOnlyBorderDistance;
    private float _boardDiagonalSize;
    private float _tooCloseShot;
    private float _dangerousRadius;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SmartChickenLogicBase"/> class.
    /// </summary>
    protected SmartChickenLogicBase(Features features) => _features = features;

    [Flags]
    protected enum Features
    {
        Default = 0,
        PredictiveShot = 0x00000001,
        TurningAway = 0x00000002
    }

    protected override void OnReset(GameState gameState)
    {
        _blockedDirectionMap.Clear();

        _forceFireDistance = 2f * GameConstants.NominalCellSize;
        _forceFireDistanceSquared = _forceFireDistance.Sqr();
        _tooCloseBorderDistance = 0.5f * GameConstants.NominalCellSize;
        _rotateOnlyBorderDistance = GameConstants.NominalCellSize;
        _boardDiagonalSize = new Vector2D(gameState.Data.RealSize).GetLength();
        _tooCloseShot = 2f * GameConstants.ShotToChickenRectilinearSpeedRatio * GameConstants.NominalCellSize;
        _dangerousRadius = 3f * GameConstants.ChickenUnit.BeakOffset;
    }

    protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
    {
        var allShots = gameState
            .UnitStates
            .SelectMany(item => item.View.Shots)
            .Distinct(KeyedEqualityComparer.For<ShotViewData>.Create(state => state.UniqueId))
            .ToArray();

        var enemyUnits = gameState
            .UnitStates
            .SelectMany(item => item.View.Chickens.Where(visibleUnit => visibleUnit.Team != Team))
            .Distinct(KeyedEqualityComparer.For<ChickenViewData>.Create(data => data.UniqueId))
            .ToArray();

        foreach (var unitState in gameState.UnitStates)
        {
            if (unitState.IsDead)
            {
                continue;
            }

            MoveDirection safestMoveDirection = null;
            if (_features.IsAnySet(Features.TurningAway))
            {
                safestMoveDirection = FindSafestMove(allShots, unitState);
            }

            if (unitState.PreviousMoveState.IsRejected())
            {
                var blockedDirections = _blockedDirectionMap.GetValueOrDefault(unitState.UniqueId);
                if (blockedDirections is null)
                {
                    blockedDirections = [];
                    _blockedDirectionMap[unitState.UniqueId] = blockedDirections;
                }

                blockedDirections.Add(unitState.PreviousMove.MoveDirection);

                var moveDirection = GameHelper
                    .BasicActiveMoveDirections
                    .FirstOrDefault(item => !blockedDirections.Contains(item));
                var unblockingMove = moveDirection is null || moveDirection.IsNone
                    ? new MoveInfo(MoveDirection.None, BeakTurn.FullyClockwise, FireMode.None)
                    : new MoveInfo(moveDirection, BeakTurn.None, FireMode.None);
                moves.Set(unitState, unblockingMove);
                continue;
            }

            _blockedDirectionMap.Remove(unitState.UniqueId);

            var possibleActions = new List<PossibleActionInfo>(unitState.View.Chickens.Count);

            //// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var enemyUnit in enemyUnits)
            {
                var shotTargetPosition = GetShotTargetPosition(unitState, enemyUnit);

                var moveDirection = safestMoveDirection
                    ?? GameHelper.GetBestMoveDirection(
                        unitState.Position,
                        unitState.BeakAngle,
                        shotTargetPosition);

                var turn = GameHelper.GetBestBeakTurn(
                    unitState.Position,
                    unitState.BeakAngle,
                    shotTargetPosition);

                possibleActions.Add(
                    new PossibleActionInfo
                    {
                        EnemyUnit = enemyUnit,
                        MoveDirection = moveDirection,
                        BeakTurn = turn,
                        DistanceSquared = unitState.Position.GetDistanceSquared(enemyUnit.Position)
                    });
            }

            var bestAction = possibleActions
                .OrderBy(item => item.DistanceSquared)
                .ThenBy(item => item.BeakTurn)
                .FirstOrDefault();
            if (bestAction is null)
            {
                var minBorderDistance = gameState.Data.BoardBorder.Edges.Min(
                    item => GameHelper.GetDistanceToLine(unitState.Position, item.Start, item.End));
                if (minBorderDistance <= _tooCloseBorderDistance)
                {
                    var boardCenter = new Point2D(gameState.Data.RealSize) / 2f;
                    var direction = GameHelper.GetBestMoveDirection(
                        unitState.Position,
                        unitState.BeakAngle,
                        boardCenter);

                    moves.Set(unitState, new MoveInfo(direction, BeakTurn.None, FireMode.None));
                    continue;
                }

                if (safestMoveDirection != null)
                {
                    moves.Set(unitState, new MoveInfo(safestMoveDirection, BeakTurn.None, FireMode.None));
                }
                else
                {
                    var seekingMove = new MoveInfo(
                        unitState.PreviousMoveState.IsRejected() || minBorderDistance <= _rotateOnlyBorderDistance
                            ? MoveDirection.None
                            : MoveDirection.MoveForward,
                        BeakTurn.FullyClockwise,
                        FireMode.None);
                    moves.Set(unitState, seekingMove);
                }

                continue;
            }

            var beakTurn = bestAction.BeakTurn.NormalizeBeakTurn();
            var doFire = bestAction.BeakTurn.IsInRange(BeakTurn.ValueRange)
                || bestAction.DistanceSquared <= _forceFireDistanceSquared;

            var move = new MoveInfo(
                bestAction.MoveDirection,
                beakTurn,
                doFire ? FireMode.Regular : FireMode.None);
            moves.Set(unitState, move);
        }
    }

    private MoveDirection FindSafestMove(IEnumerable<ShotViewData> allShots, ChickenUnitState unitState)
    {
        var safetyCircle = new CirclePrimitive(unitState.Position, _dangerousRadius);
        var dangerousShots = new List<ShotViewData>(unitState.View.Shots.Count);

        //// ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var shot in allShots)
        {
            if (unitState.Position.GetDistance(shot.Position) > _tooCloseShot)
            {
                continue;
            }

            var shotDirection = shot.Angle.ToUnitVector();
            var shotToUnitVector = unitState.Position - shot.Position;
            var angle = shotDirection.GetAngle(shotToUnitVector);
            if (angle.DegreeValue.Abs() >= MathHelper.QuarterRevolutionDegrees)
            {
                continue;
            }

            var shotLine = new LinePrimitive(
                shot.Position,
                shot.Position + shot.Angle.ToUnitVector() * _boardDiagonalSize);
            if (CollisionDetector.CheckCollision(shotLine, safetyCircle))
            {
                dangerousShots.Add(shot);
            }
        }

        if (dangerousShots.Count <= 0)
        {
            return null;
        }

        var safeMoves = GameHelper.BasicMoveDirections.ToDictionary(item => item, _ => 0);
        foreach (var dangerousShot in dangerousShots)
        {
            var shotVector = dangerousShot.Angle.ToUnitVector();

            var maxDistanceMove = MoveDirection.None;
            var maxDistanceSqr = float.MinValue;
            foreach (var moveDirection in GameHelper.BasicMoveDirections)
            {
                var potentialPosition = GameHelper.GetNewPosition(
                    unitState.Position,
                    unitState.BeakAngle,
                    moveDirection,
                    GameConstants.ChickenUnit.DefaultRectilinearSpeed);

                var shotToUnitVector = potentialPosition - dangerousShot.Position;
                var shotToUnitVectorLengthSquared = shotToUnitVector.GetLengthSquared();
                var angleCosine = shotVector.GetAngleCosine(shotToUnitVector);
                var distanceSqr = shotToUnitVectorLengthSquared - shotToUnitVectorLengthSquared * angleCosine.Sqr();
                if (distanceSqr > maxDistanceSqr)
                {
                    maxDistanceSqr = distanceSqr;
                    maxDistanceMove = moveDirection;
                }
            }

            safeMoves[maxDistanceMove]++;
        }

        var actuallySafeMovePair = safeMoves
            .Where(pair => pair.Value > 0)
            .OrderByDescending(pair => pair.Value)
            .FirstOrDefault();

        return actuallySafeMovePair.Value > 0 ? actuallySafeMovePair.Key : null;
    }

    private Point2D GetShotTargetPosition(
        ChickenUnitState unitState,
        ChickenViewData enemyUnit)
    {
        if (!_features.IsAnySet(Features.PredictiveShot))
        {
            return enemyUnit.Position;
        }

        Point2D? predictiveShotTargetPosition = null;

        var enemyUnitBeakTip = GameHelper.GetBeakTipPosition(enemyUnit.Position, enemyUnit.BeakAngle);
        var enemyUnitVector = enemyUnitBeakTip - enemyUnit.Position;
        var enemyToMeVector = unitState.Position - enemyUnit.Position;
        var cosAlpha = enemyUnitVector.GetAngleCosine(enemyToMeVector);
        var distanceToEnemySqr = enemyToMeVector.GetLengthSquared();
        var distanceToEnemy = distanceToEnemySqr.Sqrt();
        if (!(cosAlpha.Abs() - 1f).IsZero())
        {
            var equation = new QuadraticEquation(
                GameConstants.ShotToChickenRectilinearSpeedRatio.Sqr() - 1f,
                2f * distanceToEnemy * cosAlpha,
                -distanceToEnemySqr);

            if (equation.GetRoots(out var d1, out var d2) > 0)
            {
                var d = new[] { d1, d2 }
                    .Where(item => item > 0f)
                    .OrderBy(item => item)
                    .FirstOrDefault();
                if (d.IsPositive())
                {
                    predictiveShotTargetPosition = GameHelper.GetNewPosition(
                        enemyUnit.Position,
                        enemyUnit.BeakAngle,
                        d);
                }
            }
        }

        var result = predictiveShotTargetPosition
            ?? GameHelper.GetNewPosition(
                enemyUnit.Position,
                enemyUnit.BeakAngle,
                GameConstants.ChickenUnit.DefaultRectilinearSpeed);
        return result;
    }

    private sealed class PossibleActionInfo
    {
        public ChickenViewData EnemyUnit
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            get;
            set;
        }

        public MoveDirection MoveDirection
        {
            get;
            set;
        }

        public float BeakTurn
        {
            get;
            set;
        }

        public float DistanceSquared
        {
            get;
            set;
        }
    }
}