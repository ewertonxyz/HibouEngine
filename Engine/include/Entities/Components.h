#pragma once

#include <string>
#include <memory>
#include <vector>
#include <glm/glm.hpp>
#include "Graphics/SubMesh.h"

namespace Engine::Entities::Components
{
    struct TagComponent
    {
        std::string Tag;
    };

    struct TransformComponent
    {
        glm::vec3 Translation;
        glm::vec3 Rotation;
        glm::vec3 Scale;
    };

    struct NodeComponent
    {
        int NodeIdentifier;
        std::string NodeType;
    };

    struct MeshComponent
    {
        std::vector<Engine::Graphics::SubMesh> SubMeshes;
        std::string FilePath;
    };

    struct BasicMaterialComponent
    {
        glm::vec3 ObjectColor;
        glm::vec3 LightDirection;
        glm::vec3 LightColor;
    };

    struct CameraComponent
    {
        float FovDeg;
        float Near;
        float Far;
    };

    struct DirectionalLightComponent
    {
        glm::vec3 Color;
        float IntensityLux;
    };
}