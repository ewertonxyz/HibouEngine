using System.Windows.Controls;
using Editor.Interfaces;

namespace Editor.Components.MenuBar
{
    public partial class CreateMenuView : MenuItem, IEngineComponent
    {
        public string ComponentName => "CreateMenu";

        public CreateMenuView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}