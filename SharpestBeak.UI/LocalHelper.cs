using System.Windows.Media;

namespace SharpestBeak.UI;

internal static class LocalHelper
{
    public static Color WithAlpha(this Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);
}