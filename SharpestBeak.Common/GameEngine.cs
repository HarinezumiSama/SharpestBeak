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
        public GameEngine(Size size, IEnumerable<Type> chickenTypes)
        {
            this.Board = new GameBoard(size, chickenTypes);
            this.IsNextTurn = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEngine"/> class.
        /// </summary>
        public GameEngine(Size size, params Type[] chickenTypes)
            : this(size, (IEnumerable<Type>)chickenTypes)
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private void FinishGame()
        {
            this.IsGameFinished = true;
        }

        private void OnDiscreteMoveOccurred()
        {
            var discreteMoveOccurred = this.DiscreteMoveOccurred;
            if (discreteMoveOccurred != null)
            {
                discreteMoveOccurred(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Public Events

        public event EventHandler DiscreteMoveOccurred;

        #endregion

        #region Public Properties

        public GameBoard Board
        {
            get;
            private set;
        }

        public bool IsNextTurn
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

        public long TurnIndex
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

            if (this.IsNextTurn)
            {
                foreach (var aliveChicken in this.Board.AliveChickens)
                {
                    aliveChicken.CurrentMove = null;
                    aliveChicken.PeckedBy = null;
                }
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
                    OnDiscreteMoveOccurred();
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
                                if (target != null && target.PeckedBy == null)
                                {
                                    // If both A and B pecked C, first one has priority
                                    target.PeckedBy = chicken;
                                }
                            }
                            OnDiscreteMoveOccurred();
                        }
                        break;
                    default:
                        throw new NotImplementedException("Invalid chicken move.");
                }

                if (newPosition != chicken.Position && this.Board.IsValidMove(newPosition))
                {
                    chicken.Position = newPosition;
                    OnDiscreteMoveOccurred();
                }
            }

            this.PlayerIndex++;
            this.IsNextTurn = this.PlayerIndex >= this.Board.AliveChickens.Count;

            if (this.IsNextTurn)
            {
                this.PlayerIndex = 0;

                var peckedChickens = this.Board.AliveChickens.Where(item => item.PeckedBy != null).ToList();
                foreach (var peckedChicken in peckedChickens)
                {
                    peckedChicken.IsDead = true;
                    peckedChicken.PeckedBy.KillCount++;
                    this.Board.AliveChickensDirect.Remove(peckedChicken);
                }
                OnDiscreteMoveOccurred();

                this.TurnIndex++;
            }

            if (this.Board.AliveChickens.Count <= 1)
            {
                FinishGame();
            }
        }

        #endregion
    }
}