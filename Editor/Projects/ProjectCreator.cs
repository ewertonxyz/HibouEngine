using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Projects
{
    public sealed class CreateProjectRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProjectsRoot { get; set; } = string.Empty;
        public string SourceLogoPath { get; set; } = string.Empty;
        public string SourceSplashPath { get; set; } = string.Empty;
    }

    public sealed class CreateProjectResult
    {
        public bool Success { get; set; }
        public string ProjectFilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class ProjectCreator
    {
        public static string SanitizeFolderName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return string.Empty;

            var normalized = rawName.Replace('-', ' ').Replace('_', ' ');
            var tokens = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new System.Text.StringBuilder();
            foreach (var token in tokens)
            {
                var pascal = char.ToUpper(token[0]) + (token.Length > 1 ? token[1..].ToLower() : string.Empty);
                foreach (var c in pascal)
                {
                    if (char.IsLetterOrDigit(c))
                        result.Append(c);
                }
            }

            return result.ToString();
        }

        public static bool ProjectExists(string projectsRoot, string folderName)
        {
            if (string.IsNullOrWhiteSpace(projectsRoot) || string.IsNullOrWhiteSpace(folderName))
                return false;
            return Directory.Exists(Path.Combine(projectsRoot, folderName));
        }

        public static string GenerateHxprojJson(CreateProjectRequest req)
        {
            var projectRoot = Path.Combine(req.ProjectsRoot, req.FolderName).Replace('\\', '/');
            var obj = new
            {
                hxprojVersion = 1,
                name = req.DisplayName,
                description = req.Description,
                logoPath = "Settings/Images/logo.png",
                iconPath = "Settings/Images/icon.png",
                splashPath = "Settings/Images/splash.png",
                projectRoot,
                contentRoot = "Depot",
                levelsRoot = "Levels",
                sourceRoot = "SourceCode",
                settingsRoot = "Settings",
                defaultLevel = "Levels/Sample.hxlevel",
                mounts = new[] { new { name = "Depot", path = projectRoot + "/Depot" } },
                settings = new
                {
                    assetCache = new { enabled = true, path = "Intermediate/AssetCache" },
                    build = new { outputPath = "Build" }
                }
            };

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public static string GenerateHxlevelJson(string projectRoot)
        {
            var cameraGuid = Guid.NewGuid().ToString();
            var lightGuid = Guid.NewGuid().ToString();

            var obj = new
            {
                hxlevelVersion = 1,
                name = "Sample",
                projectRoot = projectRoot.Replace('\\', '/'),
                contentRoot = "Depot",
                world = new
                {
                    units = "meters",
                    upAxis = "Y",
                    handedness = "Left"
                },
                entities = new object[]
                {
                    new
                    {
                        id = cameraGuid,
                        name = "MainCamera",
                        components = new
                        {
                            Transform = new
                            {
                                position = new float[] { 0f, 150f, -350f },
                                rotationEulerDeg = new float[] { -10f, 0f, 0f },
                                scale = new float[] { 1f, 1f, 1f }
                            },
                            Camera = new
                            {
                                projection = "Perspective",
                                fovDeg = 60f,
                                near = 0.05f,
                                far = 5000f,
                                exposure = new
                                {
                                    mode = "Manual",
                                    ev100 = 12.0f
                                }
                            }
                        }
                    },
                    new
                    {
                        id = lightGuid,
                        name = "DirectionalLight",
                        components = new
                        {
                            Transform = new
                            {
                                position = new float[] { 0f, 0f, 0f },
                                rotationEulerDeg = new float[] { -45f, 45f, 0f },
                                scale = new float[] { 1f, 1f, 1f }
                            },
                            DirectionalLight = new
                            {
                                color = new float[] { 1f, 0.98f, 0.92f },
                                intensityLux = 1f,
                                castsShadows = true,
                                shadow = new
                                {
                                    enabled = true,
                                    resolution = 2048,
                                    cascadeCount = 1
                                }
                            }
                        }
                    }
                }
            };

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        public static async Task<CreateProjectResult> CreateAsync(CreateProjectRequest req, IProgress<int> progress)
        {
            if (string.IsNullOrWhiteSpace(req.DisplayName))
                return new CreateProjectResult { ErrorMessage = "Project name cannot be empty." };

            if (string.IsNullOrWhiteSpace(req.FolderName))
                return new CreateProjectResult { ErrorMessage = "Invalid folder name." };

            if (string.IsNullOrWhiteSpace(req.SourceLogoPath) || !File.Exists(req.SourceLogoPath))
                return new CreateProjectResult { ErrorMessage = "Logo file not found." };

            if (string.IsNullOrWhiteSpace(req.SourceSplashPath) || !File.Exists(req.SourceSplashPath))
                return new CreateProjectResult { ErrorMessage = "Splash screen file not found." };

            var projectRoot = Path.Combine(req.ProjectsRoot, req.FolderName);

            if (Directory.Exists(projectRoot))
                return new CreateProjectResult { ErrorMessage = $"A project named \"{req.FolderName}\" already exists." };

            progress.Report(10);

            await Task.Run(() =>
            {
                Directory.CreateDirectory(projectRoot);
                Directory.CreateDirectory(Path.Combine(projectRoot, "Depot"));
                Directory.CreateDirectory(Path.Combine(projectRoot, "Levels"));
                Directory.CreateDirectory(Path.Combine(projectRoot, "Settings"));
                Directory.CreateDirectory(Path.Combine(projectRoot, "Settings", "Images"));
                Directory.CreateDirectory(Path.Combine(projectRoot, "SourceCode"));
            });
            progress.Report(25);

            var logoDestPath = Path.Combine(projectRoot, "Settings", "Images", "logo.png");
            var iconDestPath = Path.Combine(projectRoot, "Settings", "Images", "icon.png");

            try
            {
                var srcBitmap = new BitmapImage();
                srcBitmap.BeginInit();
                srcBitmap.UriSource = new Uri(req.SourceLogoPath, UriKind.Absolute);
                srcBitmap.CacheOption = BitmapCacheOption.OnLoad;
                srcBitmap.EndInit();
                srcBitmap.Freeze();

                var logoBytes = EncodeBitmapAsPng(srcBitmap);

                byte[] iconBytes;
                if (srcBitmap.PixelWidth > 128 || srcBitmap.PixelHeight > 128)
                {
                    var vis = new DrawingVisual();
                    using (var dc = vis.RenderOpen())
                        dc.DrawImage(srcBitmap, new Rect(0, 0, 128, 128));

                    var rtb = new RenderTargetBitmap(128, 128, 96, 96, PixelFormats.Pbgra32);
                    rtb.Render(vis);
                    rtb.Freeze();

                    iconBytes = EncodeBitmapAsPng(rtb);
                }
                else
                {
                    iconBytes = logoBytes;
                }

                await Task.Run(() =>
                {
                    File.WriteAllBytes(logoDestPath, logoBytes);
                    File.WriteAllBytes(iconDestPath, iconBytes);
                });
            }
            catch (Exception ex)
            {
                return new CreateProjectResult { ErrorMessage = $"Failed to process logo: {ex.Message}" };
            }

            progress.Report(50);

            var splashDestPath = Path.Combine(projectRoot, "Settings", "Images", "splash.png");
            try
            {
                await Task.Run(() => File.Copy(req.SourceSplashPath, splashDestPath, overwrite: true));
            }
            catch (Exception ex)
            {
                return new CreateProjectResult { ErrorMessage = $"Failed to copy splash screen: {ex.Message}" };
            }
            progress.Report(65);

            var hxprojPath = Path.Combine(projectRoot, req.FolderName + ".hxproj");
            await Task.Run(() => File.WriteAllText(hxprojPath, GenerateHxprojJson(req)));
            progress.Report(75);

            var levelPath = Path.Combine(projectRoot, "Levels", "Sample.hxlevel");
            await Task.Run(() => File.WriteAllText(levelPath, GenerateHxlevelJson(projectRoot)));
            progress.Report(88);

            var verified = await Task.Run(() =>
                File.Exists(hxprojPath) && File.Exists(levelPath) && File.Exists(logoDestPath) && File.Exists(splashDestPath));

            if (!verified)
                return new CreateProjectResult { ErrorMessage = "Project files could not be verified after creation." };

            progress.Report(100);
            return new CreateProjectResult { Success = true, ProjectFilePath = hxprojPath };
        }

        private static byte[] EncodeBitmapAsPng(BitmapSource bitmap)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
    }
}
