using Editor.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Editor.Components.Console
{
    public partial class LogConsoleView : UserControl, IEngineComponent
    {
        public string ComponentName => "LogConsole";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LogCallback(int level, string message);

        [DllImport("Engine.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogCallback(LogCallback callback);

        private static LogCallback _callbackInstance;
        private static LogConsoleView _instance;

        public LogConsoleView()
        {
            InitializeComponent();
            _instance = this;

            _callbackInstance = new LogCallback(OnLogReceived);

            try
            {
                SetLogCallback(_callbackInstance);
                AppendLog("Log System Connected.");
            }
            catch (Exception ex)
            {
                AppendLog($"Log Connect Error: {ex.Message}");
            }
        }

        private static void OnLogReceived(int level, string message)
        {
            if (_instance != null)
            {
                _instance.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    _instance.AppendLog(message);
                }));
            }

            System.Console.WriteLine($"[Engine] {message}");
        }

        public void Initialize()
        {
            _instance = this;
        }

        public void AppendLog(string text)
        {
            if (LogOutput != null)
            {
                if (!text.StartsWith("[")) text = $"[{DateTime.Now:HH:mm:ss}] {text}";
                LogOutput.AppendText(text + "\n");
                LogOutput.ScrollToEnd();
            }
        }
    }
}