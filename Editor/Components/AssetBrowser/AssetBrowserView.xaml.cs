using Editor.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Editor.Components.AssetsBrowser
{
    public partial class AssetBrowserView : UserControl, IEngineComponent
    {
        public string ComponentName => "AssetBrowser";

        private readonly AssetBrowserViewModel _viewModel = new();

        public event Action<AssetItem?>? AssetSelected;

        public AssetBrowserView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        public void Initialize() { }

        public void Populate(string projectDirectory)
        {
            _viewModel.PopulateFromDirectory(projectDirectory);
        }

        private void AssetTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AssetNode node)
            {
                _viewModel.LoadFolderContents(node.FullPath);
                _viewModel.SelectItem(null);
            }
        }

        private void AssetGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = AssetGrid.SelectedItem as AssetItem;
            _viewModel.SelectItem(item);
            AssetSelected?.Invoke(item);
        }

        private void AssetGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssetGrid.SelectedItem is AssetItem item && item.IsDirectory)
            {
                _viewModel.LoadFolderContents(item.FullPath);
                _viewModel.SelectItem(null);
            }
        }

        private void AssetGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2 && AssetGrid.SelectedItem is AssetItem item)
            {
                BeginRenameAndFocus(item);
                e.Handled = true;
            }
        }

        private void AssetGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject? dep = e.OriginalSource as DependencyObject;
            while (dep != null)
            {
                if (dep is ListBoxItem lbi)
                {
                    lbi.IsSelected = true;
                    break;
                }
                dep = VisualTreeHelper.GetParent(dep);
            }
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi
                && mi.Parent is ContextMenu menu
                && menu.PlacementTarget is FrameworkElement card
                && card.DataContext is AssetItem item)
            {
                BeginRenameAndFocus(item);
            }
        }

        private void ShowProperties_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi
                && mi.Parent is ContextMenu menu
                && menu.PlacementTarget is FrameworkElement card
                && card.DataContext is AssetItem item)
            {
                AssetSelected?.Invoke(item);
            }
        }

        private void NameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            var item = GetAssetItemFromTextBox(tb);
            if (item == null || !item.IsEditing) return;
            _viewModel.CommitRename(item, tb.Text);
        }

        private void NameEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb) return;
            var item = GetAssetItemFromTextBox(tb);
            if (item == null) return;

            if (e.Key == Key.Enter)
            {
                _viewModel.CommitRename(item, tb.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.CancelRename(item);
                e.Handled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = btn;
                menu.Placement = PlacementMode.Bottom;
                menu.IsOpen = true;
            }
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            var basePath = _viewModel.CurrentPath;
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                return;

            var name = "New Folder";
            var target = Path.Combine(basePath, name);
            var counter = 1;
            while (Directory.Exists(target))
                target = Path.Combine(basePath, $"{name} ({counter++})");

            Directory.CreateDirectory(target);
            _viewModel.LoadFolderContents(basePath);

            var newName = Path.GetFileName(target);
            var newItem = _viewModel.CurrentItems.FirstOrDefault(i => i.IsDirectory && i.Name == newName);
            if (newItem != null)
                BeginRenameAndFocus(newItem);
        }

        private void ImportAsset_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Import Asset is not yet implemented.", "Import Asset",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewLevel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("New Level is not yet implemented.", "New Level",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BeginRenameAndFocus(AssetItem item)
        {
            _viewModel.BeginRenameItem(item);
            AssetGrid.SelectedItem = item;
            AssetGrid.ScrollIntoView(item);

            Dispatcher.InvokeAsync(() =>
            {
                var container = AssetGrid.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container == null) return;
                var tb = FindVisualChild<TextBox>(container);
                if (tb == null) return;
                tb.Text = item.Name;
                tb.Focus();
                tb.SelectAll();
            }, DispatcherPriority.Loaded);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T target) return target;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static AssetItem? GetAssetItemFromTextBox(TextBox tb)
        {
            DependencyObject? current = tb;
            while (current != null)
            {
                if (current is ListBoxItem lbi)
                    return lbi.DataContext as AssetItem;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
