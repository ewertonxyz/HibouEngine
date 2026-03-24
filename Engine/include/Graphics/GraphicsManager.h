#pragma once

#include <string>
#include <memory>
#include <windows.h>
#include "Graphics/Interfaces/IGraphicsAPI.h"

namespace Engine::Graphics
{
    class ENGINE_API GraphicsManager
    {
    public:
        GraphicsManager();
        ~GraphicsManager();

        bool Initialize(const std::string& apiName, HWND windowHandle);
        void Resize(int viewportWidth, int viewportHeight);
        void Clear(float red, float green, float blue, float alpha);
        void Render();
        void Shutdown();

    private:
        std::unique_ptr<Engine::Graphics::Interface::IGraphicsAPI> currentGraphicsAPI;
    };
}