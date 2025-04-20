using System;
using SharpestBeak.Properties;

namespace SharpestBeak;

internal sealed class SettingsCache
{
    private SettingsCache() => ReadSettings();

    public static SettingsCache Instance { get; } = new();

    public bool UsePerformanceCounters
    {
        get;
        private set;
    }

    public bool EnableDebugOutput
    {
        get;
        private set;
    }

    public bool InstrumentationMode
    {
        get;
        private set;
    }

    //// TODO: [vmcl] Allow to disable shooting in UI via editable setting

    public bool DebugModeDisableShooting
    {
        get;
        private set;
    }

    public void Reload() => ReadSettings();

    private void ReadSettings()
    {
        var settings = Settings.Default.EnsureNotNull();

        UsePerformanceCounters = settings.UsePerformanceCounters;
        EnableDebugOutput = settings.EnableDebugOutput;
        InstrumentationMode = settings.InstrumentationMode;
        DebugModeDisableShooting = settings.DebugModeDisableShooting;
    }
}