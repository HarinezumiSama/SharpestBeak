using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    internal static class Helper
    {
        #region Public Methods

        public static T EnsureNotNull<T>(this T value)
            where T : class
        {
            #region Argument Check

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            #endregion

            return value;
        }

        public static void DisposeAndNull<T>(ref T disposable)
            where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        public static void DisposeSafely<T>(this T disposable)
            where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        public static void DoForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            var index = 0;
            foreach (var item in collection)
            {
                action(item, index);
                index++;
            }
        }

        public static void DoForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            #region Argument Check

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            DoForEach<T>(collection, (item, index) => action(item));
        }

        public static bool IsThreadAbort(this Exception exception)
        {
            return exception is ThreadAbortException || exception is ThreadInterruptedException;
        }

        #endregion
    }
}