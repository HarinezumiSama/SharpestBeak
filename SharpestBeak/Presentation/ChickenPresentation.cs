using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Presentation
{
    public class ChickenPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenPresentation"/> class.
        /// </summary>
        internal ChickenPresentation(ChickenUnit chickenUnit)
        {
            #region Argument Check

            if (chickenUnit == null)
            {
                throw new ArgumentNullException("chickenUnit");
            }

            #endregion

            this.Element = chickenUnit.GetElement();
            this.UniqueId = chickenUnit.UniqueId;
            this.Team = chickenUnit.Team;
        }

        #endregion

        #region Public Properties

        public ChickenElement Element
        {
            get;
            private set;
        }

        public GameObjectId UniqueId
        {
            get;
            private set;
        }

        public GameTeam Team
        {
            get;
            private set;
        }

        #endregion
    }
}