using Editor.Interop;
using Editor.Projects;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Editor.Components.Lobby
{
    public partial class LobbyView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // ── Open-tab state ─────────────────────────────────────────────────────

        public ObservableCollection<ProjectCard> Projects { get; } = new();

        private ProjectCard? _selectedProject;
        public ProjectCard? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (!ReferenceEquals(_selectedProject, value))
                {
                    _selectedProject = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanOpenSelected));
                }
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
        }

        public string ProjectsRootPath { get; }
        public bool CanOpenSelected => SelectedProject != null;

        // ── View switching ─────────────────────────────────────────────────────

        private bool _showNewProjectView;
        public Visibility ProjectsViewVisibility  => _showNewProjectView ? Visibility.Collapsed : Visibility.Visible;
        public Visibility NewProjectViewVisibility => _showNewProjectView ? Visibility.Visible   : Visibility.Collapsed;

        // ── Create-tab state ───────────────────────────────────────────────────

        private string _createProjectName = string.Empty;
        public string CreateProjectName
        {
            get => _createProjectName;
            set
            {
                if (_createProjectName != value)
                {
                    _createProjectName = value;
                    OnPropertyChanged();
                    UpdateCreateFolderName();
                }
            }
        }

        private string _createProjectDescription = string.Empty;
        public string CreateProjectDescription
        {
            get => _createProjectDescription;
            set { if (_createProjectDescription != value) { _createProjectDescription = value; OnPropertyChanged(); } }
        }

        // Logo
        private string _createLogoPath = string.Empty;
        private ImageSource? _createLogoPreview;
        private string _createLogoError = string.Empty;

        public ImageSource? CreateLogoPreview
        {
            get => _createLogoPreview;
            set { if (!ReferenceEquals(_createLogoPreview, value)) { _createLogoPreview = value; OnPropertyChanged(); } }
        }

        public string CreateLogoError
        {
            get => _createLogoError;
            set
            {
                if (_createLogoError != value)
                {
                    _createLogoError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreateLogoErrorVisibility));
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public Visibility CreateLogoErrorVisibility =>
            string.IsNullOrEmpty(_createLogoError) ? Visibility.Collapsed : Visibility.Visible;

        // Splash
        private string _createSplashPath = string.Empty;
        private ImageSource? _createSplashPreview;
        private string _createSplashError = string.Empty;

        public ImageSource? CreateSplashPreview
        {
            get => _createSplashPreview;
            set { if (!ReferenceEquals(_createSplashPreview, value)) { _createSplashPreview = value; OnPropertyChanged(); } }
        }

        public string CreateSplashError
        {
            get => _createSplashError;
            set
            {
                if (_createSplashError != value)
                {
                    _createSplashError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreateSplashErrorVisibility));
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public Visibility CreateSplashErrorVisibility =>
            string.IsNullOrEmpty(_createSplashError) ? Visibility.Collapsed : Visibility.Visible;

        // Name validation
        private string _createNameError = string.Empty;
        public string CreateNameError
        {
            get => _createNameError;
            set
            {
                if (_createNameError != value)
                {
                    _createNameError = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CreateNameErrorVisibility));
                    OnPropertyChanged(nameof(CreateNameBorderBrush));
                    OnPropertyChanged(nameof(CanCreate));
                }
            }
        }

        public Visibility CreateNameErrorVisibility =>
            string.IsNullOrEmpty(_createNameError) ? Visibility.Collapsed : Visibility.Visible;

        public Brush CreateNameBorderBrush =>
            string.IsNullOrEmpty(_createNameError)
                ? new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C))
                : new SolidColorBrush(Colors.Red);

        private string _createFolderName = string.Empty;
        public string CreateFolderName
        {
            get => _createFolderName;
            set { if (_createFolderName != value) { _createFolderName = value; OnPropertyChanged(); } }
        }

        private bool _isCreatingProject;

        public bool CanCreate =>
            !string.IsNullOrWhiteSpace(_createProjectName) &&
            string.IsNullOrEmpty(_createNameError) &&
            string.IsNullOrEmpty(_createLogoError) &&
            string.IsNullOrEmpty(_createSplashError) &&
            !_isCreatingProject;

        // ── Constructor ────────────────────────────────────────────────────────

        public LobbyView()
        {
            InitializeComponent();
            DataContext = this;

            ProjectsRootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Hibou Projects");

            InitializeCreateDefaults();
            LoadProjects();
        }

        private void InitializeCreateDefaults()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var logoPath = Path.Combine(baseDir, "Assets", "Icons", "logo.png");
            if (File.Exists(logoPath))
            {
                _createLogoPath = logoPath;
                CreateLogoPreview = LoadDiskImage(logoPath);
            }

            var splashPath = Path.Combine(baseDir, "Assets", "Icons", "splash.png");
            if (File.Exists(splashPath))
            {
                _createSplashPath = splashPath;
                CreateSplashPreview = LoadDiskImage(splashPath);
            }
        }

        private void UpdateCreateFolderName()
        {
            var folder = ProjectCreator.SanitizeFolderName(_createProjectName);
            CreateFolderName = folder;

            if (string.IsNullOrWhiteSpace(_createProjectName) || string.IsNullOrEmpty(folder))
                CreateNameError = string.Empty;
            else if (ProjectCreator.ProjectExists(ProjectsRootPath, folder))
                CreateNameError = $"A project named \"{folder}\" already exists.";
            else
                CreateNameError = string.Empty;

            OnPropertyChanged(nameof(CanCreate));
        }

        // ── Create tab handlers ────────────────────────────────────────────────

        private void BrowseLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Logo",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.gif"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var decoder = BitmapDecoder.Create(
                    new Uri(dlg.FileName, UriKind.Absolute),
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                var frame = decoder.Frames[0];
                int w = frame.PixelWidth;
                int h = frame.PixelHeight;

                if (w != h)
                {
                    CreateLogoError = $"Must be square. Got {w}×{h}.";
                    return;
                }

                var validSizes = new[] { 128, 256, 512, 1024 };
                if (!Array.Exists(validSizes, s => s == w))
                {
                    CreateLogoError = $"Must be 128, 256, 512 or 1024 px. Got {w}×{h}.";
                    return;
                }

                CreateLogoError = string.Empty;
                _createLogoPath = dlg.FileName;
                CreateLogoPreview = LoadDiskImage(dlg.FileName);
            }
            catch (Exception ex)
            {
                CreateLogoError = $"Could not read image: {ex.Message}";
            }
        }

        private void BrowseSplash_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select Splash Screen",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.gif"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var decoder = BitmapDecoder.Create(
                    new Uri(dlg.FileName, UriKind.Absolute),
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                var frame = decoder.Frames[0];
                int w = frame.PixelWidth;
                int h = frame.PixelHeight;

                if (w != 1280 || h != 720)
                {
                    CreateSplashError = $"Must be 1280×720. Got {w}×{h}.";
                    return;
                }

                CreateSplashError = string.Empty;
                _createSplashPath = dlg.FileName;
                CreateSplashPreview = LoadDiskImage(dlg.FileName);
            }
            catch (Exception ex)
            {
                CreateSplashError = $"Could not read image: {ex.Message}";
            }
        }

        private async void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            _isCreatingProject = true;
            OnPropertyChanged(nameof(CanCreate));

            var splash = new SplashLoadingWindow(_createSplashPath);
            splash.Owner = Window.GetWindow(this);
            splash.UpdateProgress(0, "Initializing...");
            splash.Show();

            var request = new CreateProjectRequest
            {
                DisplayName = _createProjectName,
                FolderName = _createFolderName,
                Description = _createProjectDescription,
                ProjectsRoot = ProjectsRootPath,
                SourceLogoPath = _createLogoPath,
                SourceSplashPath = _createSplashPath
            };

            var progress = new Progress<int>(v =>
            {
                splash.UpdateProgress(v, v switch
                {
                    <= 10 => "Validating...",
                    <= 25 => "Creating directories...",
                    <= 50 => "Processing images...",
                    <= 65 => "Copying splash screen...",
                    <= 75 => "Writing project file...",
                    <= 88 => "Writing level file...",
                    _ => "Finalizing..."
                });
            });

            CreateProjectResult result;
            try
            {
                result = await ProjectCreator.CreateAsync(request, progress);
            }
            catch (Exception ex)
            {
                result = new CreateProjectResult { ErrorMessage = $"Unexpected error: {ex.Message}" };
            }

            _isCreatingProject = false;
            OnPropertyChanged(nameof(CanCreate));

            if (result.Success)
            {
                splash.UpdateProgress(100, "Launching editor...");
                await LaunchEditorAsync(result.ProjectFilePath, splash);
            }
            else
            {
                splash.Close();
                MessageBox.Show(result.ErrorMessage, "Create Project", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Project opening ────────────────────────────────────────────────────

        private void OpenSelectedProject()
        {
            if (SelectedProject == null) return;
            OpenExistingProjectAsync(SelectedProject.ProjectFilePath);
        }

        private async void OpenExistingProjectAsync(string hxprojPath)
        {
            HxProject project;
            try
            {
                project = ProjectLoader.Load(hxprojPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project: {ex.Message}", "Open Project",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var splash = new SplashLoadingWindow(project.GetResolvedSplashPath());
            splash.Owner = Window.GetWindow(this);
            splash.UpdateProgress(0, "Loading project...");
            splash.Show();

            ProjectContext.Current = project;

            splash.UpdateProgress(20, "Initializing editor...");
            await Task.Yield();

            await LaunchEditorAsync(hxprojPath, splash, project, showStages: true, startPercent: 20);
        }

        /// <summary>
        /// Launches the editor. If <paramref name="project"/> is provided it skips reloading.
        /// The splash window is closed inside this method on success.
        /// </summary>
        private async Task LaunchEditorAsync(
            string hxprojPath,
            SplashLoadingWindow splash,
            HxProject? project = null,
            bool showStages = false,
            int startPercent = 100)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[HibouEngine] LaunchEditorAsync START — hxprojPath={hxprojPath}");

                if (project == null)
                {
                    project = ProjectLoader.Load(hxprojPath);
                    ProjectContext.Current = project;
                }

                System.Diagnostics.Debug.WriteLine($"[HibouEngine] Project loaded: {project.Name}, DefaultLevel={project.DefaultLevel}");
                System.Diagnostics.Debug.WriteLine($"[HibouEngine] ProjectDirectory={project.ProjectDirectory}");
                System.Diagnostics.Debug.WriteLine($"[HibouEngine] DefaultLevelPath={project.GetDefaultLevelPath()}");

                if (showStages)
                {
                    splash.UpdateProgress(startPercent + 10, "Creating workspace...");
                    await Task.Yield();
                }

                var main = new MainWindow();

                if (showStages)
                {
                    splash.UpdateProgress(startPercent + 25, "Initializing viewport...");
                    await Task.Yield();
                }

                // Show first: BuildWindowCore runs here, which calls InitializeGameEngine.
                // Everything that touches the engine must come after this line.
                main.Show();
                Application.Current.MainWindow = main;

                if (showStages)
                {
                    splash.UpdateProgress(startPercent + 40, "Loading assets...");
                    await Task.Yield();
                }

                main.AssetBrowser.Populate(project.ProjectDirectory);

                splash.UpdateProgress(100, "Ready!");
                await Task.Delay(400);

                // Close splash then lobby; LoadDefaultLevel after the engine is live
                splash.Close();
                Window.GetWindow(this)?.Close();

                System.Diagnostics.Debug.WriteLine("[HibouEngine] About to call LoadDefaultLevel...");
                LoadDefaultLevel(project, main);
                System.Diagnostics.Debug.WriteLine("[HibouEngine] LoadDefaultLevel returned successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HibouEngine] EXCEPTION in LaunchEditorAsync: {ex}");
                try { splash.Close(); } catch { /* already closed */ }
                var msg = new System.Text.StringBuilder();
                var inner = ex;
                while (inner != null)
                {
                    msg.AppendLine(inner.GetType().Name + ": " + inner.Message);
                    inner = inner.InnerException;
                    if (inner != null) msg.AppendLine("  →");
                }
                MessageBox.Show($"Failed to open project.\n\n{msg}", "Open Project",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Infrastructure ─────────────────────────────────────────────────────

        private void LoadProjects()
        {
            Projects.Clear();

            if (!Directory.Exists(ProjectsRootPath))
            {
                StatusText = "Projects folder not found.";
                return;
            }

            foreach (var subDir in Directory.GetDirectories(ProjectsRootPath))
            {
                var hxprojFiles = Directory.GetFiles(subDir, "*.hxproj");
                if (hxprojFiles.Length == 0) continue;

                var card = ProjectCard.FromProjectFile(hxprojFiles[0]);
                if (card.Icon == null)
                    card.Icon = LoadPackImage("pack://application:,,,/Assets/Icons/logo.png");

                Projects.Add(card);
            }

            StatusText = $"Found {Projects.Count} project(s)";
        }

        private static void LoadDefaultLevel(HxProject project, MainWindow main)
        {
            var levelPath = project.GetDefaultLevelPath();
            if (string.IsNullOrEmpty(levelPath) || !File.Exists(levelPath)) return;

            System.Diagnostics.Debug.WriteLine($"[HibouEngine] LoadDefaultLevel: {levelPath}");

            var level = LevelLoader.Load(levelPath, project.ProjectDirectory);

            System.Diagnostics.Debug.WriteLine($"[HibouEngine] Level '{level.Name}' has {level.Entities.Count} entities");

            main.SceneExplorer.Populate(level);
            EngineBindings.ClearScene();

            foreach (var entity in level.Entities)
            {
                System.Diagnostics.Debug.WriteLine($"[HibouEngine] Entity '{entity.Name}' — PrimaryType={entity.PrimaryComponentType}, Components=[{string.Join(", ", entity.Components.Select(c => c.Type))}]");

                var tf = entity.GetComponent<HxTransformComponent>();
                float px = tf != null && tf.Position.Length > 0 ? tf.Position[0] : 0f;
                float py = tf != null && tf.Position.Length > 1 ? tf.Position[1] : 0f;
                float pz = tf != null && tf.Position.Length > 2 ? tf.Position[2] : 0f;
                float rx = tf != null && tf.RotationEulerDeg.Length > 0 ? tf.RotationEulerDeg[0] : 0f;
                float ry = tf != null && tf.RotationEulerDeg.Length > 1 ? tf.RotationEulerDeg[1] : 0f;
                float rz = tf != null && tf.RotationEulerDeg.Length > 2 ? tf.RotationEulerDeg[2] : 0f;
                float sx = tf != null && tf.Scale.Length > 0 ? tf.Scale[0] : 1f;
                float sy = tf != null && tf.Scale.Length > 1 ? tf.Scale[1] : 1f;
                float sz = tf != null && tf.Scale.Length > 2 ? tf.Scale[2] : 1f;

                switch (entity.PrimaryComponentType)
                {
                    case "GltfModel":
                        var gltf = entity.GetComponent<HxGltfModelComponent>();
                        System.Diagnostics.Debug.WriteLine($"[HibouEngine]   GltfModel: AssetRef={gltf?.AssetRef}, ResolvedPath={gltf?.ResolvedPath}");
                        if (gltf != null && !string.IsNullOrEmpty(gltf.ResolvedPath))
                            EngineBindings.LoadGltfEntity(entity.Name, gltf.ResolvedPath,
                                px, py, pz, rx, ry, rz, sx, sy, sz);
                        break;
                    case "Camera":
                        var cam = entity.GetComponent<HxCameraComponent>();
                        System.Diagnostics.Debug.WriteLine($"[HibouEngine]   Camera: FOV={cam?.FovDeg}, Near={cam?.Near}, Far={cam?.Far}");
                        EngineBindings.LoadCameraEntity(entity.Name,
                            px, py, pz, rx, ry, rz,
                            cam?.FovDeg ?? 60f, cam?.Near ?? 0.1f, cam?.Far ?? 1000f);
                        break;
                    case "DirectionalLight":
                        var light = entity.GetComponent<HxDirectionalLightComponent>();
                        float lr = light != null && light.Color.Length > 0 ? light.Color[0] : 1f;
                        float lg = light != null && light.Color.Length > 1 ? light.Color[1] : 1f;
                        float lb = light != null && light.Color.Length > 2 ? light.Color[2] : 1f;
                        System.Diagnostics.Debug.WriteLine($"[HibouEngine]   DirLight: Color=({lr},{lg},{lb}), Intensity={light?.IntensityLux}");
                        EngineBindings.LoadDirectionalLightEntity(entity.Name,
                            px, py, pz, rx, ry, rz,
                            lr, lg, lb, light?.IntensityLux ?? 1f);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"[HibouEngine]   Unknown component type: {entity.PrimaryComponentType}");
                        break;
                }
            }

            System.Diagnostics.Debug.WriteLine("[HibouEngine] LoadDefaultLevel complete");
        }

        private static ImageSource? LoadDiskImage(string filePath)
        {
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

        private static ImageSource? LoadPackImage(string packUri)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(packUri, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }

        private void OpenSelected_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProject == null) return;
            OpenSelectedProject();
        }

        private void Projects_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedProject == null) return;
            OpenSelectedProject();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadProjects();

        private void ShowNewProject_Click(object sender, RoutedEventArgs e)
        {
            _showNewProjectView = true;
            OnPropertyChanged(nameof(ProjectsViewVisibility));
            OnPropertyChanged(nameof(NewProjectViewVisibility));
        }

        private void BackToProjects_Click(object sender, RoutedEventArgs e)
        {
            _showNewProjectView = false;
            OnPropertyChanged(nameof(ProjectsViewVisibility));
            OnPropertyChanged(nameof(NewProjectViewVisibility));
        }

        private void OpenCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProjectCard card)
                OpenExistingProjectAsync(card.ProjectFilePath);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public sealed class ProjectCard
    {
        public string Name { get; set; } = "Unknown Project";
        public string Description { get; set; } = string.Empty;
        public string ProjectFilePath { get; set; } = string.Empty;
        public string ContentRootLabel { get; set; } = "Depot";
        public ImageSource? Icon { get; set; }

        public static ProjectCard FromProjectFile(string hxprojPath)
        {
            var card = new ProjectCard { ProjectFilePath = hxprojPath };

            if (!File.Exists(hxprojPath))
            {
                card.Name = Path.GetFileNameWithoutExtension(hxprojPath);
                card.Description = "Project file not found.";
                return card;
            }

            try
            {
                var project = ProjectLoader.Load(hxprojPath);
                card.Name = project.Name;
                card.Description = string.IsNullOrWhiteSpace(project.Description) ? "No description." : project.Description;
                card.ContentRootLabel = project.ContentRootName;
                var icon = project.GetResolvedIconPath();
                if (!string.IsNullOrWhiteSpace(icon) && File.Exists(icon))
                    card.Icon = LoadDiskImage(icon);
            }
            catch
            {
                card.Name = Path.GetFileNameWithoutExtension(hxprojPath);
                card.Description = "Could not parse project file.";
            }

            return card;
        }

        private static ImageSource? LoadDiskImage(string filePath)
        {
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
    }
}
