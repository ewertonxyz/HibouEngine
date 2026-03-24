using Editor.Components.TopMenu.Help;
using Editor.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Components.MenuBar
{
    public partial class HelpMenuView : MenuItem, IEngineComponent
    {
        public string ComponentName => "HelpMenuView";

        public HelpMenuView()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = Application.Current.MainWindow;
            aboutWindow.ShowDialog();
        }
    }
}
