using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using SharpestBeak.Model;

//// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
//// ReSharper disable once CheckNamespace
namespace SharpestBeak
{
    public static class ExtensionMethods
    {
        #region Public Methods

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

        public static bool IsRejected(this MoveInfoStates value)
        {
            return (value & MoveInfoStates.RejectedMask) != 0;
        }

        public static SizeF Scale(this SizeF value, float coefficient)
        {
            return new SizeF(value.Width * coefficient, value.Height * coefficient);
        }

        public static PointF Scale(this PointF value, float coefficient)
        {
            return new PointF(value.X * coefficient, value.Y * coefficient);
        }

        public static TimeSpan Multiply(this TimeSpan value, double ratio)
        {
            return TimeSpan.FromTicks((long)(value.Ticks * ratio));
        }

        #endregion
    }
}