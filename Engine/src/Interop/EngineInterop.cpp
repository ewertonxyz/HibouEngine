#include "Core/EnginePCH.h"
#include <windows.h>
#include "Entities/Scene.h"
#include "Entities/Entity.h"
#include "Entities/Components.h"
#include "Graphics/Renderer.h"
#include "Graphics/ModelLoader.h"
#include "Graphics/GraphicsManager.h"

namespace Engine::Interop
{
    static Engine::Graphics::GraphicsManager* ActiveGraphicsManager = nullptr;
    static Engine::Entities::Scene* ActiveScene = nullptr;
    static Engine::Graphics::Renderer* ActiveRenderer = nullptr;
    static int ActiveViewportWidth  = 800;
    static int ActiveViewportHeight = 600;
}

extern "C"
{
    __declspec(dllexport) void InitializeGameEngine(void* windowHandle)
    {
        HWND nativeWindowHandle = static_cast<HWND>(windowHandle);

        Engine::Interop::ActiveGraphicsManager = new Engine::Graphics::GraphicsManager();
        Engine::Interop::ActiveGraphicsManager->Initialize("OpenGL", nativeWindowHandle);

        Engine::Interop::ActiveScene = new Engine::Entities::Scene();
        Engine::Interop::ActiveRenderer = new Engine::Graphics::Renderer();

        Engine::Interop::ActiveRenderer->Initialize();
    }

    __declspec(dllexport) void ClearScene()
    {
        if (Engine::Interop::ActiveScene != nullptr)
        {
            delete Engine::Interop::ActiveScene;
            Engine::Interop::ActiveScene = new Engine::Entities::Scene();
        }
    }

    __declspec(dllexport) void LoadGltfEntity(
        const char* name,
        const char* filePath,
        float px, float py, float pz,
        float rx, float ry, float rz,
        float sx, float sy, float sz)
    {
        if (Engine::Interop::ActiveScene == nullptr)
            return;

        Engine::Entities::Entity entity = Engine::Interop::ActiveScene->CreateEntity(name);

        Engine::Entities::Components::TransformComponent& transform = entity.AddComponent<Engine::Entities::Components::TransformComponent>();
        transform.Translation = glm::vec3(px, py, pz);
        transform.Rotation    = glm::vec3(rx, ry, rz);
        transform.Scale       = glm::vec3(sx, sy, sz);

        Engine::Entities::Components::MeshComponent& mesh = entity.AddComponent<Engine::Entities::Components::MeshComponent>();
        mesh.FilePath  = filePath;
        mesh.SubMeshes = Engine::Graphics::ModelLoader::LoadGLTF(filePath);

        Engine::Entities::Components::BasicMaterialComponent& material = entity.AddComponent<Engine::Entities::Components::BasicMaterialComponent>();
        material.ObjectColor    = glm::vec3(0.8f, 0.8f, 0.8f);
        material.LightDirection = glm::vec3(-0.2f, -1.0f, -0.3f);
        material.LightColor     = glm::vec3(1.0f, 0.98f, 0.92f);
    }

    __declspec(dllexport) void LoadCameraEntity(
        const char* name,
        float px, float py, float pz,
        float rx, float ry, float rz,
        float fovDeg, float nearPlane, float farPlane)
    {
        if (Engine::Interop::ActiveScene == nullptr)
            return;

        Engine::Entities::Entity entity = Engine::Interop::ActiveScene->CreateEntity(name);

        Engine::Entities::Components::TransformComponent& transform = entity.AddComponent<Engine::Entities::Components::TransformComponent>();
        transform.Translation = glm::vec3(px, py, pz);
        transform.Rotation    = glm::vec3(rx, ry, rz);
        transform.Scale       = glm::vec3(1.0f, 1.0f, 1.0f);

        Engine::Entities::Components::CameraComponent& camera = entity.AddComponent<Engine::Entities::Components::CameraComponent>();
        camera.FovDeg = fovDeg;
        camera.Near   = nearPlane;
        camera.Far    = farPlane;
    }

    __declspec(dllexport) void LoadDirectionalLightEntity(
        const char* name,
        float px, float py, float pz,
        float rx, float ry, float rz,
        float r, float g, float b,
        float intensityLux)
    {
        if (Engine::Interop::ActiveScene == nullptr)
            return;

        Engine::Entities::Entity entity = Engine::Interop::ActiveScene->CreateEntity(name);

        Engine::Entities::Components::TransformComponent& transform = entity.AddComponent<Engine::Entities::Components::TransformComponent>();
        transform.Translation = glm::vec3(px, py, pz);
        transform.Rotation    = glm::vec3(rx, ry, rz);
        transform.Scale       = glm::vec3(1.0f, 1.0f, 1.0f);

        Engine::Entities::Components::DirectionalLightComponent& light = entity.AddComponent<Engine::Entities::Components::DirectionalLightComponent>();
        light.Color        = glm::vec3(r, g, b);
        light.IntensityLux = intensityLux;
    }

    __declspec(dllexport) void ResizeEngineViewport(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;
        Engine::Interop::ActiveViewportWidth  = width;
        Engine::Interop::ActiveViewportHeight = height;
        glViewport(0, 0, width, height);
    }

    __declspec(dllexport) void RenderEngineFrame()
    {
        if (Engine::Interop::ActiveGraphicsManager != nullptr && Engine::Interop::ActiveRenderer != nullptr && Engine::Interop::ActiveScene != nullptr)
        {
            Engine::Interop::ActiveGraphicsManager->Clear(0.2f, 0.2f, 0.2f, 1.0f);
            Engine::Interop::ActiveRenderer->RenderScene(Engine::Interop::ActiveScene, Engine::Interop::ActiveViewportWidth, Engine::Interop::ActiveViewportHeight);
            Engine::Interop::ActiveGraphicsManager->Render();
        }
    }

    __declspec(dllexport) void ShutdownGameEngine()
    {
        if (Engine::Interop::ActiveScene != nullptr)
        {
            delete Engine::Interop::ActiveScene;
            Engine::Interop::ActiveScene = nullptr;
        }

        if (Engine::Interop::ActiveRenderer != nullptr)
        {
            delete Engine::Interop::ActiveRenderer;
            Engine::Interop::ActiveRenderer = nullptr;
        }

        if (Engine::Interop::ActiveGraphicsManager != nullptr)
        {
            Engine::Interop::ActiveGraphicsManager->Shutdown();
            delete Engine::Interop::ActiveGraphicsManager;
            Engine::Interop::ActiveGraphicsManager = nullptr;
        }
    }
}