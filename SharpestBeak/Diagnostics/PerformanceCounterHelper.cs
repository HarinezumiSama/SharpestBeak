using System.Diagnostics;

namespace SharpestBeak.Diagnostics;

internal sealed class PerformanceCounterHelper
{
    private const string CollisionCountPerStepCounterName = "CollisionCountPerStep";
    private const string CollisionCountPerStepBaseCounterName = "CollisionCountPerStepBase";

    private static readonly string CategoryName = typeof(PerformanceCounterHelper).Assembly.GetName().Name;

    private PerformanceCounterHelper()
    {
        DebugHelper.CallAndMeasure(Setup);

        CollisionCountPerStep = new PerformanceCounter(CategoryName, CollisionCountPerStepCounterName, false)
        {
            RawValue = 0
        };

        CollisionCountPerStepBase = new PerformanceCounter(CategoryName, CollisionCountPerStepBaseCounterName, false)
        {
            RawValue = 0
        };
    }

    public static PerformanceCounterHelper Instance { get; } = new();

    public PerformanceCounter CollisionCountPerStep { get; }

    public PerformanceCounter CollisionCountPerStepBase { get; }

    /// <summary>
    ///     Initializes <see cref="PerformanceCounterHelper"/>. Used to initialize <see cref="PerformanceCounterHelper"/> at a specific time.
    /// </summary>
    public static PerformanceCounterHelper Initialize() => Instance;

    private void Setup()
    {
        if (PerformanceCounterCategory.Exists(CategoryName))
        {
            PerformanceCounterCategory.Delete(CategoryName);
        }

        var counters = new CounterCreationDataCollection
        {
            new CounterCreationData(
                CollisionCountPerStepCounterName,
                "The number of collision detection calls per one game engine step.",
                PerformanceCounterType.AverageCount64),
            new CounterCreationData(
                CollisionCountPerStepBaseCounterName,
                $"Base counter for '{CollisionCountPerStepCounterName}'.",
                PerformanceCounterType.AverageBase)
        };

        PerformanceCounterCategory.Create(
            CategoryName,
            "Sharpest Beak performance counters.",
            PerformanceCounterCategoryType.Unknown,
            counters);

        Debug.WriteLine($"{GetType().FullName} is successfully set up.");
    }
}