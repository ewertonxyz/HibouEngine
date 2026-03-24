#pragma once

#include <memory>
#include <glm/glm.hpp>
#include "Graphics/OpenGL/Core/VertexArray.h"
#include "Graphics/OpenGL/Core/Texture.h"

namespace Engine::Graphics
{
    struct PbrMaterial
    {
        std::shared_ptr<OpenGL::Core::Texture> AlbedoTexture;
        std::shared_ptr<OpenGL::Core::Texture> NormalTexture;
        std::shared_ptr<OpenGL::Core::Texture> MetallicRoughnessTexture; // G = roughness, B = metallic
        std::shared_ptr<OpenGL::Core::Texture> OcclusionTexture;

        glm::vec3 BaseColorFactor = glm::vec3(1.0f);
        float     MetallicFactor  = 0.0f;
        float     RoughnessFactor = 1.0f;
    };

    struct SubMesh
    {
        std::shared_ptr<OpenGL::Core::VertexArray> VertexArray;
        PbrMaterial Material;
    };
}
