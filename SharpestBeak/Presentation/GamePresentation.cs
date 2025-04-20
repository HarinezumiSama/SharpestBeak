using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpestBeak.Presentation;

public sealed class GamePresentation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePresentation"/> class.
    /// </summary>
    internal GamePresentation(GameEngine engine)
    {
        if (engine is null)
        {
            throw new ArgumentNullException(nameof(engine));
        }

        Data = engine.Data;
        StepStopwatch = new EngineStepStopwatch();

        Chickens = engine
            .AliveChickens
            .Select(item => new ChickenPresentation(this, item))
            .ToList()
            .AsReadOnly();

        ////var chickenMap = this.Chickens.ToDictionary(item => item.UniqueId);
        Shots = engine
            .ShotUnits
            .Select(item => new ShotPresentation(this, item))
            .ToList()
            .AsReadOnly();

        // This MUST be the very last statement
        StepStopwatch.Start();
    }

    public GameEngineData Data { get; private set; }

    public ReadOnlyCollection<ChickenPresentation> Chickens { get; }

    public ReadOnlyCollection<ShotPresentation> Shots { get; }

    internal EngineStepStopwatch StepStopwatch { get; }
}