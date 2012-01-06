using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion
    }
}