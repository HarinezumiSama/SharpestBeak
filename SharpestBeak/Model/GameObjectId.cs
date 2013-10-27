﻿using System;
using System.Diagnostics;

namespace SharpestBeak.Model
{
    public struct GameObjectId : IEquatable<GameObjectId>
    {
        #region Constants and Fields

        private static readonly GameObjectId NoneField = new GameObjectId();

        private readonly int _value;

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

            _value = value;
        }

        #endregion

        #region Public Properties

        public static GameObjectId None
        {
            [DebuggerStepThrough]
            get { return NoneField; }
        }

        public bool IsValid
        {
            [DebuggerNonUserCode]
            get
            {
                return _value > 0;
            }
        }

        public bool IsNone
        {
            [DebuggerNonUserCode]
            get
            {
                return Equals(NoneField);
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
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}. Value = {1}", GetType().Name, _value);
        }

        #endregion

        #region IEquatable<GameObjectId> Members

        public bool Equals(GameObjectId other)
        {
            return _value == other._value;
        }

        #endregion

        #region Internal Properties

        internal int Value
        {
            [DebuggerStepThrough]
            get { return _value; }
        }

        #endregion
    }
}