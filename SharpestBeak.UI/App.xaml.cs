using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace SharpestBeak.UI;

/// <summary>
///     Interaction logic for App.xaml.
/// </summary>
public partial class App
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        Initialize();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    public new static App Current
    {
        [DebuggerNonUserCode]
        get => (App)Application.Current;
    }

    public string FullProductName { get; private set; }

    public string FullProductDescription { get; private set; }

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
                $"The application has failed to initialize properly:{Environment.NewLine}{Environment.NewLine}[{ex.GetType().FullName}] {ex.Message}{
                    Environment.NewLine}{Environment.NewLine}The application will now terminate.",
                typeof(App).Namespace,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            KillThisProcess();
        }
    }

    private void InitializeProperties()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        var productName = assembly.GetSingleCustomAttribute<AssemblyProductAttribute>(false).Product;
        var productCopyright = assembly.GetSingleCustomAttribute<AssemblyCopyrightAttribute>(false).Copyright;

        var productVersion = assembly.GetSingleOrDefaultCustomAttribute<AssemblyInformationalVersionAttribute>(false)?.InformationalVersion
            ?? assembly.GetName().Version.ToString();

        FullProductName = $"{productName} ({productVersion})";
        FullProductDescription = $"{productName} ({productVersion}) {productCopyright}";
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        var typeName = exception is null ? "<UnknownException>" : exception.GetType().FullName;
        var message = exception is null ? "(Unknown error)" : exception.Message;

        MessageBox.Show(
            $"Unhandled exception has occurred:{Environment.NewLine}{Environment.NewLine}[{typeName}] {message}{Environment.NewLine}{
                Environment.NewLine}The application will now terminate.",
            FullProductName,
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        KillThisProcess();
    }
}