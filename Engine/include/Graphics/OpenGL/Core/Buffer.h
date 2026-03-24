#pragma once

namespace Engine::Graphics::OpenGL::Core
{
    class ENGINE_API VertexBuffer
    {
    public:
        VertexBuffer(float* vertexData, uint32_t dataSize);
        ~VertexBuffer();

        void Bind() const;
        void Unbind() const;

    private:
        uint32_t vertexBufferId;
    };

    class ENGINE_API IndexBuffer
    {
    public:
        IndexBuffer(uint32_t* indexData, uint32_t indexCount);
        ~IndexBuffer();

        void Bind() const;
        void Unbind() const;
        uint32_t GetIndexCount() const;

    private:
        uint32_t indexBufferId;
        uint32_t totalIndexCount;
    };

}