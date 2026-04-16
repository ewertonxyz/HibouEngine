using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Editor
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var sb = new StringBuilder();
            var ex = e.Exception;
            while (ex != null)
            {
                sb.AppendLine($"{ex.GetType().Name}: {ex.Message}");
                sb.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
                if (ex != null) sb.AppendLine("--- Inner Exception ---");
            }

            Debug.WriteLine($"[HibouEngine] UNHANDLED EXCEPTION:\n{sb}");
            MessageBox.Show(sb.ToString(), "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
