using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace Editor.ViewModels
{
    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nodeTitle;

        [ObservableProperty]
        private Point nodeLocation;

        public NodeViewModel()
        {
            nodeTitle = "Base Node";
            nodeLocation = new Point(0, 0);
        }
    }
}