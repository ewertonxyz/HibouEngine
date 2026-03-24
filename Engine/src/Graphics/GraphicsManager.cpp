#include "Core/EnginePCH.h"
#include "Graphics/GraphicsManager.h"
#include "Graphics/OpenGL/OpenGLManager.h"

namespace Engine::Graphics
{
    GraphicsManager::GraphicsManager()
    {
    }

    GraphicsManager::~GraphicsManager()
    {
    }

    bool GraphicsManager::Initialize(const std::string& apiName, HWND windowHandle)
    {
        if (apiName == "OpenGL")
        {
            currentGraphicsAPI = std::make_unique<Engine::Graphics::OpenGL::OpenGLManager>();
        }
        else
        {
            return false;
        }

        return currentGraphicsAPI->Initialize(windowHandle);
    }

    void GraphicsManager::Resize(int viewportWidth, int viewportHeight)
    {
        if (currentGraphicsAPI != nullptr)
        {
            currentGraphicsAPI->Resize(viewportWidth, viewportHeight);
        }
    }

    void GraphicsManager::Clear(float red, float green, float blue, float alpha)
    {
        if (currentGraphicsAPI != nullptr)
        {
            currentGraphicsAPI->Clear(red, green, blue, alpha);
        }
    }

    void GraphicsManager::Render()
    {
        if (currentGraphicsAPI != nullptr)
        {
            currentGraphicsAPI->Render();
        }
    }

    void GraphicsManager::Shutdown()
    {
        if (currentGraphicsAPI != nullptr)
        {
            currentGraphicsAPI->Shutdown();
        }
    }
}