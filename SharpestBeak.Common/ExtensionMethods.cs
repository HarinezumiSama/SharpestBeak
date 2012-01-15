using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    public static class ExtensionMethods
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

        public static void DisposeSafely<T>(this T disposable)
            where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        [DebuggerNonUserCode]
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        [DebuggerNonUserCode]
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

        [DebuggerNonUserCode]
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

        public static void ChangeContents<T>(this IList<T> list, IEnumerable<T> newContents)
        {
            #region Argument Check

            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (newContents == null)
            {
                throw new ArgumentNullException("newContents");
            }

            #endregion

            list.Clear();
            var castList = list as List<T>;
            if (castList == null)
            {
                foreach (var item in newContents)
                {
                    list.Add(item);
                }
            }
            else
            {
                castList.AddRange(newContents);
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default(TValue))
        {
            #region Argument Check

            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            #endregion

            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : defaultValue;
        }

        public static void MinMax(this IEnumerable<float> collection, out float min, out float max)
        {
            #region Argument Check

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            #endregion

            float? minProxy = null;
            float? maxProxy = null;
            foreach (var item in collection)
            {
                if (!minProxy.HasValue || item < minProxy.Value)
                {
                    minProxy = item;
                }
                if (!maxProxy.HasValue || item > maxProxy.Value)
                {
                    maxProxy = item;
                }
            }
            if (!minProxy.HasValue || !maxProxy.HasValue)
            {
                throw new InvalidOperationException("No elements in the collection.");
            }

            min = minProxy.Value;
            max = maxProxy.Value;
        }

        public static bool IsThreadAbort(this Exception exception)
        {
            return exception is ThreadAbortException || exception is ThreadInterruptedException;
        }

        public static void ExecuteInReadLock(this ReaderWriterLockSlim @lock, Action action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public static TResult ExecuteInReadLock<TResult>(this ReaderWriterLockSlim @lock, Func<TResult> action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterReadLock();
            try
            {
                return action();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        public static void ExecuteInUpgradeableReadLock(this ReaderWriterLockSlim @lock, Action action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterUpgradeableReadLock();
            try
            {
                action();
            }
            finally
            {
                @lock.ExitUpgradeableReadLock();
            }
        }

        public static TResult ExecuteInUpgradeableReadLock<TResult>(
            this ReaderWriterLockSlim @lock,
            Func<TResult> action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterUpgradeableReadLock();
            try
            {
                return action();
            }
            finally
            {
                @lock.ExitUpgradeableReadLock();
            }
        }

        public static void ExecuteInWriteLock(this ReaderWriterLockSlim @lock, Action action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterWriteLock();
            try
            {
                action();
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public static TResult ExecuteInWriteLock<TResult>(this ReaderWriterLockSlim @lock, Func<TResult> action)
        {
            #region Argument Check

            if (@lock == null)
            {
                throw new ArgumentNullException("lock");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            #endregion

            @lock.EnterWriteLock();
            try
            {
                return action();
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        public static string GetQualifiedName(this MethodBase method)
        {
            #region Argument Check

            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            #endregion

            return (method.DeclaringType ?? method.ReflectedType).Name + Type.Delimiter + method.Name;
        }

        #endregion
    }
}