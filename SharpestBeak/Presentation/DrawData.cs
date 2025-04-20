using System;
using System.Drawing;

namespace SharpestBeak.Presentation;

//// TODO: [VM] To be removed
public sealed class DrawData : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DrawData"/> class.
    /// </summary>
    public DrawData(Color color, float coefficient)
    {
        if (coefficient.IsNegativeOrZero())
        {
            throw new ArgumentOutOfRangeException(nameof(coefficient), coefficient, "The coefficient must be positive.");
        }

        Color = color;
        Coefficient = coefficient;

        Brush = new SolidBrush(color);
        Pen = new Pen(color);
    }

    public Color Color { get; }

    public float Coefficient { get; }

    public Brush Brush { get; }

    public Pen Pen { get; }

    public void Dispose()
    {
        Brush.DisposeSafely();
        Pen.DisposeSafely();
    }
}