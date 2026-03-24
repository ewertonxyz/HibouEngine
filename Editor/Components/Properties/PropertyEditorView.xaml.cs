using Editor.Components.AssetsBrowser;
using Editor.Components.Explorer;
using Editor.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Components.Properties
{
    public partial class PropertyEditorView : UserControl, IEngineComponent
    {
        public string ComponentName => "PropertyEditor";

        public PropertyEditorView()
        {
            InitializeComponent();
        }

        public void Initialize() { }

        public void ShowEntity(SceneEntityItem? item)
        {
            AssetPropertiesScroll.DataContext = null;
            AssetPropertiesScroll.Visibility = Visibility.Collapsed;

            if (item == null)
            {
                PropertiesScroll.DataContext = null;
                PropertiesScroll.Visibility  = Visibility.Collapsed;
                NoSelectionText.Visibility   = Visibility.Visible;
                return;
            }

            PropertiesScroll.DataContext = item;
            PropertiesScroll.Visibility  = Visibility.Visible;
            NoSelectionText.Visibility   = Visibility.Collapsed;
        }

        public void ShowAsset(AssetItem? item)
        {
            if (item == null)
            {
                AssetPropertiesScroll.DataContext = null;
                AssetPropertiesScroll.Visibility = Visibility.Collapsed;
                if (PropertiesScroll.Visibility == Visibility.Collapsed)
                    NoSelectionText.Visibility = Visibility.Visible;
                return;
            }

            PropertiesScroll.DataContext = null;
            PropertiesScroll.Visibility  = Visibility.Collapsed;
            NoSelectionText.Visibility   = Visibility.Collapsed;
            AssetPropertiesScroll.DataContext = item;
            AssetPropertiesScroll.Visibility  = Visibility.Visible;
        }
    }
}
