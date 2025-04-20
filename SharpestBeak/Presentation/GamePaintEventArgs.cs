using System;

namespace SharpestBeak.Presentation;

public sealed class GamePaintEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePaintEventArgs"/> class.
    /// </summary>
    internal GamePaintEventArgs(GamePresentation presentation) => Presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));

    public GamePresentation Presentation { get; }
}