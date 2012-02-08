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

            var chickens = new List<ChickenViewData>(engine.AliveChickens.Count);
            foreach (var aliveChicken in engine.AliveChickens)
            {
                if (!aliveChicken.IsDead
                    && aliveChicken != unit
                    && (aliveChicken.Team == unitTeam || unit.CanSee(aliveChicken.Position)))
                {
                    chickens.Add(new ChickenViewData(aliveChicken));
                }
            }
            this.Chickens = chickens.AsReadOnly();

            var shots = new List<ShotViewData>(engine.ShotUnits.Count);
            foreach (var shotUnit in engine.ShotUnits)
            {
                if (unit.CanSee(shotUnit.Position))
                {
                    shots.Add(new ShotViewData(shotUnit));
                }
            }
            this.Shots = shots.AsReadOnly();
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