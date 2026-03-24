using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Components.AssetsBrowser
{
    public class AssetNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AssetNode> Children { get; } = new();

        public AssetNode(string name, string fullPath)
        {
            _name = name;
            _fullPath = fullPath;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class AssetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set { _fullPath = value; OnPropertyChanged(); }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public bool IsDirectory { get; }
        public bool IsImage { get; }
        public string TypeLabel { get; }
        public Brush IconBackground { get; }
        public ImageSource? Thumbnail { get; }

        public string RelativePath { get; }
        public string AssetId { get; }

        public double FileSizeMB
        {
            get
            {
                if (IsDirectory) return 0;
                try { return new FileInfo(FullPath).Length / 1_048_576.0; }
                catch { return 0; }
            }
        }

        public int ImageWidth
        {
            get
            {
                if (!IsImage) return 0;
                try
                {
                    using var stream = File.OpenRead(FullPath);
                    var decoder = BitmapDecoder.Create(stream,
                        BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.None);
                    return decoder.Frames[0].PixelWidth;
                }
                catch { return 0; }
            }
        }

        public int ImageHeight
        {
            get
            {
                if (!IsImage) return 0;
                try
                {
                    using var stream = File.OpenRead(FullPath);
                    var decoder = BitmapDecoder.Create(stream,
                        BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.None);
                    return decoder.Frames[0].PixelHeight;
                }
                catch { return 0; }
            }
        }

        public string CompressionLabel
        {
            get
            {
                var ext = Path.GetExtension(FullPath).ToLowerInvariant();
                return ext switch
                {
                    ".png"              => "Lossless",
                    ".jpg" or ".jpeg"   => "Lossy/DCT",
                    ".tga" or ".bmp"    => "Uncompressed",
                    ".dds"              => "Block Compressed",
                    ".tif" or ".tiff"   => "Varies",
                    _                   => "N/A"
                };
            }
        }

        public int FileCount
        {
            get
            {
                if (!IsDirectory) return 0;
                try { return Directory.GetFiles(FullPath, "*", SearchOption.AllDirectories).Length; }
                catch { return 0; }
            }
        }

        public double TotalSizeMB
        {
            get
            {
                if (!IsDirectory) return 0;
                try
                {
                    double total = 0;
                    foreach (var f in Directory.GetFiles(FullPath, "*", SearchOption.AllDirectories))
                        total += new FileInfo(f).Length;
                    return total / 1_048_576.0;
                }
                catch { return 0; }
            }
        }

        public bool IsGltf
        {
            get
            {
                var ext = Path.GetExtension(FullPath).ToLowerInvariant();
                return ext is ".gltf" or ".glb";
            }
        }

        public System.Windows.Visibility ImageSectionVisibility  =>
            IsImage     ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility FolderSectionVisibility =>
            IsDirectory ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility GltfSectionVisibility   =>
            IsGltf      ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        private static readonly HashSet<string> ImageExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            { ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".gif", ".tga", ".dds" };

        private static readonly ImageSource? FolderIcon = LoadIconFromDisk(
            @"D:\Dev\HibouEngine\Assets\Icons\folder5.png");

        public AssetItem(string name, string fullPath, bool isDirectory, string rootDirectory = "")
        {
            _name = name;
            _fullPath = fullPath;
            IsDirectory = isDirectory;

            RelativePath = string.IsNullOrEmpty(rootDirectory)
                ? fullPath
                : Path.GetRelativePath(rootDirectory, fullPath);

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(RelativePath));
            AssetId = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();

            if (isDirectory)
            {
                TypeLabel = "DIR";
                IconBackground = MakeBrush("#4A7CC7");
                Thumbnail = FolderIcon;
            }
            else
            {
                var ext = Path.GetExtension(fullPath).ToLowerInvariant();
                IsImage = ImageExtensions.Contains(ext);

                (TypeLabel, IconBackground) = ext switch
                {
                    ".gltf" or ".glb"                         => ("GLTF", MakeBrush("#5A9E6F")),
                    ".png" or ".jpg" or ".jpeg" or ".tga"
                        or ".bmp" or ".tif" or ".tiff"        => ("IMG",  MakeBrush("#C87941")),
                    ".dds"                                    => ("DDS",  MakeBrush("#C87941")),
                    ".hxlevel"                                => ("LVL",  MakeBrush("#7B4FBE")),
                    ".hxproj"                                 => ("PROJ", MakeBrush("#7B4FBE")),
                    ".json"                                   => ("JSON", MakeBrush("#C8A041")),
                    ".hlsl" or ".glsl" or ".vert" or ".frag" => ("SHDR", MakeBrush("#C8516A")),
                    ".cs" or ".cpp" or ".h"                  => ("CODE", MakeBrush("#4A9EC8")),
                    _ => (ext.Length > 1
                            ? ext[1..].ToUpper()[..Math.Min(4, ext.Length - 1)]
                            : "FILE",
                          MakeBrush("#555555"))
                };

                if (IsImage)
                    Thumbnail = LoadThumbnail(fullPath);
            }
        }

        private static ImageSource? LoadThumbnail(string filePath)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(filePath, UriKind.Absolute);
                bmp.DecodePixelWidth = 56;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        private static ImageSource? LoadIconFromDisk(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(filePath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        private static SolidColorBrush MakeBrush(string hex) =>
            new((Color)ColorConverter.ConvertFromString(hex));

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class AssetBrowserViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<AssetNode> RootNodes { get; } = new();
        public ObservableCollection<AssetItem> CurrentItems { get; } = new();

        private readonly List<AssetItem> _allCurrentItems = new();
        private string _rootDirectory = string.Empty;

        private string _currentPath = string.Empty;
        public string CurrentPath
        {
            get => _currentPath;
            private set { _currentPath = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private AssetItem? _selectedAssetItem;
        public AssetItem? SelectedAssetItem
        {
            get => _selectedAssetItem;
            private set
            {
                _selectedAssetItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedImage));
                LoadPreviewForItem(value);
            }
        }

        public bool HasSelectedImage => _selectedAssetItem?.IsImage == true;

        private ImageSource? _previewImage;
        public ImageSource? PreviewImage
        {
            get => _previewImage;
            private set { _previewImage = value; OnPropertyChanged(); }
        }

        private string _previewInfo = string.Empty;
        public string PreviewInfo
        {
            get => _previewInfo;
            private set { _previewInfo = value; OnPropertyChanged(); }
        }

        public void SelectItem(AssetItem? item)
        {
            SelectedAssetItem = item;
        }

        private void LoadPreviewForItem(AssetItem? item)
        {
            if (item == null || !item.IsImage)
            {
                PreviewImage = null;
                PreviewInfo = string.Empty;
                return;
            }

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(item.FullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                PreviewImage = bmp;

                var fi = new FileInfo(item.FullPath);
                var kb = fi.Length / 1024.0;
                var sizeStr = kb >= 1024 ? $"{kb / 1024:F1} MB" : $"{kb:F0} KB";
                PreviewInfo = $"{bmp.PixelWidth} × {bmp.PixelHeight}  ·  {sizeStr}";
            }
            catch
            {
                PreviewImage = null;
                PreviewInfo = string.Empty;
            }
        }

        public void PopulateFromDirectory(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            RootNodes.Clear();

            if (!Directory.Exists(rootDirectory))
            {
                CurrentItems.Clear();
                return;
            }

            var rootNode = new AssetNode(Path.GetFileName(rootDirectory), rootDirectory);
            BuildFolderTree(rootNode, rootDirectory);
            RootNodes.Add(rootNode);

            LoadFolderContents(rootDirectory);
        }

        public void LoadFolderContents(string directory)
        {
            _allCurrentItems.Clear();
            CurrentPath = directory;

            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));

            if (!Directory.Exists(directory))
            {
                CurrentItems.Clear();
                return;
            }

            foreach (var subDir in Directory.GetDirectories(directory))
                _allCurrentItems.Add(new AssetItem(Path.GetFileName(subDir), subDir, isDirectory: true, _rootDirectory));

            foreach (var file in Directory.GetFiles(directory))
                _allCurrentItems.Add(new AssetItem(Path.GetFileName(file), file, isDirectory: false, _rootDirectory));

            ApplyFilter();
        }

        public void BeginRenameItem(AssetItem item)
        {
            item.IsEditing = true;
        }

        public void CommitRename(AssetItem item, string newName)
        {
            newName = newName.Trim();
            if (string.IsNullOrEmpty(newName) || newName == item.Name)
            {
                item.IsEditing = false;
                return;
            }

            var dir = Path.GetDirectoryName(item.FullPath)!;
            var newPath = Path.Combine(dir, newName);
            if (item.IsDirectory ? Directory.Exists(newPath) : File.Exists(newPath))
            {
                item.IsEditing = false;
                return;
            }

            try
            {
                if (item.IsDirectory)
                    Directory.Move(item.FullPath, newPath);
                else
                    File.Move(item.FullPath, newPath);
            }
            catch { item.IsEditing = false; return; }

            var oldPath = item.FullPath;
            item.Name = newName;
            item.FullPath = newPath;
            item.IsEditing = false;

            UpdateNodePath(RootNodes, oldPath, newPath, newName);
        }

        public void CancelRename(AssetItem item)
        {
            item.IsEditing = false;
        }

        private static void UpdateNodePath(IEnumerable<AssetNode> nodes, string oldPath, string newPath, string newName)
        {
            foreach (var node in nodes)
            {
                if (node.FullPath == oldPath)
                {
                    node.FullPath = newPath;
                    node.Name = newName;
                    return;
                }
                UpdateNodePath(node.Children, oldPath, newPath, newName);
            }
        }

        private void ApplyFilter()
        {
            CurrentItems.Clear();
            var query = _searchText.Trim();

            if (string.IsNullOrEmpty(query))
            {
                foreach (var item in _allCurrentItems)
                    CurrentItems.Add(item);
                return;
            }

            if (Directory.Exists(_rootDirectory))
                SearchRecursive(_rootDirectory, query);
        }

        private void SearchRecursive(string directory, string query)
        {
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var name = Path.GetFileName(subDir);
                if (name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    CurrentItems.Add(new AssetItem(name, subDir, isDirectory: true, _rootDirectory));
                SearchRecursive(subDir, query);
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    CurrentItems.Add(new AssetItem(name, file, isDirectory: false, _rootDirectory));
            }
        }

        private static void BuildFolderTree(AssetNode parent, string directory)
        {
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var node = new AssetNode(Path.GetFileName(subDir), subDir);
                BuildFolderTree(node, subDir);
                parent.Children.Add(node);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
