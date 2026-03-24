#include "Core/EnginePCH.h"
#include "Graphics/OpenGL/Core/VertexArray.h"
#include "Graphics/OpenGL/Core/Buffer.h"

namespace Engine::Graphics::OpenGL::Core
{
    VertexArray::VertexArray()
    {
        glGenVertexArrays(1, &m_VertexArrayId);
    }

    VertexArray::~VertexArray()
    {
        glDeleteVertexArrays(1, &m_VertexArrayId);
    }

    void VertexArray::Bind() const
    {
        glBindVertexArray(m_VertexArrayId);
    }

    void VertexArray::Unbind() const
    {
        glBindVertexArray(0);
    }

    void VertexArray::AddVertexBuffer(const std::shared_ptr<VertexBuffer>& vertexBuffer)
    {
        Bind();
        vertexBuffer->Bind();

        constexpr uint32_t stride = 8 * sizeof(float);

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, stride, (const void*)(0));

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, stride, (const void*)(3 * sizeof(float)));

        glEnableVertexAttribArray(2);
        glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, stride, (const void*)(6 * sizeof(float)));

        m_VertexBuffers.push_back(vertexBuffer);
    }

    void VertexArray::SetIndexBuffer(const std::shared_ptr<IndexBuffer>& idxBuffer)
    {
        Bind();
        idxBuffer->Bind();
        m_CurrentIndexBuffer = idxBuffer;
    }

    const std::shared_ptr<IndexBuffer>& VertexArray::GetIndexBuffer() const
    {
        return m_CurrentIndexBuffer;
    }
}