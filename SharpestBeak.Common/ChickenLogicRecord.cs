using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    public sealed class ChickenLogicRecord
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenLogicRecord"/> class.
        /// </summary>
        public ChickenLogicRecord(Type type, int unitCount)
        {
            #region Argument Check

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (unitCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "unitCount",
                    unitCount,
                    "The number of units must be positive.");
            }

            #endregion

            this.Type = type;
            this.UnitCount = unitCount;
        }

        #endregion

        #region Public Properties

        public Type Type
        {
            get;
            private set;
        }

        public int UnitCount
        {
            get;
            private set;
        }

        #endregion
    }
}