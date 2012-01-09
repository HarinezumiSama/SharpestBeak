using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SharpestBeak.Common
{
    internal static class DebugHelper
    {
        #region Fields

        [ThreadStatic]
        private static StringBuilder s_sb;

        #endregion

        #region Public Methods

        public static void WriteLine(string format, params object[] args)
        {
            if (s_sb == null)
            {
                s_sb = new StringBuilder();
            }
            else
            {
                s_sb.Clear();
            }

            s_sb.AppendFormat("[{0}] ", DateTime.Now.ToFixedString());
            s_sb.AppendFormat(format, args);
            Debug.WriteLine(s_sb.ToString());
            s_sb.Clear();
        }

        #endregion
    }
}