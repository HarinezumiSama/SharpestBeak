using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using SharpestBeak.Diagnostics;

namespace SharpestBeak
{
    public sealed class ThreadSafeRandom : Random
    {
        #region Fields

        private static ulong s_instanceCount;
        private static readonly object s_instanceCountLock = new object();

        private readonly object m_syncLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeRandom"/> class
        ///     using the specified seed value.
        /// </summary>
        public ThreadSafeRandom(int seed)
            : base(seed)
        {
            this.InitialSeed = seed;
            this.InstanceId = GetNextInstanceId();

            DebugHelper.WriteLineForced(
                "{0} #{1}: InitialSeed = {2}",
                GetType().FullName,
                this.InstanceId,
                this.InitialSeed);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ThreadSafeRandom"/> class.
        /// </summary>
        public ThreadSafeRandom()
            : this(Environment.TickCount)
        {
            // Nothing to do
        }

        #endregion

        #region Private Methods

        private static ulong GetNextInstanceId()
        {
            lock (s_instanceCountLock)
            {
                s_instanceCount++;
                return s_instanceCount;
            }
        }

        #endregion

        #region Protected Methods

        protected override double Sample()
        {
            lock (m_syncLock)
            {
                return base.Sample();
            }
        }

        #endregion

        #region Public Properties

        public ulong InstanceId
        {
            get;
            private set;
        }

        public int InitialSeed
        {
            get;
            private set;
        }

        public object SyncLock
        {
            [DebuggerStepThrough]
            get
            {
                return m_syncLock;
            }
        }

        #endregion

        #region Public Methods

        public override int Next()
        {
            lock (m_syncLock)
            {
                return base.Next();
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (m_syncLock)
            {
                return base.Next(minValue, maxValue);
            }
        }

        public override int Next(int maxValue)
        {
            lock (m_syncLock)
            {
                return base.Next(maxValue);
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            lock (m_syncLock)
            {
                base.NextBytes(buffer);
            }
        }

        public override double NextDouble()
        {
            lock (m_syncLock)
            {
                return base.NextDouble();
            }
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}. InstanceId = {1}, InitialSeed = {2}",
                GetType().Name,
                this.InstanceId,
                this.InitialSeed);
        }

        public T ChooseRandomValue<T>(IList<T> list)
        {
            #region Argument Check

            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            #endregion

            return list[Next(list.Count)];
        }

        #endregion
    }
}