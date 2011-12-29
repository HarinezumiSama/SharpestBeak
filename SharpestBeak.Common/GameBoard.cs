using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class GameBoard
    {
        #region Constants

        public const int MinSizeDimension = 2;

        #endregion

        #region Fields

        private static readonly Random s_random = new Random();
        private static readonly BeakAngle[] s_beakAngles = (BeakAngle[])Enum.GetValues(typeof(BeakAngle));
        private static readonly Dictionary<BeakAngle, Size> s_beakAngleToOffsetMap =
            new Dictionary<BeakAngle, Size>()
            {
                { BeakAngle.Up, new Size(0, -1) },
                { BeakAngle.UpRight, new Size(1, -1) },
                { BeakAngle.Right, new Size(1, 0) },
                { BeakAngle.DownRight, new Size(1, 1) },
                { BeakAngle.Down, new Size(0, 1) },
                { BeakAngle.DownLeft, new Size(-1, 1) },
                { BeakAngle.Left, new Size(-1, 0) },
                { BeakAngle.UpLeft, new Size(-1, -1) }
            };

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameBoard"/> class.
        /// </summary>
        public GameBoard(Size size, IEnumerable<Type> chickenLogicTypes)
        {
            #region Argument Check

            if (size.Width < MinSizeDimension || size.Height < MinSizeDimension)
            {
                throw new ArgumentException(
                    string.Format("The size dimension must be at least {0}.", MinSizeDimension),
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
            if (this.AllChickens.Count > this.Size.Width * this.Size.Height / 2)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Too many chickens ({0}) for the board of size {1}x{2}.",
                        this.AllChickens.Count,
                        this.Size.Width,
                        this.Size.Height));
            }

            for (int index = 0; index < this.AllChickens.Count; index++)
            {
                var chicken = this.AllChickens[index];

                Point newPosition;
                do
                {
                    newPosition = new Point(s_random.Next(this.Size.Width), s_random.Next(this.Size.Height));
                }
                while (this.AllChickens.Take(index).Any(item => item.Position == newPosition));

                chicken.Position = newPosition;
                chicken.BeakAngle = s_beakAngles[s_random.Next(s_beakAngles.Length)];
            }
        }

        #endregion

        #region Internal Properties

        internal List<ChickenUnit> AliveChickensDirect
        {
            get;
            private set;
        }

        #endregion

        #region Public Properties

        public IList<ChickenUnit> AllChickens
        {
            get;
            private set;
        }

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

        public bool IsValidPoint(Point value)
        {
            return value.X >= 0 && value.X < this.Size.Width && value.Y >= 0 && value.Y < this.Size.Height;
        }

        public bool IsValidMove(Point value)
        {
            return IsValidPoint(value) && GetChickenAtPoint(value) == null;
        }

        public BeakAngle GetNewBeakAngle(BeakAngle oldBeakAngle, BeakTurn beakTurn)
        {
            int beakTurnOffset = (int)beakTurn;
            if (Math.Abs(beakTurnOffset) > 1)
            {
                throw new ArgumentOutOfRangeException(
                    "beakTurn",
                    beakTurn,
                    "Invalid beak turn. Has someone tried to cheat?");
            }
            var result = oldBeakAngle + beakTurnOffset;
            if (result < BeakAngle.Min)
            {
                result = BeakAngle.Max;
            }
            else if (result > BeakAngle.Max)
            {
                result = BeakAngle.Min;
            }
            return result;
        }

        public Point? GetPeckAttackPoint(ChickenUnit chicken, BeakTurn beakTurn = BeakTurn.None)
        {
            #region Argument Check

            if (chicken == null)
            {
                throw new ArgumentNullException("chicken");
            }

            #endregion

            var newBeakAngle = GetNewBeakAngle(chicken.BeakAngle, beakTurn);
            var offset = s_beakAngleToOffsetMap[newBeakAngle];
            var result = chicken.Position + offset;
            return IsValidPoint(result) ? result : (Point?)null;
        }

        public ChickenUnit GetChickenAtPoint(Point value)
        {
            return IsValidPoint(value) ? this.AliveChickens.SingleOrDefault(item => item.Position == value) : null;
        }

        #endregion
    }
}