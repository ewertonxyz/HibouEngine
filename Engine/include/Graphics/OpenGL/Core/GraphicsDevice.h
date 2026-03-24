#pragma once

namespace Engine::Graphics::OpenGL::Core
{
    class VertexArray;

    class ENGINE_API GraphicsDevice
    {
    public:
        static void Initialize();
        static void SetViewport(int viewportWidth, int viewportHeight);
        static void SetClearColor(float red, float green, float blue, float alpha);
        static void Clear();
        static void DrawIndexed(const std::shared_ptr<VertexArray>& vtxArray);
    };
}