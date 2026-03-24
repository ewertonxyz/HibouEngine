#pragma once

#include <memory>
#include "Entities/Scene.h"
#include "Graphics/OpenGL/Core/Shader.h"

namespace Engine::Graphics
{
    class Renderer
    {
    public:
        Renderer();
        ~Renderer();

        void Initialize();
        void RenderScene(Engine::Entities::Scene* currentScene, int viewportWidth, int viewportHeight);

    private:
        std::shared_ptr<Engine::Graphics::OpenGL::Core::Shader> BasicGeometryShader;
    };
}