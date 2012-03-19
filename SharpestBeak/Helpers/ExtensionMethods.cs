using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using SharpestBeak;
using SharpestBeak.Model;

// The type is placed intentionally in the root namespace to ease access from other projects and namespaces
namespace SharpestBeak
{
    public static class ExtensionMethods
    {
        #region Private Methods

        [DebuggerNonUserCode]
        private static bool IsSetInternal<TEnum>(this TEnum enumerationValue, TEnum flags, bool all)
            where TEnum : struct
        {
            #region Argument Check

            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                    string.Format("The type must be an enumeration ({0}).", enumType.FullName),
                    "TEnum");
            }
            if (!enumType.IsDefined(typeof(FlagsAttribute), true))
            {
                throw new ArgumentException(
                    string.Format("The type must be a flag enumeration ({0}).", enumType.FullName),
                    "TEnum");
            }

            #endregion

            Type underlyingType = Enum.GetUnderlyingType(enumType);
            if (underlyingType == typeof(ulong))
            {
                ulong castFlags = Convert.ToUInt64(flags);
                ulong andedValue = Convert.ToUInt64(enumerationValue) & castFlags;
                return all ? andedValue == castFlags : andedValue != 0;
            }
            else
            {
                long castFlags = Convert.ToInt64(flags);
                long andedValue = Convert.ToInt64(enumerationValue) & castFlags;
                return all ? andedValue == castFlags : andedValue != 0;
            }
        }

        #endregion

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
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
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

        public static bool IsRejected(this MoveInfoStates value)
        {
            return (value & MoveInfoStates.RejectedMask) != 0;
        }

        /// <summary>
        ///     Determines whether the specified enumeration value contains all the specified flags set.
        /// </summary>
        /// <typeparam name="TEnum">
        ///     The system type of the enumeration.
        /// </typeparam>
        /// <param name="enumerationValue">
        ///     The enumeration value to check the flags in.
        /// </param>
        /// <param name="flags">
        ///     The combination of the bit flags to check.
        /// </param>
        /// <returns>
        ///     <b>true</b> if all bits specified in <paramref name="flags"/> are set
        ///     in <paramref name="enumerationValue"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     <para>
        ///         <typeparamref name="TEnum"/> is not an enumeration type.
        ///     </para>
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <para>
        ///         The enumeration <typeparamref name="TEnum"/> is not a flag enumeration, that is,
        ///         <typeparamref name="TEnum"/> type is not marked by <see cref="System.FlagsAttribute"/>.
        ///     </para>
        /// </exception>
        [DebuggerNonUserCode]
        public static bool IsAllSet<TEnum>(this TEnum enumerationValue, TEnum flags)
            where TEnum : struct
        {
            return IsSetInternal<TEnum>(enumerationValue, flags, true);
        }

        /// <summary>
        ///     Determines whether the specified enumeration value contains any of the specified flags set.
        /// </summary>
        /// <typeparam name="TEnum">
        ///     The system type of the enumeration.
        /// </typeparam>
        /// <param name="enumerationValue">
        ///     The enumeration value to check the flags in.
        /// </param>
        /// <param name="flags">
        ///     The combination of the bit flags to check.
        /// </param>
        /// <returns>
        ///     <b>true</b> if any of flags specified by the <paramref name="flags"/> parameter is set
        ///     in <paramref name="enumerationValue"/>; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///     <para>
        ///         <typeparamref name="TEnum"/> is not an enumeration type.
        ///     </para>
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <para>
        ///         The enumeration <typeparamref name="TEnum"/> is not a flag enumeration, that is,
        ///         <typeparamref name="TEnum"/> type is not marked by <see cref="System.FlagsAttribute"/>.
        ///     </para>
        /// </exception>
        [DebuggerNonUserCode]
        public static bool IsAnySet<TEnum>(this TEnum enumerationValue, TEnum flags)
            where TEnum : struct
        {
            return IsSetInternal<TEnum>(enumerationValue, flags, false);
        }

        public static SizeF Scale(this SizeF value, float coefficient)
        {
            return new SizeF(value.Width * coefficient, value.Height * coefficient);
        }

        public static PointF Scale(this PointF value, float coefficient)
        {
            return new PointF(value.X * coefficient, value.Y * coefficient);
        }

        #endregion
    }
}