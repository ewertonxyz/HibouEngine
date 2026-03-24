using Editor.Projects;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Editor.Components.Explorer
{
    public sealed class SceneEntityItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Guid   EntityId     { get; init; } = Guid.Empty;
        public string Name         { get; init; } = string.Empty;
        public string ComponentType{ get; init; } = string.Empty;

        public float PosX   { get; init; }
        public float PosY   { get; init; }
        public float PosZ   { get; init; }
        public float RotX   { get; init; }
        public float RotY   { get; init; }
        public float RotZ   { get; init; }
        public float ScaleX { get; init; }
        public float ScaleY { get; init; }
        public float ScaleZ { get; init; }

        public float FovDeg { get; init; }
        public float Near   { get; init; }
        public float Far    { get; init; }

        public float LightR      { get; init; }
        public float LightG      { get; init; }
        public float LightB      { get; init; }
        public float IntensityLux{ get; init; }

        public string GltfPath { get; init; } = string.Empty;

        public bool IsCamera => ComponentType == "Camera";
        public bool IsLight  => ComponentType == "DirectionalLight";
        public bool IsGltf   => ComponentType == "GltfModel";

        public Visibility CameraVisibility => IsCamera ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LightVisibility  => IsLight  ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GltfVisibility   => IsGltf   ? Visibility.Visible : Visibility.Collapsed;

        public string TypeLabel => ComponentType switch
        {
            "Camera"           => "Camera",
            "DirectionalLight" => "Directional Light",
            "GltfModel"        => "GLTF Model",
            _                  => ComponentType
        };

        public string TypeIcon => ComponentType switch
        {
            "Camera"           => "C",
            "DirectionalLight" => "L",
            "GltfModel"        => "M",
            _                  => "?"
        };

        public Brush TypeBadgeBrush => ComponentType switch
        {
            "Camera"           => new SolidColorBrush(Color.FromRgb(0x22, 0x77, 0xBB)),
            "DirectionalLight" => new SolidColorBrush(Color.FromRgb(0xCC, 0x88, 0x22)),
            "GltfModel"        => new SolidColorBrush(Color.FromRgb(0x33, 0x99, 0x55)),
            _                  => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55))
        };

        public string GltfFileName => System.IO.Path.GetFileName(GltfPath);

        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public static SceneEntityItem FromLevelEntity(HxLevelEntity e)
        {
            var tf = e.GetComponent<HxTransformComponent>();
            var cam = e.GetComponent<HxCameraComponent>();
            var light = e.GetComponent<HxDirectionalLightComponent>();
            var gltf = e.GetComponent<HxGltfModelComponent>();

            return new SceneEntityItem
            {
                EntityId      = e.Id,
                Name          = e.Name,
                ComponentType = e.PrimaryComponentType,
                PosX   = tf != null && tf.Position.Length > 0 ? tf.Position[0] : 0f,
                PosY   = tf != null && tf.Position.Length > 1 ? tf.Position[1] : 0f,
                PosZ   = tf != null && tf.Position.Length > 2 ? tf.Position[2] : 0f,
                RotX   = tf != null && tf.RotationEulerDeg.Length > 0 ? tf.RotationEulerDeg[0] : 0f,
                RotY   = tf != null && tf.RotationEulerDeg.Length > 1 ? tf.RotationEulerDeg[1] : 0f,
                RotZ   = tf != null && tf.RotationEulerDeg.Length > 2 ? tf.RotationEulerDeg[2] : 0f,
                ScaleX = tf != null && tf.Scale.Length > 0 ? tf.Scale[0] : 1f,
                ScaleY = tf != null && tf.Scale.Length > 1 ? tf.Scale[1] : 1f,
                ScaleZ = tf != null && tf.Scale.Length > 2 ? tf.Scale[2] : 1f,
                FovDeg       = cam?.FovDeg ?? 60f,
                Near         = cam?.Near ?? 0.1f,
                Far          = cam?.Far ?? 1000f,
                LightR       = light != null && light.Color.Length > 0 ? light.Color[0] : 1f,
                LightG       = light != null && light.Color.Length > 1 ? light.Color[1] : 1f,
                LightB       = light != null && light.Color.Length > 2 ? light.Color[2] : 1f,
                IntensityLux = light?.IntensityLux ?? 1f,
                GltfPath     = gltf?.ResolvedPath ?? string.Empty
            };
        }
    }

    public sealed class SceneViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<SceneEntityItem?>?    SelectionChanged;

        public ObservableCollection<SceneEntityItem> Entities { get; } = new();
        public string LevelName { get; private set; } = string.Empty;

        private SceneEntityItem? _selected;
        public SceneEntityItem? SelectedEntity
        {
            get => _selected;
            set
            {
                if (ReferenceEquals(_selected, value)) return;
                _selected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(value);
            }
        }

        public void PopulateFromLevel(HxLevel level)
        {
            LevelName = level.Name;
            Entities.Clear();
            foreach (var e in level.Entities)
                Entities.Add(SceneEntityItem.FromLevelEntity(e));
            SelectedEntity = null;
            OnPropertyChanged(nameof(LevelName));
        }

        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
