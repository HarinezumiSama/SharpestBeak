using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Model;
using SharpestBeak.Physics;
using SharpestBeak.Presentation.Elements;

namespace SharpestBeak.Presentation
{
    public class ChickenPresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChickenPresentation"/> class.
        /// </summary>
        internal ChickenPresentation(GamePresentation gamePresentation, ChickenUnit chickenUnit)
        {
            #region Argument Check

            if (gamePresentation == null)
            {
                throw new ArgumentNullException("gamePresentation");
            }

            if (chickenUnit == null)
            {
                throw new ArgumentNullException("chickenUnit");
            }

            #endregion

            this.GamePresentation = gamePresentation;
            this.UniqueId = chickenUnit.UniqueId;
            this.Team = chickenUnit.Team;

            this.InitialPosition = chickenUnit.Position;
            this.Movement = chickenUnit.Movement;

            this.InitialBeakAngle = chickenUnit.BeakAngle;
            this.BeakMovement = chickenUnit.BeakMovement;
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

        public GameTeam Team
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public DirectionalPosition GetCurrentPosition()
        {
            var ratio = GetCurrentRatio();
            
            var currentMovement = this.Movement * ratio;
            var currentBeakMovement = this.BeakMovement * ratio;

            var position = this.InitialPosition + currentMovement;
            var angle = this.InitialBeakAngle + currentBeakMovement;
            
            return new DirectionalPosition(position, angle);
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

        internal GameAngle InitialBeakAngle
        {
            get;
            private set;
        }

        internal GameAngle BeakMovement
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