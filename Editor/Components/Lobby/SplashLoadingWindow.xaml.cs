using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Editor.Components.Lobby
{
    public partial class SplashLoadingWindow : Window
    {
        public SplashLoadingWindow(string? splashImagePath)
        {
            InitializeComponent();
            LoadSplashImage(splashImagePath);
            LoadBrandLogo();
        }

        public void UpdateProgress(int percent, string message)
        {
            StatusTextBlock.Text = message;
            PercentTextBlock.Text = $"{percent}%";
        }

        private void LoadSplashImage(string? path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                SplashImage.Source = bmp;
            }
            catch { }
        }

        private void LoadBrandLogo()
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "logo.png");
            if (!File.Exists(logoPath))
                return;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(logoPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                BrandLogo.Source = bmp;
            }
            catch { }
        }
    }
}
