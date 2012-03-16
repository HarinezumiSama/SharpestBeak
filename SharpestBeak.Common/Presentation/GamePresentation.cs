using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common.Presentation
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

            this.Chickens = engine
                .AliveChickens
                .Select(item => new ChickenPresentation(item))
                .ToList()
                .AsReadOnly();

            var chickenMap = this.Chickens.ToDictionary(item => item.UniqueId);
            this.Shots = engine
                .ShotUnits
                .Select(item => new ShotPresentation(item, chickenMap))
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region Public Properties

        public GameEngineData Data
        {
            get;
            private set;
        }

        public IList<ChickenPresentation> Chickens
        {
            get;
            private set;
        }

        public IList<ShotPresentation> Shots
        {
            get;
            private set;
        }

        #endregion
    }
}