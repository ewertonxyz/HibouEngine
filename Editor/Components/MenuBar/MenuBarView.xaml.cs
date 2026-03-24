using Editor.Interfaces;
using System.Windows.Controls;

namespace Editor.Components.MenuBar
{
    public partial class MenuBarView : UserControl, IEngineComponent
    {
        public string ComponentName => "MenuBar";

        public MenuBarView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}