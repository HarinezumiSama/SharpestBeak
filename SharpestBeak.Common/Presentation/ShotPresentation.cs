using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Presentation.Elements;

namespace SharpestBeak.Common.Presentation
{
    public sealed class ShotPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotPresentation"/> class.
        /// </summary>
        internal ShotPresentation(ShotUnit shotUnit, Dictionary<int, ChickenPresentation> chickenMap)
        {
            #region Argument Check

            if (shotUnit == null)
            {
                throw new ArgumentNullException("shotUnit");
            }
            if (chickenMap == null)
            {
                throw new ArgumentNullException("chickenMap");
            }

            #endregion

            this.Element = shotUnit.GetElement();
            var createdChickenPresentation = chickenMap.GetValueOrDefault(shotUnit.Owner.UniqueIndex);
            this.Owner = createdChickenPresentation ?? new ChickenPresentation(shotUnit.Owner);
        }

        #endregion

        #region Public Properties

        public ShotElement Element
        {
            get;
            private set;
        }

        public ChickenPresentation Owner
        {
            get;
            private set;
        }

        #endregion
    }
}