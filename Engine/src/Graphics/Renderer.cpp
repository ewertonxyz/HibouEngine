#include "Core/EnginePCH.h"
#include "Graphics/Renderer.h"
#include "Entities/Components.h"
#include "Graphics/OpenGL/Core/GraphicsDevice.h"

#include <glm/gtc/matrix_transform.hpp>

namespace Engine::Graphics
{
    Renderer::Renderer()
    {
    }

    Renderer::~Renderer()
    {
    }

    void Renderer::Initialize()
    {
        BasicGeometryShader = std::make_shared<Engine::Graphics::OpenGL::Core::Shader>(
            "D:/Dev/HibouEngine/Assets/Shaders/OpenGL/BasicShader.vert.glsl",
            "D:/Dev/HibouEngine/Assets/Shaders/OpenGL/BasicShader.frag.glsl"
        );
    }

    void Renderer::RenderScene(Engine::Entities::Scene* currentScene, int viewportWidth, int viewportHeight)
    {
        BasicGeometryShader->Bind();

        glm::vec3 cameraPosition(0.0f, 2.0f, 5.0f);
        float fovDeg    = 45.0f;
        float nearPlane = 0.1f;
        float farPlane  = 5000.0f;
        glm::mat4 viewMatrix = glm::lookAt(cameraPosition, glm::vec3(0.0f), glm::vec3(0.0f, 1.0f, 0.0f));

        auto cameraView = currentScene->GetRegistry().view<
            Engine::Entities::Components::TransformComponent,
            Engine::Entities::Components::CameraComponent>();

        cameraView.each([&](entt::entity,
            Engine::Entities::Components::TransformComponent& t,
            Engine::Entities::Components::CameraComponent& c)
        {
            cameraPosition = t.Translation;
            fovDeg    = c.FovDeg;
            nearPlane = c.Near;
            farPlane  = c.Far;

            float pitchRad = glm::radians(t.Rotation.x);
            float yawRad   = glm::radians(t.Rotation.y);
            glm::vec3 forward(
                glm::cos(pitchRad) * glm::sin(yawRad),
                glm::sin(pitchRad),
                glm::cos(pitchRad) * glm::cos(yawRad)
            );
            viewMatrix = glm::lookAt(cameraPosition, cameraPosition + forward, glm::vec3(0.0f, 1.0f, 0.0f));
        });

        float aspectRatio = (viewportHeight > 0) ? static_cast<float>(viewportWidth) / static_cast<float>(viewportHeight) : 16.0f / 9.0f;
        glm::mat4 projectionMatrix = glm::perspective(
            glm::radians(fovDeg),
            aspectRatio,
            nearPlane,
            farPlane
        );

        BasicGeometryShader->SetMat4("u_ViewMatrix", viewMatrix);
        BasicGeometryShader->SetMat4("u_ProjectionMatrix", projectionMatrix);
        BasicGeometryShader->SetFloat3("u_CameraPosition", cameraPosition);

        glm::vec3 lightDirection(0.0f, -1.0f, 0.0f);
        glm::vec3 lightColor(1.0f);

        auto lightView = currentScene->GetRegistry().view<
            Engine::Entities::Components::TransformComponent,
            Engine::Entities::Components::DirectionalLightComponent>();

        lightView.each([&](entt::entity,
            Engine::Entities::Components::TransformComponent& t,
            Engine::Entities::Components::DirectionalLightComponent& l)
        {
            float pitchRad = glm::radians(t.Rotation.x);
            float yawRad   = glm::radians(t.Rotation.y);
            lightDirection = glm::vec3(
                glm::cos(pitchRad) * glm::sin(yawRad),
                glm::sin(pitchRad),
                glm::cos(pitchRad) * glm::cos(yawRad)
            );
            lightColor = l.Color;
        });

        BasicGeometryShader->SetInt("u_AlbedoTexture",            0);
        BasicGeometryShader->SetInt("u_NormalTexture",            1);
        BasicGeometryShader->SetInt("u_MetallicRoughnessTexture", 2);
        BasicGeometryShader->SetInt("u_OcclusionTexture",         3);

        BasicGeometryShader->SetFloat3("u_LightDirection", lightDirection);
        BasicGeometryShader->SetFloat3("u_LightColor",     lightColor);

        auto componentView = currentScene->GetRegistry().view<
            Engine::Entities::Components::TransformComponent,
            Engine::Entities::Components::MeshComponent>();

        componentView.each([&](entt::entity,
            Engine::Entities::Components::TransformComponent& transformComponent,
            Engine::Entities::Components::MeshComponent& meshComponent)
        {
            glm::mat4 modelMatrix = glm::mat4(1.0f);
            modelMatrix = glm::translate(modelMatrix, transformComponent.Translation);
            modelMatrix = glm::rotate(modelMatrix, glm::radians(transformComponent.Rotation.x), glm::vec3(1.0f, 0.0f, 0.0f));
            modelMatrix = glm::rotate(modelMatrix, glm::radians(transformComponent.Rotation.y), glm::vec3(0.0f, 1.0f, 0.0f));
            modelMatrix = glm::rotate(modelMatrix, glm::radians(transformComponent.Rotation.z), glm::vec3(0.0f, 0.0f, 1.0f));
            modelMatrix = glm::scale(modelMatrix, transformComponent.Scale);

            BasicGeometryShader->SetMat4("u_ModelMatrix", modelMatrix);

            for (auto& subMesh : meshComponent.SubMeshes)
            {
                const auto& mat = subMesh.Material;

                int hasAlbedo            = mat.AlbedoTexture            ? 1 : 0;
                int hasNormal            = mat.NormalTexture             ? 1 : 0;
                int hasMetallicRoughness = mat.MetallicRoughnessTexture  ? 1 : 0;
                int hasOcclusion         = mat.OcclusionTexture          ? 1 : 0;

                if (mat.AlbedoTexture)            mat.AlbedoTexture->Bind(0);
                if (mat.NormalTexture)            mat.NormalTexture->Bind(1);
                if (mat.MetallicRoughnessTexture) mat.MetallicRoughnessTexture->Bind(2);
                if (mat.OcclusionTexture)         mat.OcclusionTexture->Bind(3);

                BasicGeometryShader->SetInt("u_HasAlbedoTexture",            hasAlbedo);
                BasicGeometryShader->SetInt("u_HasNormalTexture",            hasNormal);
                BasicGeometryShader->SetInt("u_HasMetallicRoughnessTexture", hasMetallicRoughness);
                BasicGeometryShader->SetInt("u_HasOcclusionTexture",         hasOcclusion);

                BasicGeometryShader->SetFloat3("u_BaseColorFactor",  mat.BaseColorFactor);
                BasicGeometryShader->SetFloat("u_MetallicFactor",    mat.MetallicFactor);
                BasicGeometryShader->SetFloat("u_RoughnessFactor",   mat.RoughnessFactor);

                if (subMesh.VertexArray)
                {
                    subMesh.VertexArray->Bind();
                    Engine::Graphics::OpenGL::Core::GraphicsDevice::DrawIndexed(subMesh.VertexArray);
                    subMesh.VertexArray->Unbind();
                }
            }
        });

        BasicGeometryShader->Unbind();
    }
}