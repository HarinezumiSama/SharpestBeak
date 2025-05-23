﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.Logic.Default;

[Description("Random")]
public sealed class RandomChickenLogic : ChickenUnitLogic
{
    private static readonly ThreadSafeRandom RandomGenerator = new();

    private readonly Dictionary<GameObjectId, Point2D> _unitIdToTargetPointMap = new();
    private readonly object _targetPointsLock = new();

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "For debug purposes.")]
    public Point2D[] TargetPoints
    {
        [DebuggerStepThrough]
        get
        {
            lock (_targetPointsLock)
            {
                return _unitIdToTargetPointMap.Values.ToArray();
            }
        }
    }

    protected override void OnReset(GameState gameState)
    {
        lock (_targetPointsLock)
        {
            _unitIdToTargetPointMap.Clear();
            gameState.UnitStates.DoForEach(item => ChooseTargetPoint(gameState, item));
        }
    }

    protected override void OnMakeMove(GameState gameState, LogicMoveResult moves)
    {
        foreach (var unitState in gameState.UnitStates)
        {
            if (unitState.IsDead)
            {
                lock (_targetPointsLock)
                {
                    _unitIdToTargetPointMap.Remove(unitState.UniqueId);
                }

                continue;
            }

            Point2D targetPoint;
            lock (_targetPointsLock)
            {
                targetPoint = _unitIdToTargetPointMap.GetValueOrDefault(unitState.UniqueId, unitState.Position);
            }

            var needNewTargetPoint = unitState.PreviousMoveState.IsRejected()
                || unitState.Position.GetDistance(targetPoint)
                    .IsZero(GameConstants.ChickenUnit.DefaultRectilinearSpeed);

            if (needNewTargetPoint)
            {
                targetPoint = ChooseTargetPoint(gameState, unitState);
            }

            var move = GameHelper.GetBestMoveDirection(unitState.Position, unitState.BeakAngle, targetPoint);
            var turn = GameHelper.GetBestBeakTurnNormalized(unitState.Position, unitState.BeakAngle, targetPoint);
            var fireMode = unitState.CanShoot && RandomGenerator.Next(10) == 0 ? FireMode.Regular : FireMode.None;

            var moveInfo = new MoveInfo(move, turn, fireMode);
            moves.Set(unitState, moveInfo);
        }
    }

    private Point2D ChooseTargetPoint(GameState state, ChickenUnitState unitState)
    {
        var point = new Point(
            RandomGenerator.Next(state.Data.NominalSize.Width),
            RandomGenerator.Next(state.Data.NominalSize.Height));

        var result = GameHelper.NominalToReal(point);
        lock (_targetPointsLock)
        {
            _unitIdToTargetPointMap[unitState.UniqueId] = result;
        }

        return result;
    }
}