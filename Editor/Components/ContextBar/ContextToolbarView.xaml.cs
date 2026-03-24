using Editor.Components.ContextBar;
using Editor.Interfaces;
using System.Windows.Controls;

namespace Editor.Components.ContextBar
{
    public partial class ContextToolbarView : UserControl, IEngineComponent
    {
        public string ComponentName => "ContextToolbar";

        public ContextToolbarView()
        {
            InitializeComponent();
            DataContext = new ContextToolbarViewModel();
        }

        public void Initialize() { }
    }
}