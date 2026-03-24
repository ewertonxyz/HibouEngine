using System.Windows;

namespace Editor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SceneExplorer.EntitySelected += SceneProperties.ShowEntity;
            AssetBrowser.AssetSelected  += SceneProperties.ShowAsset;
        }
    }
}