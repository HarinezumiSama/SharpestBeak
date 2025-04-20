namespace SharpestBeak.Physics;

public readonly struct QuadraticEquation
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QuadraticEquation"/> class.
    /// </summary>
    public QuadraticEquation(float a, float b, float c)
    {
        A = a;
        B = b;
        C = c;
    }

    public float A { get; }

    public float B { get; }

    public float C { get; }

    public float GetDiscriminant() => B.Sqr() - 4f * A * C;

    public int GetRoots(out float x1, out float x2)
    {
        if (A.IsZero())
        {
            //// Linear equation case

            if (B.IsZero())
            {
                x1 = 0f;
                x2 = 0f;
                return 0;
            }

            x1 = -C / B;
            x2 = x1;
            return 1;
        }

        var d = GetDiscriminant();
        if (d.IsNegative())
        {
            x1 = 0f;
            x2 = 0f;
            return 0;
        }

        var ds = d.Sqrt();
        var a2 = 2f * A;

        x1 = (-B - ds) / a2;
        x2 = (-B + ds) / a2;
        return 2;
    }

    public int GetRootsOrdered(out float x1, out float x2)
    {
        var result = GetRoots(out x1, out x2);
        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
        }

        return result;
    }
}