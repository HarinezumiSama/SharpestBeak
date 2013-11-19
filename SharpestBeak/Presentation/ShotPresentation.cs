using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Presentation
{
    public sealed class ShotPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotPresentation"/> class.
        /// </summary>
        internal ShotPresentation(
            GamePresentation gamePresentation,
            ShotUnit shotUnit,
            IDictionary<GameObjectId, ChickenPresentation> chickenMap)
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

            this.GamePresentation = gamePresentation;
            this.Element = shotUnit.GetElement();
            this.UniqueId = shotUnit.UniqueId;
            var createdChickenPresentation = chickenMap.GetValueOrDefault(shotUnit.Owner.UniqueId);
            this.Owner = createdChickenPresentation ?? new ChickenPresentation(gamePresentation, shotUnit.Owner);
        }

        #endregion

        #region Public Properties

        public GamePresentation GamePresentation
        {
            get;
            private set;
        }

        public ShotElement Element
        {
            get;
            private set;
        }

        public GameObjectId UniqueId
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