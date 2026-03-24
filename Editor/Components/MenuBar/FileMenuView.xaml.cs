using System.Windows.Controls;
using Editor.Interfaces;

namespace Editor.Components.MenuBar
{
    public partial class FileMenuView : MenuItem, IEngineComponent
    {
        public string ComponentName => "FileMenu";

        public FileMenuView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}