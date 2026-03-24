#pragma once

#include <windows.h>
#include "Graphics/Interfaces/IGraphicsAPI.h"

namespace Engine::Graphics::OpenGL
{
    class ENGINE_API OpenGLManager : public Engine::Graphics::Interface::IGraphicsAPI
    {
    public:
        OpenGLManager();
        ~OpenGLManager() override;

        bool Initialize(HWND windowHandle) override;
        void Resize(int viewportWidth, int viewportHeight) override;
        void Clear(float red, float green, float blue, float alpha) override;
        void Render() override;
        void Shutdown() override;

    private:
        HDC deviceContext;
        HGLRC renderContext;
    };
}