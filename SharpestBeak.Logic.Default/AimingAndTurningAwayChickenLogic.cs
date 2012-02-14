using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Common;
using SharpestBeak.Common.Presentation.Primitives;
using SharpestBeak.Common.View;

namespace SharpestBeak.Logic.Default
{
    public sealed class AimingAndTurningAwayChickenLogic : ChickenUnitLogic
    {
        #region Fields

        private readonly Dictionary<int, HashSet<MoveDirection>> m_blockedDirectionMap =
            new Dictionary<int, HashSet<MoveDirection>>();

        #endregion

        #region Protected Methods

        protected override void OnReset(GameState gameState)
        {
            // Nothing to do
        }

        protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
        {
            var aimingShotMaxDistance = 10f * GameConstants.NominalCellSize;
            var tooCloseDistance = 2f * GameConstants.NominalCellSize;
            var tooCloseBorderDistance = 0.5f * GameConstants.NominalCellSize;
            var rotateOnlyBorderDistance = GameConstants.NominalCellSize;
            var boardDiagonalSize = new Vector2D(gameState.Data.RealSize).GetLength();

            var tooCloseShot = 2f * GameConstants.ShotToChickenRectilinearSpeedRatio * GameConstants.NominalCellSize;
            var dangerousRadius = 1.5f * GameConstants.ChickenUnit.BeakOffset;

            var allShots = gameState.UnitStates.SelectMany(item => item.View.Shots).ToList();

            foreach (var unitState in gameState.UnitStates)
            {
                if (unitState.IsDead)
                {
                    continue;
                }

                var safetyCircle = new CirclePrimitive(unitState.Position, dangerousRadius);
                var dangerousShots = new List<ShotViewData>(unitState.View.Shots.Count);
                foreach (var shot in allShots)
                {
                    if (unitState.Position.GetDistance(shot.Position) > tooCloseShot)
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
                        shot.Position + shot.Angle.ToUnitVector() * boardDiagonalSize);
                    if (CollisionDetector.CheckCollision(shotLine, safetyCircle))
                    {
                        dangerousShots.Add(shot);
                    }
                }

                MoveDirection? safestMove = null;
                if (dangerousShots.Count > 0)
                {
                    var safeMoves = GameHelper.MoveDirections.ToDictionary(item => item, item => 0);
                    foreach (var dangerousShot in dangerousShots)
                    {
                        var shotVector = dangerousShot.Angle.ToUnitVector();

                        var maxDistanceMove = MoveDirection.None;
                        var maxDistanceSqr = float.MinValue;
                        foreach (var moveDirection in GameHelper.MoveDirections)
                        {
                            var potentialPosition = GameHelper.GetNewPosition(
                               unitState.Position,
                               unitState.BeakAngle,
                               moveDirection,
                               GameConstants.ChickenUnit.DefaultRectilinearStepDistance);
                            var shotToUnitVector = potentialPosition - dangerousShot.Position;
                            var shotToUnitVectorLengthSquared = shotToUnitVector.GetLengthSquared();
                            var angleCosine = shotVector.GetAngleCosine(shotToUnitVector);
                            var distanceSqr = shotToUnitVectorLengthSquared
                                - shotToUnitVectorLengthSquared * angleCosine.Sqr();
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
                    if (actuallySafeMovePair.Value > 0)
                    {
                        safestMove = actuallySafeMovePair.Key;
                    }
                }

                if (unitState.PreviousMoveState.IsRejected())
                {
                    var blockedDirections = m_blockedDirectionMap.GetValueOrDefault(unitState.UniqueId);
                    if (blockedDirections == null)
                    {
                        blockedDirections = new HashSet<MoveDirection>();
                        m_blockedDirectionMap[unitState.UniqueId] = blockedDirections;
                    }
                    blockedDirections.Add(unitState.PreviousMove.MoveDirection);

                    var moveDirection = GameHelper
                        .ActiveMoveDirections
                        .FirstOrDefault(item => !blockedDirections.Contains(item));
                    var unblockingMove = moveDirection == MoveDirection.None
                        ? new MoveInfo(MoveDirection.None, BeakTurn.FullyClockwise, FireMode.None)
                        : new MoveInfo(moveDirection, BeakTurn.None, FireMode.None);
                    moves.Set(unitState, unblockingMove);
                    continue;
                }
                m_blockedDirectionMap.Remove(unitState.UniqueId);

                var list = new List<Tuple<ChickenViewData, MoveDirection, float, float>>();
                var enemyUnits = unitState.View.Chickens.Where(item => item.Team != this.Team);
                foreach (var enemyUnit in enemyUnits)
                {
                    Point2D? predictedEnemyUnitPosition = null;

                    var enemyUnitBeakTip = GameHelper.GetBeakTipPosition(enemyUnit.Position, enemyUnit.BeakAngle);
                    var enemyUnitVector = enemyUnitBeakTip - enemyUnit.Position;
                    var enemyToMeVector = unitState.Position - enemyUnit.Position;
                    var cosAlpha = enemyUnitVector.GetAngleCosine(enemyToMeVector);
                    var distanceToEnemySqr = enemyToMeVector.GetLengthSquared();
                    var distanceToEnemy = distanceToEnemySqr.Sqrt();
                    if ((cosAlpha.Abs() - 1f).IsNotZero() && distanceToEnemy <= aimingShotMaxDistance)
                    {
                        var equation = new QuadraticEquation(
                            GameConstants.ShotToChickenRectilinearSpeedRatio.Sqr() - 1f,
                            2f * distanceToEnemy * cosAlpha,
                            -distanceToEnemySqr);

                        float d1, d2;
                        if (equation.GetRoots(out d1, out d2) > 0)
                        {
                            var d = new[] { d1, d2 }.Where(item => item > 0f).OrderBy(item => item).FirstOrDefault();
                            if (d.IsPositive())
                            {
                                predictedEnemyUnitPosition = GameHelper.GetNewPosition(
                                    enemyUnit.Position,
                                    enemyUnit.BeakAngle,
                                    d);
                            }
                        }
                    }

                    if (!predictedEnemyUnitPosition.HasValue)
                    {
                        predictedEnemyUnitPosition = GameHelper.GetNewPosition(
                            enemyUnit.Position,
                            enemyUnit.BeakAngle,
                            GameConstants.ChickenUnit.DefaultRectilinearStepDistance);
                    }

                    var moveDirection = safestMove
                        ?? GameHelper.GetBestMoveDirection(
                            unitState.Position,
                            unitState.BeakAngle,
                            predictedEnemyUnitPosition.Value);
                    var turn = GameHelper.GetBestBeakTurn(
                        unitState.Position,
                        unitState.BeakAngle,
                        predictedEnemyUnitPosition.Value);

                    list.Add(
                        Tuple.Create(
                            enemyUnit,
                            moveDirection,
                            turn,
                            unitState.Position.GetDistanceSquared(enemyUnit.Position)));
                }

                var best = list
                    .OrderBy(item => item.Item4)
                    .ThenBy(item => item.Item3)
                    .FirstOrDefault();

                if (best == null)
                {
                    var minBorderDistance = gameState.Data.BoardBorder.Edges.Min(
                        item => GameHelper.GetDistanceToLine(unitState.Position, item.Start, item.End));
                    if (minBorderDistance <= tooCloseBorderDistance)
                    {
                        var boardCenter = new Point2D(gameState.Data.RealSize) / 2f;
                        var direction = GameHelper.GetBestMoveDirection(
                            unitState.Position,
                            unitState.BeakAngle,
                            boardCenter);

                        moves.Set(unitState, new MoveInfo(direction, BeakTurn.None, FireMode.None));
                        continue;
                    }

                    var seekingMove = new MoveInfo(
                        unitState.PreviousMoveState.IsRejected() || minBorderDistance <= rotateOnlyBorderDistance
                            ? MoveDirection.None
                            : MoveDirection.MoveForward,
                        BeakTurn.FullyClockwise,
                        FireMode.None);

                    moves.Set(unitState, seekingMove);
                    continue;
                }

                var beakTurn = GameHelper.NormalizeBeakTurn(best.Item3);
                var doFire = best.Item3.IsInRange(BeakTurn.ValueRange)
                    || unitState.Position.GetDistance(best.Item1.Position) <= tooCloseDistance;

                var move = new MoveInfo(best.Item2, beakTurn, doFire ? FireMode.Regular : FireMode.None);
                moves.Set(unitState, move);
            }
        }

        protected override string GetCaption()
        {
            return "Aiming and Turning Away";
        }

        #endregion
    }
}