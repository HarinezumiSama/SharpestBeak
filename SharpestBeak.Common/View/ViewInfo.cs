using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.View
{
    [DebuggerDisplay("{GetType().Name,nq}. Chickens.Count = {Chickens.Count}, Shots.Count = {Shots.Count}")]
    public sealed class ViewInfo
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewInfo"/> class.
        /// </summary>
        internal ViewInfo(ChickenUnit unit)
        {
            #region Argument Check

            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            #endregion

            var engine = unit.Logic.Engine;
            var unitTeam = unit.Team;

            // TODO: [VM] Improve computing details according to actual unit view
            // [VM] Currently, algorithm of determining if other chicken or shot is visible is very simple

            this.Chickens = engine
                .AliveChickens
                .Where(
                    item => !item.IsDead
                        && item != unit
                        && (item.Team == unitTeam || unit.CanSee(item.Position)))
                .Select(item => new ChickenViewData(item))
                .ToList()
                .AsReadOnly();

            this.Shots = engine
                .ShotUnits
                .Where(item => unit.CanSee(item.Position))
                .Select(item => new ShotViewData(item))
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public IList<ChickenViewData> Chickens
        {
            get;
            private set;
        }

        public IList<ShotViewData> Shots
        {
            get;
            private set;
        }

        #endregion
    }
}