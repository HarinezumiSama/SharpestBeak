using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace SharpestBeak.UI
{
    /// <summary>
    ///     Interaction logic for App.xaml.
    /// </summary>
    public partial class App
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            Initialize();

            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
        }

        #endregion

        #region Public Properties

        public static new App Current
        {
            [DebuggerNonUserCode]
            get
            {
                return (App)Application.Current;
            }
        }

        public string ProductName
        {
            get;
            private set;
        }

        public Version ProductVersion
        {
            get;
            private set;
        }

        public string ProductCopyright
        {
            get;
            private set;
        }

        public string FullProductName
        {
            get;
            private set;
        }

        public string FullProductDescription
        {
            get;
            private set;
        }

        #endregion

        #region Private Methods

        private static void KillThisProcess()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Initialize()
        {
            try
            {
                InitializeProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(
                        "The application has failed to initialize properly:{0}"
                            + "{0}"
                            + "[{1}] {2}{0}"
                            + "{0}"
                            + "The application will now terminate.",
                        Environment.NewLine,
                        ex.GetType().FullName,
                        ex.Message),
                    typeof(App).Namespace,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                KillThisProcess();
            }
        }

        private void InitializeProperties()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            //// Independent properties

            this.ProductName = assembly.GetSoleAttributeStrict<AssemblyProductAttribute>().Product;
            this.ProductVersion = assembly.GetName().Version;
            this.ProductCopyright = assembly.GetSoleAttributeStrict<AssemblyCopyrightAttribute>().Copyright;

            //// Dependent properties

            var versionString = string.Format(
                CultureInfo.InvariantCulture,
                "v{0} rev. {1}",
                this.ProductVersion.ToString(2),
                this.ProductVersion.Revision);

            this.FullProductName = string.Format(
                CultureInfo.InvariantCulture,
                "{0} ({1})",
                this.ProductName,
                versionString);

            this.FullProductDescription = string.Format(
                CultureInfo.InvariantCulture,
                "{0} ({1}) {2}",
                this.ProductName,
                versionString,
                this.ProductCopyright);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            var typeName = exception == null ? "<UnknownException>" : exception.GetType().FullName;
            var message = exception == null ? "(Unknown error)" : exception.Message;

            MessageBox.Show(
                string.Format(
                    "Unhandled exception has occurred:{0}"
                        + "{0}"
                        + "[{1}] {2}{0}"
                        + "{0}"
                        + "The application will now terminate.",
                    Environment.NewLine,
                    typeName,
                    message),
                this.ProductName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            KillThisProcess();
        }

        #endregion
    }
}