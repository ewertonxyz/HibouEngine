using System;
using System.IO;
using System.Text.Json;

namespace Editor.Projects
{
    public static class ProjectLoader
    {
        private sealed class HxProjectJson
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? IconPath { get; set; }
            public string? SplashPath { get; set; }
            public string? ContentRoot { get; set; }
            public string? LevelsRoot { get; set; }
            public string? SourceRoot { get; set; }
            public string? DefaultLevel { get; set; }
        }

        public static HxProject Load(string hxprojPath)
        {
            if (string.IsNullOrWhiteSpace(hxprojPath))
                throw new ArgumentException("Invalid project file path.", nameof(hxprojPath));

            if (!File.Exists(hxprojPath))
                throw new FileNotFoundException("Project file not found.", hxprojPath);

            var projectDir = Path.GetDirectoryName(hxprojPath) ?? string.Empty;
            var jsonText = File.ReadAllText(hxprojPath);

            HxProjectJson? dto = null;

            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                try
                {
                    dto = JsonSerializer.Deserialize<HxProjectJson>(jsonText, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    System.Diagnostics.Debug.WriteLine($"[HibouEngine] ProjectLoader: Deserialized OK — Name={dto?.Name}, DefaultLevel={dto?.DefaultLevel}, ContentRoot={dto?.ContentRoot}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[HibouEngine] ProjectLoader: Deserialization FAILED — {ex.GetType().Name}: {ex.Message}");
                    dto = null;
                }
            }

            var name = dto?.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = Path.GetFileNameWithoutExtension(hxprojPath);

            var description = dto?.Description ?? string.Empty;

            var contentRoot = dto?.ContentRoot;
            if (string.IsNullOrWhiteSpace(contentRoot))
                contentRoot = "Depot";

            var levelsRoot = dto?.LevelsRoot;
            if (string.IsNullOrWhiteSpace(levelsRoot))
                levelsRoot = "Levels";

            var sourceRoot = dto?.SourceRoot;
            if (string.IsNullOrWhiteSpace(sourceRoot))
                sourceRoot = "SourceCode";

            var iconPath = dto?.IconPath ?? string.Empty;
            var defaultLevel = dto?.DefaultLevel ?? string.Empty;

            var project = new HxProject(
                name: name,
                description: description,
                projectFilePath: hxprojPath,
                iconPath: iconPath,
                contentRootName: ResolveContentRootWithFallback(projectDir, contentRoot),
                levelsRootName: levelsRoot,
                sourceRootName: sourceRoot,
                defaultLevel: defaultLevel,
                splashPath: dto?.SplashPath ?? string.Empty
            );

            return project;
        }

        private static string ResolveContentRootWithFallback(string projectDir, string preferred)
        {
            var preferredPath = Path.Combine(projectDir, preferred);
            if (Directory.Exists(preferredPath))
                return preferred;

            var legacyAssets = Path.Combine(projectDir, "Assets");
            if (Directory.Exists(legacyAssets))
                return "Assets";

            return preferred;
        }
    }
}
