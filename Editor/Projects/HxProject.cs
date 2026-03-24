using System;
using System.IO;

namespace Editor.Projects
{
    public sealed class HxProject
    {
        public string Name { get; }
        public string Description { get; }
        public string ProjectFilePath { get; }
        public string ProjectDirectory { get; }
        public string IconPath { get; }
        public string SplashPath { get; }
        public string ContentRootName { get; }
        public string LevelsRootName { get; }
        public string SourceRootName { get; }

        public string ContentDirectory { get; }
        public string LevelsDirectory { get; }
        public string SourceDirectory { get; }

        public string DefaultLevel { get; }

        public HxProject(
            string name,
            string description,
            string projectFilePath,
            string iconPath,
            string contentRootName,
            string levelsRootName,
            string sourceRootName,
            string defaultLevel = "",
            string splashPath = "")
        {
            Name = name;
            Description = description;
            ProjectFilePath = projectFilePath;
            ProjectDirectory = Path.GetDirectoryName(projectFilePath) ?? string.Empty;
            IconPath = iconPath;
            SplashPath = splashPath;
            ContentRootName = contentRootName;
            LevelsRootName = levelsRootName;
            SourceRootName = sourceRootName;
            DefaultLevel = defaultLevel;

            ContentDirectory = Path.Combine(ProjectDirectory, ContentRootName);
            LevelsDirectory = Path.Combine(ProjectDirectory, LevelsRootName);
            SourceDirectory = Path.Combine(ProjectDirectory, SourceRootName);
        }

        public string GetDefaultLevelPath()
        {
            if (string.IsNullOrWhiteSpace(DefaultLevel))
                return string.Empty;

            return Path.Combine(ProjectDirectory, DefaultLevel);
        }

        public string GetResolvedIconPath()
        {
            if (string.IsNullOrWhiteSpace(IconPath))
                return string.Empty;

            if (Path.IsPathRooted(IconPath))
                return IconPath;

            return Path.Combine(ProjectDirectory, IconPath);
        }

        public string GetResolvedSplashPath()
        {
            if (string.IsNullOrWhiteSpace(SplashPath))
                return string.Empty;

            if (Path.IsPathRooted(SplashPath))
                return SplashPath;

            return Path.Combine(ProjectDirectory, SplashPath);
        }
    }
}
