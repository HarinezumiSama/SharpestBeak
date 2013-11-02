using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpestBeak.Diagnostics
{
    public static class DebugHelper
    {
        #region Public Methods

        public static void WriteLine(string message)
        {
            if (!SettingsCache.Instance.EnableDebugOutput)
            {
                return;
            }

            WriteLineInternal(message);
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (!SettingsCache.Instance.EnableDebugOutput)
            {
                return;
            }

            WriteLineInternal(format, args);
        }

        public static void WriteLineForced(string message)
        {
            WriteLineInternal(message);
        }

        public static void WriteLineForced(string format, params object[] args)
        {
            WriteLineInternal(format, args);
        }

        public static void CallAndMeasure(Action action)
        {
            if (!SettingsCache.Instance.EnableDebugOutput)
            {
                action();
                return;
            }

            using (new AutoStopwatch(WriteLineInternal)
            {
                OutputFormat = string.Format("{0} took {{0}}.", action.Method.GetQualifiedName())
            })
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

            using (new AutoStopwatch(WriteLineInternal)
            {
                OutputFormat = string.Format("{0} took {{0}}.", action.Method.GetQualifiedName())
            })
            {
                return action();
            }
        }

        #endregion

        #region Private Methods

        private static void WriteLineInternal(string message)
        {
            Trace.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToFixedString(), message));
        }

        private static void WriteLineInternal(string format, params object[] args)
        {
            WriteLineInternal(string.Format(format, args));
        }

        #endregion
    }
}