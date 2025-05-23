﻿using System;
using System.Runtime.Serialization;

namespace SharpestBeak;

[Serializable]
public sealed class GameException : Exception
{
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
}