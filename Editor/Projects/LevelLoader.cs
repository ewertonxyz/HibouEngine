using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Editor.Projects
{
    public static class LevelLoader
    {
        public static HxLevel Load(string hxlevelPath, string projectDirectory)
        {
            var json = File.ReadAllText(hxlevelPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var level = new HxLevel
            {
                Name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : ""
            };

            var assetGuidMap = new Dictionary<Guid, string>();
            var assetLegacyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (root.TryGetProperty("assets", out var assetsEl))
            {
                foreach (var asset in assetsEl.EnumerateArray())
                {
                    var uri = asset.TryGetProperty("uri", out var uriEl) ? uriEl.GetString() ?? "" : "";
                    var type = asset.TryGetProperty("type", out var typeEl) ? typeEl.GetString() ?? "" : "";
                    var resolvedPath = string.IsNullOrEmpty(uri) ? "" : Path.Combine(projectDirectory, uri);

                    Guid assetGuid = Guid.Empty;
                    string legacyId = "";

                    if (asset.TryGetProperty("id", out var idEl))
                    {
                        var idStr = idEl.GetString() ?? "";
                        if (Guid.TryParse(idStr, out var parsed))
                        {
                            assetGuid = parsed;
                        }
                        else
                        {
                            legacyId = idStr;
                            assetGuid = Guid.NewGuid(); // Assign a runtime GUID
                        }
                    }
                    else
                    {
                        assetGuid = Guid.NewGuid();
                    }

                    var hxAsset = new HxLevelAsset
                    {
                        Id = assetGuid,
                        Type = type,
                        Uri = uri,
                        ResolvedPath = resolvedPath
                    };
                    level.Assets.Add(hxAsset);

                    if (assetGuid != Guid.Empty)
                        assetGuidMap[assetGuid] = resolvedPath;

                    if (!string.IsNullOrEmpty(legacyId))
                        assetLegacyMap[legacyId] = resolvedPath;
                }
            }

            if (root.TryGetProperty("entities", out var entitiesEl))
            {
                foreach (var entEl in entitiesEl.EnumerateArray())
                    level.Entities.Add(ParseEntity(entEl, assetGuidMap, assetLegacyMap));
            }

            return level;
        }

        private static HxLevelEntity ParseEntity(
            JsonElement entEl,
            Dictionary<Guid, string> assetGuidMap,
            Dictionary<string, string> assetLegacyMap)
        {
            Guid entityGuid = Guid.NewGuid();
            if (entEl.TryGetProperty("id", out var idEl))
            {
                var idStr = idEl.GetString() ?? "";
                if (Guid.TryParse(idStr, out var parsed))
                    entityGuid = parsed;
            }

            var entity = new HxLevelEntity
            {
                Id = entityGuid,
                Name = entEl.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? "" : ""
            };

            if (!entEl.TryGetProperty("components", out var components))
                return entity;

            if (components.TryGetProperty("Transform", out var tfEl))
                entity.Components.Add(ParseTransform(tfEl));

            if (components.TryGetProperty("GltfModel", out var gltfEl))
                entity.Components.Add(ParseGltfModel(gltfEl, assetGuidMap, assetLegacyMap));

            if (components.TryGetProperty("Camera", out var camEl))
                entity.Components.Add(ParseCamera(camEl));

            if (components.TryGetProperty("DirectionalLight", out var dlEl))
                entity.Components.Add(ParseDirectionalLight(dlEl));

            return entity;
        }

        private static HxTransformComponent ParseTransform(JsonElement tfEl)
        {
            var tf = new HxTransformComponent();

            if (tfEl.TryGetProperty("position", out var pos))
                tf.Position = pos.EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();

            if (tfEl.TryGetProperty("rotationEulerDeg", out var rot))
                tf.RotationEulerDeg = rot.EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();

            if (tfEl.TryGetProperty("scale", out var scl))
                tf.Scale = scl.EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();

            return tf;
        }

        private static HxGltfModelComponent ParseGltfModel(
            JsonElement gltfEl,
            Dictionary<Guid, string> assetGuidMap,
            Dictionary<string, string> assetLegacyMap)
        {
            var comp = new HxGltfModelComponent();

            if (gltfEl.TryGetProperty("assetRef", out var refEl))
            {
                var refStr = refEl.GetString() ?? "";
                comp.AssetRef = refStr;
                if (Guid.TryParse(refStr, out var refGuid) && assetGuidMap.TryGetValue(refGuid, out var guidPath))
                    comp.ResolvedPath = guidPath;
            }
            else if (gltfEl.TryGetProperty("assetId", out var aidEl))
            {
                var legacyId = aidEl.GetString() ?? "";
                comp.AssetRef = legacyId;
                if (assetLegacyMap.TryGetValue(legacyId, out var legacyPath))
                    comp.ResolvedPath = legacyPath;
            }

            return comp;
        }

        private static HxCameraComponent ParseCamera(JsonElement camEl)
        {
            return new HxCameraComponent
            {
                FovDeg = camEl.TryGetProperty("fovDeg", out var fov) ? (float)fov.GetDouble() : 60f,
                Near = camEl.TryGetProperty("near", out var near) ? (float)near.GetDouble() : 0.1f,
                Far = camEl.TryGetProperty("far", out var far) ? (float)far.GetDouble() : 1000f
            };
        }

        private static HxDirectionalLightComponent ParseDirectionalLight(JsonElement dlEl)
        {
            var comp = new HxDirectionalLightComponent();

            if (dlEl.TryGetProperty("color", out var colorEl))
                comp.Color = colorEl.EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();

            comp.IntensityLux = dlEl.TryGetProperty("intensityLux", out var lux) ? (float)lux.GetDouble() : 1f;

            return comp;
        }
    }
}
