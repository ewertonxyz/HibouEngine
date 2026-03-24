#pragma once
#include "Graphics/OpenGL/Core/Buffer.h"

namespace Engine::Graphics::OpenGL::Core
{

    class ENGINE_API VertexArray
    {
    public:
        VertexArray();
        ~VertexArray();

        void Bind() const;
        void Unbind() const;

        void AddVertexBuffer(const std::shared_ptr<VertexBuffer>& vtxBuffer);
        void SetIndexBuffer(const std::shared_ptr<IndexBuffer>& idxBuffer);

        const std::shared_ptr<IndexBuffer>& GetIndexBuffer() const;

    private:
        uint32_t m_VertexArrayId;
        std::vector<std::shared_ptr<VertexBuffer>> m_VertexBuffers;
        std::shared_ptr<IndexBuffer> m_CurrentIndexBuffer;
    };
}
	