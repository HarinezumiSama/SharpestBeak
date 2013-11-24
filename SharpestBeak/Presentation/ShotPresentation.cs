using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Physics;

namespace SharpestBeak.Presentation
{
    public sealed class ShotPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShotPresentation"/> class.
        /// </summary>
        internal ShotPresentation(GamePresentation gamePresentation, ShotUnit shotUnit)
        {
            #region Argument Check

            if (gamePresentation == null)
            {
                throw new ArgumentNullException("gamePresentation");
            }

            if (shotUnit == null)
            {
                throw new ArgumentNullException("shotUnit");
            }

            #endregion

            this.GamePresentation = gamePresentation;
            this.UniqueId = shotUnit.UniqueId;
            this.OwnerTeam = shotUnit.Owner.Team;

            this.InitialPosition = shotUnit.Position;
            this.Movement = shotUnit.Movement;
        }

        #endregion

        #region Public Properties

        public GamePresentation GamePresentation
        {
            get;
            private set;
        }

        public GameObjectId UniqueId
        {
            get;
            private set;
        }

        public GameTeam OwnerTeam
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public Point2D GetCurrentPosition()
        {
            var ratio = GetCurrentRatio();
            var currentMovement = this.Movement * ratio;

            var result = this.InitialPosition + currentMovement;
            return result;
        }

        #endregion

        #region Internal Properties

        internal Point2D InitialPosition
        {
            get;
            private set;
        }

        internal Vector2D Movement
        {
            get;
            private set;
        }

        #endregion

        #region Private Methods

        private float GetCurrentRatio()
        {
            return this.GamePresentation.StepStopwatch.StepRatio;
        }

        #endregion
    }
}