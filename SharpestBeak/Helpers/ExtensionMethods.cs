using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using SharpestBeak.Model;

//// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
//// ReSharper disable once CheckNamespace
namespace SharpestBeak;

public static class ExtensionMethods
{
    public static void ChangeContents<T>(this IList<T> list, IEnumerable<T> newContents)
    {
        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (newContents is null)
        {
            throw new ArgumentNullException(nameof(newContents));
        }

        list.Clear();
        if (list is List<T> castList)
        {
            castList.AddRange(newContents);
        }
        else
        {
            foreach (var item in newContents)
            {
                list.Add(item);
            }
        }
    }

    public static void ExecuteInReadLock(this ReaderWriterLockSlim @lock, Action action)
    {
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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
        if (@lock is null)
        {
            throw new ArgumentNullException(nameof(@lock));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

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

    public static bool IsRejected(this MoveInfoStates value) => (value & MoveInfoStates.RejectedMask) != 0;

    public static SizeF Scale(this SizeF value, float coefficient) => new(value.Width * coefficient, value.Height * coefficient);

    public static PointF Scale(this PointF value, float coefficient) => new(value.X * coefficient, value.Y * coefficient);

    public static TimeSpan Multiply(this TimeSpan value, double ratio) => TimeSpan.FromTicks((long)(value.Ticks * ratio));
}