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
        private static readonly Dictionary<BeakAngle, Point> s_beakAngleToOffsetMap =
            new Dictionary<BeakAngle, Point>()
            {
                { BeakAngle.Up, new Point(0, -1) },
                { BeakAngle.UpRight, new Point(1, -1) },
                { BeakAngle.Right, new Point(1, 0) },
                { BeakAngle.DownRight, new Point(1, 1) },
                { BeakAngle.Down, new Point(0, 1) },
                { BeakAngle.DownLeft, new Point(-1, 1) },
                { BeakAngle.Left, new Point(-1, 0) },
                { BeakAngle.UpLeft, new Point(-1, -1) }
            };

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameBoard"/> class.
        /// </summary>
        public GameBoard(Size size, IEnumerable<Type> chickenTypes)
        {
            #region Argument Check

            if (size.Width < MinSizeDimension || size.Height < MinSizeDimension)
            {
                throw new ArgumentException(
                    string.Format("The size dimension must be at least {0}.", MinSizeDimension),
                    "size");
            }
            if (chickenTypes == null)
            {
                throw new ArgumentNullException("chickenTypes");
            }
            if (!chickenTypes.Any())
            {
                throw new ArgumentException("At least one chicken must be specified.", "chickenTypes");
            }
            if (chickenTypes.Any(item => item == null))
            {
                throw new ArgumentException("The collection contains a null element.", "chickenTypes");
            }
            if (chickenTypes.Any(item => !typeof(ChickenUnit).IsAssignableFrom(item)))
            {
                throw new ArgumentException("Invalid chicken type.", "chickenTypes");
            }

            #endregion

            // Pre-initialized properties
            this.Size = size;

            // Post-initialized properties
            this.AllChickens = chickenTypes.Select(item => CreateChicken(item)).ToList().AsReadOnly();
            this.AliveChickensDirect = new List<ChickenUnit>(this.AllChickens);
            this.AliveChickens = this.AliveChickensDirect.AsReadOnly();
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

        private ChickenUnit CreateChicken(Type item)
        {
            var result = (ChickenUnit)Activator.CreateInstance(item);

            // TODO: Improve positioning: 1) not same as any previous; 2) check minimum distance
            result.Position = new Point(s_random.Next(this.Size.Width), s_random.Next(this.Size.Height));

            result.Board = this;
            return result;
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

        public Point? GetPeckAttackPoint(ChickenUnit chicken)
        {
            #region Argument Check

            if (chicken == null)
            {
                throw new ArgumentNullException("chicken");
            }

            #endregion

            var offset = s_beakAngleToOffsetMap[chicken.BeakAngle];
            var result = chicken.Position;
            result.Offset(offset);
            return IsValidPoint(result) ? result : (Point?)null;
        }

        public ChickenUnit GetChickenAtPoint(Point value)
        {
            return this.AliveChickens.SingleOrDefault(item => item.Position == value);
        }

        #endregion
    }
}