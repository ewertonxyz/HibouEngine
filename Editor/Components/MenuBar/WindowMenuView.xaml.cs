using Editor.Interfaces;
using System.Windows.Controls;

namespace Editor.Components.MenuBar
{
    public partial class WindowMenuView : MenuItem, IEngineComponent
    {
        public string ComponentName => "WindowMenu";

        public WindowMenuView()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
