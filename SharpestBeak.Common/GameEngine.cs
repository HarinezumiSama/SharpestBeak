using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameEngine
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine()
        {
            // TODO: Obtain chickens and size
            this.Board = new GameBoard(Enumerable.Empty<Type>(), new Size(20, 20));
            this.IsTurnFinished = true;
        }

        #endregion

        #region Private Methods

        private void FinishGame()
        {
            this.IsGameFinished = true;
        }

        #endregion

        #region Public Properties

        public GameBoard Board
        {
            get;
            private set;
        }

        public bool IsTurnFinished
        {
            get;
            private set;
        }

        public int PlayerIndex
        {
            get;
            private set;
        }

        public bool IsGameFinished
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public void MakePrimitiveMove()
        {
            if (this.IsGameFinished)
            {
                return;
            }

            var chicken = this.Board.AliveChickens[this.PlayerIndex];
            var newMove = chicken.MakeMove();
            if (newMove != null)
            {
                // Action 1 - Beak turn
                var beakTurnOffset = (int)newMove.Beak;
                if (beakTurnOffset != 0)
                {
                    if (Math.Abs(beakTurnOffset) > 1)
                    {
                        throw new InvalidOperationException("Invalid beak turn. Seems that someone tried to cheat!");
                    }
                    var newBeakAngle = chicken.BeakAngle + beakTurnOffset;
                    if (newBeakAngle < BeakAngle.Min)
                    {
                        newBeakAngle = BeakAngle.Max;
                    }
                    else if (newBeakAngle > BeakAngle.Max)
                    {
                        newBeakAngle = BeakAngle.Min;
                    }
                    chicken.BeakAngle = newBeakAngle;
                }

                // Action 2 - Move or peck
                var newPosition = chicken.Position;
                switch (newMove.Move)
                {
                    case MoveAction.None:
                        // Nothing to do
                        break;
                    case MoveAction.MoveUp:
                        newPosition.Y--;
                        break;
                    case MoveAction.MoveRight:
                        newPosition.X++;
                        break;
                    case MoveAction.MoveDown:
                        newPosition.Y++;
                        break;
                    case MoveAction.MoveLeft:
                        newPosition.X--;
                        break;
                    case MoveAction.Peck:
                        {
                            var attackPoint = this.Board.GetPeckAttackPoint(chicken);
                            if (attackPoint.HasValue)
                            {
                                var target = this.Board.GetChickenAtPoint(attackPoint.Value);
                                if (target != null)
                                {
                                    target.IsPecked = true;
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException("Invalid chicken move.");
                }

                if (newPosition != chicken.Position && this.Board.IsValidMove(newPosition))
                {
                    chicken.Position = newPosition;
                }
            }

            this.PlayerIndex++;
            this.IsTurnFinished = this.PlayerIndex >= this.Board.AliveChickens.Count;

            if (this.IsTurnFinished)
            {
                this.PlayerIndex = 0;

                var peckedChickens = this.Board.AliveChickens.Where(item => item.IsPecked).ToList();
                foreach (var peckedChicken in peckedChickens)
                {
                    peckedChicken.IsDead = true;
                    this.Board.AliveChickensDirect.Remove(peckedChicken);
                }
            }

            if (this.Board.AliveChickens.Count <= 1)
            {
                FinishGame();
            }
        }

        #endregion
    }
}