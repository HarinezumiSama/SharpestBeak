using System;
using System.Linq;
using System.Windows.Media;

namespace SharpestBeak.UI
{
    internal static class LocalHelper
    {
        #region Public Methods

        public static Color WithAlpha(this Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        #endregion
    }
}