using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpestBeak.Common.Properties;

namespace SharpestBeak.Common.Diagnostics
{
    public static class DebugHelper
    {
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

        #region Public Methods

        public static void WriteLine(string message)
        {
            if (!Settings.Default.EnableDebugOutput)
            {
                return;
            }

            WriteLineInternal(message);
        }

        public static void WriteLine(string format, params object[] args)
        {
            if (!Settings.Default.EnableDebugOutput)
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
            if (!Settings.Default.EnableDebugOutput)
            {
                action();
                return;
            }

            using (var asw =
                new AutoStopwatch(s => WriteLineInternal(s))
                {
                    OutputFormat = string.Format("{0} took {{0}}.", action.Method.GetQualifiedName())
                })
            {
                action();
            }
        }

        public static T CallAndMeasure<T>(Func<T> action)
        {
            if (!Settings.Default.EnableDebugOutput)
            {
                return action();
            }

            using (var asw =
                new AutoStopwatch(s => WriteLineInternal(s))
                {
                    OutputFormat = string.Format("{0} took {{0}}.", action.Method.GetQualifiedName())
                })
            {
                return action();
            }
        }

        #endregion
    }
}