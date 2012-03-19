using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Primitives;
using SharpestBeak.View;

namespace SharpestBeak.Logic.Default
{
    // TODO: [VM] Implement and use logic settings instead of inheritance
    // [VM] For instance, (a) each logic may have virtual Setup(ChickenUnitLogicSettings) method; and
    // [VM] (b) each logic may be marked with a custom attribute specifying the the settings class to use.

    public abstract class SmartChickenLogicBase : ChickenUnitLogic
    {
        #region Nested Types

        #region Features Enumeration

        [Flags]
        protected enum Features
        {
            Default = 0,
            PredictiveShot = 0x00000001,
            TurningAway = 0x00000002
        }

        #endregion

        #region PossibleActionInfo Class

        private sealed class PossibleActionInfo
        {
            #region Public Properties

            public ChickenViewData EnemyUnit
            {
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

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Features m_features;

        private float m_forceFireDistance;
        private float m_forceFireDistanceSquared;
        private float m_tooCloseBorderDistance;
        private float m_rotateOnlyBorderDistance;
        private float m_boardDiagonalSize;
        private float m_tooCloseShot;
        private float m_dangerousRadius;

        private readonly Dictionary<GameObjectId, HashSet<MoveDirection>> m_blockedDirectionMap =
            new Dictionary<GameObjectId, HashSet<MoveDirection>>();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmartChickenLogicBase"/> class.
        /// </summary>
        protected SmartChickenLogicBase(Features features)
            : base()
        {
            m_features = features;
        }

        #endregion

        #region Private Methods

        private MoveDirection? FindSafestMove(List<ShotViewData> allShots, ChickenUnitState unitState)
        {
            var safetyCircle = new CirclePrimitive(unitState.Position, m_dangerousRadius);
            var dangerousShots = new List<ShotViewData>(unitState.View.Shots.Count);
            foreach (var shot in allShots)
            {
                if (unitState.Position.GetDistance(shot.Position) > m_tooCloseShot)
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
                    shot.Position + shot.Angle.ToUnitVector() * m_boardDiagonalSize);
                if (CollisionDetector.CheckCollision(shotLine, safetyCircle))
                {
                    dangerousShots.Add(shot);
                }
            }

            if (dangerousShots.Count <= 0)
            {
                return null;
            }

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
                return actuallySafeMovePair.Key;
            }

            return null;
        }

        private Point2D GetShotTargetPosition(
            ChickenUnitState unitState,
            ChickenViewData enemyUnit)
        {
            if (!m_features.IsAnySet(Features.PredictiveShot))
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
            if ((cosAlpha.Abs() - 1f).IsNotZero())
            {
                var equation = new QuadraticEquation(
                    GameConstants.ShotToChickenRectilinearSpeedRatio.Sqr() - 1f,
                    2f * distanceToEnemy * cosAlpha,
                    -distanceToEnemySqr);

                float d1, d2;
                if (equation.GetRoots(out d1, out d2) > 0)
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
                    GameConstants.ChickenUnit.DefaultRectilinearStepDistance);
            return result;
        }

        #endregion

        #region Protected Methods

        protected override void OnReset(GameState gameState)
        {
            m_blockedDirectionMap.Clear();

            m_forceFireDistance = 2f * GameConstants.NominalCellSize;
            m_forceFireDistanceSquared = m_forceFireDistance.Sqr();
            m_tooCloseBorderDistance = 0.5f * GameConstants.NominalCellSize;
            m_rotateOnlyBorderDistance = GameConstants.NominalCellSize;
            m_boardDiagonalSize = new Vector2D(gameState.Data.RealSize).GetLength();
            m_tooCloseShot = 2f * GameConstants.ShotToChickenRectilinearSpeedRatio * GameConstants.NominalCellSize;
            m_dangerousRadius = 3f * GameConstants.ChickenUnit.BeakOffset;
        }

        protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
        {
            var allShots = gameState.UnitStates.SelectMany(item => item.View.Shots).ToList();
            var enemyUnits = gameState
                .UnitStates
                .SelectMany(item => item.View.Chickens.Where(visibleUnit => visibleUnit.Team != this.Team))
                .ToList();

            foreach (var unitState in gameState.UnitStates)
            {
                if (unitState.IsDead)
                {
                    continue;
                }

                var safestMoveDirection = m_features.IsAnySet(Features.TurningAway)
                    ? FindSafestMove(allShots, unitState)
                    : null;

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

                var possibleActions = new List<PossibleActionInfo>(unitState.View.Chickens.Count);
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
                if (bestAction == null)
                {
                    var minBorderDistance = gameState.Data.BoardBorder.Edges.Min(
                        item => GameHelper.GetDistanceToLine(unitState.Position, item.Start, item.End));
                    if (minBorderDistance <= m_tooCloseBorderDistance)
                    {
                        var boardCenter = new Point2D(gameState.Data.RealSize) / 2f;
                        var direction = GameHelper.GetBestMoveDirection(
                            unitState.Position,
                            unitState.BeakAngle,
                            boardCenter);

                        moves.Set(unitState, new MoveInfo(direction, BeakTurn.None, FireMode.None));
                        continue;
                    }

                    if (safestMoveDirection.HasValue)
                    {
                        moves.Set(unitState, new MoveInfo(safestMoveDirection.Value, BeakTurn.None, FireMode.None));
                    }
                    else
                    {
                        var seekingMove = new MoveInfo(
                            unitState.PreviousMoveState.IsRejected() || minBorderDistance <= m_rotateOnlyBorderDistance
                                ? MoveDirection.None
                                : MoveDirection.MoveForward,
                            BeakTurn.FullyClockwise,
                            FireMode.None);
                        moves.Set(unitState, seekingMove);
                    }

                    continue;
                }

                var beakTurn = GameHelper.NormalizeBeakTurn(bestAction.BeakTurn);
                var doFire = bestAction.BeakTurn.IsInRange(BeakTurn.ValueRange)
                    || bestAction.DistanceSquared <= m_forceFireDistanceSquared;

                var move = new MoveInfo(
                    bestAction.MoveDirection,
                    beakTurn,
                    doFire ? FireMode.Regular : FireMode.None);
                moves.Set(unitState, move);
            }
        }

        protected override string GetCaption()
        {
            var selectedFeatures = Helper
                .GetEnumValues<Features>()
                .Where(item => item != 0 && m_features.IsAllSet(item))
                .Select(item => item.ToString());
            var featuresString = string.Join(", ", selectedFeatures);
            if (string.IsNullOrEmpty(featuresString))
            {
                featuresString = Features.Default.ToString();
            }

            return string.Format("Smart [{0}]", featuresString);
        }

        #endregion
    }
}