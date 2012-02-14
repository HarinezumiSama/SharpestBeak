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
        #region Fields

        private static readonly IList<ChickenViewData> s_emptyChickens = new ChickenViewData[0];
        private static readonly IList<ShotViewData> s_emptyShots = new ShotViewData[0];

        #endregion

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

            if (unit.IsDead)
            {
                this.Chickens = s_emptyChickens;
                this.Shots = s_emptyShots;
            }
            else
            {
                var chickens = new List<ChickenViewData>(unit.IsDead ? 0 : engine.AliveChickens.Count);
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

                var shots = new List<ShotViewData>(unit.IsDead ? 0 : engine.ShotUnits.Count);
                foreach (var shotUnit in engine.ShotUnits)
                {
                    if (unit.CanSee(shotUnit.Position))
                    {
                        shots.Add(new ShotViewData(shotUnit));
                    }
                }
                this.Shots = shots.AsReadOnly();
            }
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