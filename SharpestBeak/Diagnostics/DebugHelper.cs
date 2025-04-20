using System;
using System.Diagnostics;
using System.Reflection;
using Omnifactotum.Annotations;

namespace SharpestBeak.Diagnostics;

public static class DebugHelper
{
    public static void WriteLine(string message)
    {
        if (!SettingsCache.Instance.EnableDebugOutput)
        {
            return;
        }

        WriteLineInternal(message);
    }

    [StringFormatMethod(nameof(format))]
    public static void WriteLine(string format, params object[] args)
    {
        if (!SettingsCache.Instance.EnableDebugOutput)
        {
            return;
        }

        WriteLineInternal(format, args);
    }

    public static void WriteLineForced(string message) => WriteLineInternal(message);

    [StringFormatMethod(nameof(format))]
    public static void WriteLineForced(string format, params object[] args) => WriteLineInternal(format, args);

    public static void CallAndMeasure(Action action)
    {
        if (!SettingsCache.Instance.EnableDebugOutput)
        {
            action();
            return;
        }

        using (new AutoStopwatch(WriteLineInternal) { OutputFormat = $"{action.Method.GetQualifiedName()} took {{0}}." })
        {
            action();
        }
    }

    public static T CallAndMeasure<T>(Func<T> action)
    {
        if (!SettingsCache.Instance.EnableDebugOutput)
        {
            return action();
        }

        using (new AutoStopwatch(WriteLineInternal) { OutputFormat = $"{action.Method.GetQualifiedName()} took {{0}}." })
        {
            return action();
        }
    }

    private static void WriteLineInternal(string message) => Trace.WriteLine($"[{DateTime.Now.ToFixedString()}] {message}");

    private static void WriteLineInternal(string format, params object[] args) => WriteLineInternal(string.Format(format, args));
}