using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameBoard
    {
        #region Fields

        private static readonly Random s_random = new Random();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameBoard"/> class.
        /// </summary>
        public GameBoard(Size size, IEnumerable<Type> chickenLogicTypes)
        {
            #region Argument Check

            if (size.Width < Constants.MinNominalCellCount || size.Height < Constants.MinNominalCellCount)
            {
                throw new ArgumentException(
                    string.Format("The size dimension must be at least {0}.", Constants.MinNominalCellCount),
                    "size");
            }
            if (chickenLogicTypes == null)
            {
                throw new ArgumentNullException("chickenLogicTypes");
            }
            if (!chickenLogicTypes.Any())
            {
                throw new ArgumentException("At least one chicken must be specified.", "chickenLogicTypes");
            }
            if (chickenLogicTypes.Any(item => item == null))
            {
                throw new ArgumentException("The collection contains a null element.", "chickenLogicTypes");
            }
            if (chickenLogicTypes.Any(item => !typeof(ChickenUnitLogic).IsAssignableFrom(item)))
            {
                throw new ArgumentException("Invalid chicken logic type.", "chickenLogicTypes");
            }

            #endregion

            // Pre-initialized properties
            this.Size = size;

            // Post-initialized properties
            this.AllChickens = chickenLogicTypes
                .Select((item, index) => CreateChicken(item, index))
                .ToList()
                .AsReadOnly();
            this.AliveChickensDirect = new List<ChickenUnit>(this.AllChickens);
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();

            PositionChickens();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameBoard"/> class.
        /// </summary>
        public GameBoard(Size size, params Type[] chickenTypes)
            : this(size, (IEnumerable<Type>)chickenTypes)
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private ChickenUnit CreateChicken(Type item, int index)
        {
            var logic = (ChickenUnitLogic)Activator.CreateInstance(item);
            logic.Board = this;

            var result = new ChickenUnit(logic);
            result.UniqueIndex = index + 1;

            return result;
        }

        private void PositionChickens()
        {
            throw new NotImplementedException();

            //if (this.AllChickens.Count > this.Size.Width * this.Size.Height / 2)
            //{
            //    throw new InvalidOperationException(
            //        string.Format(
            //            "Too many chickens ({0}) for the board of size {1}x{2}.",
            //            this.AllChickens.Count,
            //            this.Size.Width,
            //            this.Size.Height));
            //}

            //for (int index = 0; index < this.AllChickens.Count; index++)
            //{
            //    var chicken = this.AllChickens[index];

            //    Point newPosition;
            //    do
            //    {
            //        newPosition = new Point(s_random.Next(this.Size.Width), s_random.Next(this.Size.Height));
            //    }
            //    while (this.AllChickens.Take(index).Any(item => item.Position == newPosition));

            //    chicken.Position = newPosition;
            //    chicken.BeakAngle = s_beakAngles[s_random.Next(s_beakAngles.Length)];
            //}
        }

        #endregion

        #region Internal Properties

        internal List<ChickenUnit> AliveChickensDirect
        {
            get;
            private set;
        }

        internal IList<ChickenUnit> AllChickens
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public IList<ChickenUnit> AliveChickens
        {
            get;
            private set;
        }

        public Size Size
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public float GetNewBeakAngle(float oldBeakAngle, BeakTurn beakTurn, float timeDelta)
        {
            int beakTurnOffset = (int)beakTurn;

            #region Argument Check

            if (oldBeakAngle < 0)
            {
                throw new ArgumentOutOfRangeException("oldBeakAngle", oldBeakAngle, "Beak angle cannot be negative.");
            }
            if (Math.Abs(beakTurnOffset) > 1)
            {
                throw new ArgumentOutOfRangeException("beakTurn", beakTurn, "Invalid beak turn.");
            }
            if (timeDelta < 0)
            {
                throw new ArgumentOutOfRangeException("timeDelta", timeDelta, "Time delta cannot be negative.");
            }

            #endregion

            var result = (oldBeakAngle + timeDelta * Constants.NominalBeakAngleSpeed * beakTurnOffset) / 360f;
            if (result < 0f)
            {
                result += Constants.FullRotationAngle;
            }
            else if (result >= Constants.FullRotationAngle)
            {
                result -= Constants.FullRotationAngle;
            }
            return result;
        }

        #endregion
    }
}