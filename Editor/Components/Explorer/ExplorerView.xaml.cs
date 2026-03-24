using Editor.Interfaces;
using Editor.Projects;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Components.Explorer
{
    public partial class ExplorerView : UserControl, IEngineComponent
    {
        public string ComponentName => "Explorer";

        public event Action<SceneEntityItem?>? EntitySelected;

        public ExplorerView()
        {
            InitializeComponent();
        }

        public void Initialize() { }

        public void Populate(HxLevel level)
        {
            LevelNameText.Text = string.IsNullOrEmpty(level.Name) ? "Scene" : level.Name;
            EntityTree.ItemsSource = level.Entities
                .Select(SceneEntityItem.FromLevelEntity)
                .ToList();
        }

        private void EntityTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EntitySelected?.Invoke(e.NewValue as SceneEntityItem);
        }
    }
}
