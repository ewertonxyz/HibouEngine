#pragma once

#include <windows.h>

namespace Engine::Graphics::Interface
{
    class IGraphicsAPI
    {
    public:
        virtual ~IGraphicsAPI() = default;

        virtual bool Initialize(HWND windowHandle) = 0;
        virtual void Resize(int viewportWidth, int viewportHeight) = 0;
        virtual void Clear(float red, float green, float blue, float alpha) = 0;
        virtual void Render() = 0;
        virtual void Shutdown() = 0;
    };
}