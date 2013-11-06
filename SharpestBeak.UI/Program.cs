using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SharpestBeak.UI
{
    internal static class Program
    {
        #region Private Methods

        private static void HandleUnexpectedException(object sender, UnhandledExceptionEventArgs e)
        {
            var form = Application.OpenForms.OfType<Form>().FirstOrDefault();

            var message = new StringBuilder();
            message.Append("An unhandled exception has occurred. The application will now terminate.");
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                message
                    .AppendLine()
                    .AppendLine()
                    .AppendFormat(exception.ToString());
            }

            form.ShowErrorMessage(message.ToString(), "Unhandled error", MessageBoxIcon.Stop);

            Process.GetCurrentProcess().Kill();
        }

        #endregion

        #region Entry Point

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += Domain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        #endregion

        #region Event Handlers

        private static void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnexpectedException(sender, e);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleUnexpectedException(sender, new UnhandledExceptionEventArgs(e.Exception, false));
        }

        #endregion
    }
}