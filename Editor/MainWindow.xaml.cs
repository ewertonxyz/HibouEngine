using System.Windows;
using Editor.Components.Viewport.OpenGL;

namespace Editor
{
    public partial class MainWindow : Window
    {
        private OpenGLViewportView? _viewportView;

        public MainWindow()
        {
            InitializeComponent();
            SceneExplorer.EntitySelected += SceneProperties.ShowEntity;
            AssetBrowser.AssetSelected  += SceneProperties.ShowAsset;
        }

        /// <summary>
        /// Creates the OpenGL viewport and initializes the engine.
        /// Must be called after Show() returns so AvalonDock layout is stable
        /// and the HwndHost can safely create its native window.
        /// </summary>
        public void InitializeViewport()
        {
            _viewportView = new OpenGLViewportView();
            ViewportHost.Content = _viewportView;

            // Force a synchronous layout pass so BuildWindowCore runs
            // and the native HWND is created before we init the engine.
            ViewportHost.UpdateLayout();

            _viewportView.InitializeEngine();
        }
    }
}