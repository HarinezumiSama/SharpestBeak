using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpestBeak.Common
{
    public static class Helper
    {
        #region Public Methods

        public static void DisposeAndNull<T>(ref T disposable)
            where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        #endregion
    }
}