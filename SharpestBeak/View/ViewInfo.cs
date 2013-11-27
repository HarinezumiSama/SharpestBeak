using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SharpestBeak.Model;

namespace SharpestBeak.View
{
    [DebuggerDisplay("{GetType().Name,nq}. Chickens.Count = {Chickens.Count}, Shots.Count = {Shots.Count}")]
    public sealed class ViewInfo
    {
        #region Constants and Fields

        private static readonly ReadOnlyCollection<ChickenViewData> EmptyChickens =
            new ChickenViewData[0].AsReadOnly();

        private static readonly ReadOnlyCollection<ShotViewData> EmptyShots = new ShotViewData[0].AsReadOnly();

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

            var engine = unit.LogicExecutor.Engine;
            var unitTeam = unit.Team;

            if (unit.IsDead)
            {
                this.Chickens = EmptyChickens;
                this.Shots = EmptyShots;
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

        public ReadOnlyCollection<ChickenViewData> Chickens
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ShotViewData> Shots
        {
            get;
            private set;
        }

        #endregion
    }
}