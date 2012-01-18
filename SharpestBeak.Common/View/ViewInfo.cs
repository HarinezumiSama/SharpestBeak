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

            // [VM] Currently every chicken knows about every other chicken
            // [VM] Anyway, each chicken should know everything about teammates
            var allChickens = engine
                .AliveChickens
                .Where(item => !item.IsDead && item != unit)
                .Select(item => new ChickenViewData(item))
                .ToList()
                .AsReadOnly();

            // [VM] Currently every chicken knows about every shot
            var allShots = engine.ShotUnits.Select(item => new ShotViewData(item)).ToList().AsReadOnly();

            // TODO: [VM] Compute details according to actual unit view

            this.Chickens = allChickens;
            this.Shots = allShots;
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