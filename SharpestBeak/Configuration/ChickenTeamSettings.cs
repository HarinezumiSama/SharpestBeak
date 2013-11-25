using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpestBeak.Configuration
{
    public sealed class ChickenTeamSettings
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenTeamSettings"/> class.
        /// </summary>
        public ChickenTeamSettings(Type type, int unitCount)
        {
            #region Argument Check

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!unitCount.IsInRange(GameConstants.TeamPlayerUnitCountRange))
            {
                throw new ArgumentOutOfRangeException(
                    "unitCount",
                    unitCount,
                    string.Format(
                        "The number of units must be in the range {0}.",
                        GameConstants.TeamPlayerUnitCountRange));
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