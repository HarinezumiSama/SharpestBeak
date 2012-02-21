using System;
using System.Runtime.Serialization;

namespace SharpestBeak.Common
{
    [Serializable]
    public sealed class GameException : Exception
    {
        #region Constructors

        public GameException(string message)
            : base(message)
        {
            // Nothing to do
        }

        public GameException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Nothing to do
        }

        private GameException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Nothing to do
        }

        #endregion
    }
}