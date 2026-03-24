using Editor.Interfaces;
using System.Windows.Controls;

namespace Editor.Components.SideTools
{
    public partial class VerticalToolBarView : UserControl, IEngineComponent
    {
        public string ComponentName => "VerticalToolbar";

        public VerticalToolBarView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}