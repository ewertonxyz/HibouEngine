using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Editor.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        public ObservableCollection<NodeViewModel> Nodes { get; }

        public EditorViewModel()
        {
            Nodes = new ObservableCollection<NodeViewModel>();

            Nodes.Add(new NodeViewModel
            {
                NodeTitle = "HairCard Generator",
                NodeLocation = new Point(150, 200)
            });

            Nodes.Add(new NodeViewModel
            {
                NodeTitle = "Material Output",
                NodeLocation = new Point(500, 200)
            });
        }
    }
}