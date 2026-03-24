using System.Windows.Controls;
using Editor.Interfaces;

namespace Editor.Components.MenuBar
{
    public partial class EditMenuView : MenuItem, IEngineComponent
    {
        public string ComponentName => "EditMenu";

        public EditMenuView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}