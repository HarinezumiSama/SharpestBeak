using System;
using System.Collections.Generic;
using System.Linq;
using SharpestBeak.Model;

namespace SharpestBeak.UI
{
    public sealed class LogicInfo : IEquatable<LogicInfo>
    {
        #region Fields

        private readonly string _asString;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicInfo"/> class.
        /// </summary>
        internal LogicInfo(Type type)
        {
            #region Argument Check

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            #endregion

            var logic = (ChickenUnitLogic)Activator.CreateInstance(type).EnsureNotNull();
            var caption = logic.Caption;

            if (caption.IsNullOrWhiteSpace())
            {
                throw new ApplicationException(string.Format("Logic '{0}' has empty caption.", type.FullName));
            }

            this.Type = type;
            this.Caption = caption;
            _asString = string.Format("{0} ({1})", caption, type.Name);
        }

        #endregion

        #region Public Properties

        public Type Type
        {
            get;
            private set;
        }

        public string Caption
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LogicInfo);
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode();
        }

        public override string ToString()
        {
            return _asString;
        }

        #endregion

        #region IEquatable<LogicInfo> Members

        public bool Equals(LogicInfo other)
        {
            return !ReferenceEquals(other, null) && Equals(this.Type, other.Type);
        }

        #endregion
    }
}