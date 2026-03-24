#include "Core/EnginePCH.h"

#pragma warning(disable : 4996)

#include "Graphics/ModelLoader.h"

#define TINYGLTF_IMPLEMENTATION
#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include <tiny_gltf.h>

#include "Graphics/OpenGL/Core/Buffer.h"
#include "Graphics/OpenGL/Core/Texture.h"

#include <unordered_map>

namespace Engine::Graphics
{
    static std::shared_ptr<OpenGL::Core::Texture> LoadGltfTexture(
        const tinygltf::Model& model,
        int textureIndex,
        bool isSRGB,
        std::unordered_map<int, std::shared_ptr<OpenGL::Core::Texture>>& cache)
    {
        if (textureIndex < 0)
            return nullptr;

        auto it = cache.find(textureIndex);
        if (it != cache.end())
            return it->second;

        const tinygltf::Texture& gltfTex = model.textures[textureIndex];
        if (gltfTex.source < 0)
            return nullptr;

        const tinygltf::Image& img = model.images[gltfTex.source];
        if (img.image.empty() || img.width <= 0 || img.height <= 0)
            return nullptr;

        auto tex = std::make_shared<OpenGL::Core::Texture>(
            img.image.data(), img.width, img.height, img.component, isSRGB);

        cache[textureIndex] = tex;
        return tex;
    }

    std::vector<SubMesh> ModelLoader::LoadGLTF(const std::string& filePath)
    {
        tinygltf::Model gltfModel;
        tinygltf::TinyGLTF gltfContext;
        std::string errorString;
        std::string warningString;

        bool isLoaded = gltfContext.LoadASCIIFromFile(&gltfModel, &errorString, &warningString, filePath);
        if (!isLoaded)
            return {};

        std::vector<SubMesh> result;
        std::unordered_map<int, std::shared_ptr<OpenGL::Core::Texture>> textureCache;

        for (const auto& currentMesh : gltfModel.meshes)
        {
            for (const auto& primitive : currentMesh.primitives)
            {
                const float* positionBuffer = nullptr;
                const float* normalBuffer   = nullptr;
                const float* uvBuffer       = nullptr;
                size_t vertexCount = 0;

                if (primitive.attributes.count("POSITION"))
                {
                    const auto& acc  = gltfModel.accessors[primitive.attributes.at("POSITION")];
                    const auto& view = gltfModel.bufferViews[acc.bufferView];
                    positionBuffer   = reinterpret_cast<const float*>(
                        &gltfModel.buffers[view.buffer].data[view.byteOffset + acc.byteOffset]);
                    vertexCount = acc.count;
                }

                if (primitive.attributes.count("NORMAL"))
                {
                    const auto& acc  = gltfModel.accessors[primitive.attributes.at("NORMAL")];
                    const auto& view = gltfModel.bufferViews[acc.bufferView];
                    normalBuffer     = reinterpret_cast<const float*>(
                        &gltfModel.buffers[view.buffer].data[view.byteOffset + acc.byteOffset]);
                }

                if (primitive.attributes.count("TEXCOORD_0"))
                {
                    const auto& acc  = gltfModel.accessors[primitive.attributes.at("TEXCOORD_0")];
                    const auto& view = gltfModel.bufferViews[acc.bufferView];
                    uvBuffer         = reinterpret_cast<const float*>(
                        &gltfModel.buffers[view.buffer].data[view.byteOffset + acc.byteOffset]);
                }

                if (vertexCount == 0)
                    continue;

                std::vector<float> vertexData;
                vertexData.reserve(vertexCount * 8);

                for (size_t i = 0; i < vertexCount; ++i)
                {
                    vertexData.push_back(positionBuffer ? positionBuffer[i * 3 + 0] : 0.0f);
                    vertexData.push_back(positionBuffer ? positionBuffer[i * 3 + 1] : 0.0f);
                    vertexData.push_back(positionBuffer ? positionBuffer[i * 3 + 2] : 0.0f);

                    vertexData.push_back(normalBuffer ? normalBuffer[i * 3 + 0] : 0.0f);
                    vertexData.push_back(normalBuffer ? normalBuffer[i * 3 + 1] : 1.0f);
                    vertexData.push_back(normalBuffer ? normalBuffer[i * 3 + 2] : 0.0f);

                    vertexData.push_back(uvBuffer ? uvBuffer[i * 2 + 0] : 0.0f);
                    vertexData.push_back(uvBuffer ? uvBuffer[i * 2 + 1] : 0.0f);
                }

                std::vector<uint32_t> indexData;
                if (primitive.indices >= 0)
                {
                    const auto& acc  = gltfModel.accessors[primitive.indices];
                    const auto& view = gltfModel.bufferViews[acc.bufferView];
                    const auto& buf  = gltfModel.buffers[view.buffer];
                    indexData.reserve(acc.count);

                    if (acc.componentType == TINYGLTF_COMPONENT_TYPE_UNSIGNED_SHORT)
                    {
                        const uint16_t* ptr = reinterpret_cast<const uint16_t*>(
                            &buf.data[view.byteOffset + acc.byteOffset]);
                        for (size_t i = 0; i < acc.count; ++i)
                            indexData.push_back(static_cast<uint32_t>(ptr[i]));
                    }
                    else if (acc.componentType == TINYGLTF_COMPONENT_TYPE_UNSIGNED_INT)
                    {
                        const uint32_t* ptr = reinterpret_cast<const uint32_t*>(
                            &buf.data[view.byteOffset + acc.byteOffset]);
                        for (size_t i = 0; i < acc.count; ++i)
                            indexData.push_back(ptr[i]);
                    }
                }

                if (indexData.empty())
                    continue;

                auto vao = std::make_shared<OpenGL::Core::VertexArray>();
                auto vbo = std::make_shared<OpenGL::Core::VertexBuffer>(
                    vertexData.data(), static_cast<uint32_t>(vertexData.size() * sizeof(float)));
                vao->AddVertexBuffer(vbo);

                auto ibo = std::make_shared<OpenGL::Core::IndexBuffer>(
                    indexData.data(), static_cast<uint32_t>(indexData.size()));
                vao->SetIndexBuffer(ibo);

                PbrMaterial material;
                if (primitive.material >= 0 &&
                    primitive.material < static_cast<int>(gltfModel.materials.size()))
                {
                    const tinygltf::Material& mat = gltfModel.materials[primitive.material];
                    const auto& pbr = mat.pbrMetallicRoughness;

                    const auto& cf = pbr.baseColorFactor;
                    material.BaseColorFactor = glm::vec3(
                        static_cast<float>(cf[0]),
                        static_cast<float>(cf[1]),
                        static_cast<float>(cf[2]));
                    material.MetallicFactor  = static_cast<float>(pbr.metallicFactor);
                    material.RoughnessFactor = static_cast<float>(pbr.roughnessFactor);

                    material.AlbedoTexture = LoadGltfTexture(
                        gltfModel, pbr.baseColorTexture.index, true, textureCache);

                    material.MetallicRoughnessTexture = LoadGltfTexture(
                        gltfModel, pbr.metallicRoughnessTexture.index, false, textureCache);

                    material.NormalTexture = LoadGltfTexture(
                        gltfModel, mat.normalTexture.index, false, textureCache);

                    material.OcclusionTexture = LoadGltfTexture(
                        gltfModel, mat.occlusionTexture.index, false, textureCache);
                }

                result.push_back({ vao, material });
            }
        }

        return result;
    }
}
