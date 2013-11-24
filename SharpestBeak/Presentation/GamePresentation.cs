using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpestBeak.Presentation
{
    public sealed class GamePresentation
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePresentation"/> class.
        /// </summary>
        internal GamePresentation(GameEngine engine)
        {
            #region Argument Check

            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }

            #endregion

            this.Data = engine.Data;
            this.StepStopwatch = new EngineStepStopwatch();

            this.Chickens = engine
                .AliveChickens
                .Select(item => new ChickenPresentation(this, item))
                .ToList()
                .AsReadOnly();

            var chickenMap = this.Chickens.ToDictionary(item => item.UniqueId);
            this.Shots = engine
                .ShotUnits
                .Select(item => new ShotPresentation(this, item))
                .ToList()
                .AsReadOnly();

            // This MUST be the very last statement
            this.StepStopwatch.Start();
        }

        #endregion

        #region Public Properties

        public GameEngineData Data
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ChickenPresentation> Chickens
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ShotPresentation> Shots
        {
            get;
            private set;
        }

        #endregion

        #region Internal Properties

        internal EngineStepStopwatch StepStopwatch
        {
            get;
            private set;
        }

        #endregion
    }
}