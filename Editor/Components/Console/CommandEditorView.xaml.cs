using Editor.Interfaces;
using System.Windows.Controls;

namespace Editor.Components.Console
{
    public partial class CommandEditorView : UserControl, IEngineComponent
    {
        public string ComponentName => "CommandEditor";

        public CommandEditorView()
        {
            InitializeComponent();
        }

        public void Initialize() { }
    }
}