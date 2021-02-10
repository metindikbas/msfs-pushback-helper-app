using System.Windows;
using System.Windows.Threading;

namespace PushbackHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = string.Format("{0} {1} {2}", e.Exception.Message, e.Exception.TargetSite, e.Exception.InnerException);
            MessageBox.Show(message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            Clipboard.SetText(message);
        }
    }
}
