using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.View
{
    [DebuggerDisplay("{GetType().Name,nq}. Items.Count = {Items.Count}")]
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

            // [VM] Currently every chicken know about every other chicken
            var allChickens = unit
                .Logic
                .Engine
                .AliveChickens
                .Where(item => !item.IsDead && item != unit)
                .Select(item => new ChickenViewData(item))
                .ToArray();

            // TODO: [VM] Compute details according to actual unit view

            this.Items = allChickens.Cast<BaseViewData>().ToList().AsReadOnly();
        }

        #endregion

        #region Public Properties

        public IList<BaseViewData> Items
        {
            get;
            private set;
        }

        #endregion
    }
}