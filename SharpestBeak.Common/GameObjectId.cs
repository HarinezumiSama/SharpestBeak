using System;
using System.Diagnostics;

namespace SharpestBeak.Common
{
    public struct GameObjectId : IEquatable<GameObjectId>
    {
        #region Fields

        private static readonly GameObjectId s_none = new GameObjectId();

        private readonly int m_value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameObjectId"/> structure.
        /// </summary>
        internal GameObjectId(int value)
        {
            #region Argument Check

            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "value",
                    value,
                    "The ID must be positive.");
            }

            #endregion

            m_value = value;
        }

        #endregion

        #region Internal Properties

        internal int Value
        {
            [DebuggerStepThrough]
            get { return m_value; }
        }

        #endregion

        #region Public Properties

        public static GameObjectId None
        {
            [DebuggerStepThrough]
            get { return s_none; }
        }

        public bool IsValid
        {
            [DebuggerNonUserCode]
            get
            {
                return m_value > 0;
            }
        }

        public bool IsNone
        {
            [DebuggerNonUserCode]
            get
            {
                return Equals(s_none);
            }
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is GameObjectId && this.Equals((GameObjectId)obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}. Value = {1}", GetType().Name, m_value);
        }

        #endregion

        #region IEquatable<GameObjectId> Members

        public bool Equals(GameObjectId other)
        {
            return m_value == other.m_value;
        }

        #endregion
    }
}