using System;
using System.Collections.Generic;
using System.Linq;

namespace Editor.Projects
{
    public abstract class HxLevelComponent
    {
        public string Type { get; }
        protected HxLevelComponent(string type) { Type = type; }
    }

    public sealed class HxTransformComponent : HxLevelComponent
    {
        public HxTransformComponent() : base("Transform") { }

        public float[] Position { get; set; } = new float[3];
        public float[] RotationEulerDeg { get; set; } = new float[3];
        public float[] Scale { get; set; } = new float[] { 1f, 1f, 1f };
    }

    public sealed class HxGltfModelComponent : HxLevelComponent
    {
        public HxGltfModelComponent() : base("GltfModel") { }

        public string AssetRef { get; set; } = string.Empty;
        public string ResolvedPath { get; set; } = string.Empty;
    }

    public sealed class HxCameraComponent : HxLevelComponent
    {
        public HxCameraComponent() : base("Camera") { }

        public float FovDeg { get; set; } = 60f;
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 1000f;
    }

    public sealed class HxDirectionalLightComponent : HxLevelComponent
    {
        public HxDirectionalLightComponent() : base("DirectionalLight") { }

        public float[] Color { get; set; } = new float[] { 1f, 1f, 1f };
        public float IntensityLux { get; set; } = 1f;
    }

    public sealed class HxLevelAsset
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Type { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string ResolvedPath { get; set; } = string.Empty;
    }

    public sealed class HxLevelEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public List<HxLevelComponent> Components { get; set; } = new();

        public T? GetComponent<T>() where T : HxLevelComponent
            => Components.OfType<T>().FirstOrDefault();

        public bool HasComponent<T>() where T : HxLevelComponent
            => Components.OfType<T>().Any();

        public string PrimaryComponentType
            => Components.FirstOrDefault(c => c.Type != "Transform")?.Type ?? "Unknown";
    }

    public sealed class HxLevel
    {
        public string Name { get; set; } = string.Empty;
        public List<HxLevelAsset> Assets { get; set; } = new();
        public List<HxLevelEntity> Entities { get; set; } = new();
    }
}
