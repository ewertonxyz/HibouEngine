#include "Core/EnginePCH.h"
#include "Graphics/OpenGL/Core/GraphicsDevice.h"
#include "Graphics/OpenGL/Core/VertexArray.h"

namespace Engine::Graphics::OpenGL::Core
{
    void GraphicsDevice::Initialize()
    {
        glEnable(GL_DEPTH_TEST);
    }

    void GraphicsDevice::SetViewport(int viewportWidth, int viewportHeight)
    {
        glViewport(0, 0, viewportWidth, viewportHeight);
    }

    void GraphicsDevice::SetClearColor(float red, float green, float blue, float alpha)
    {
        glClearColor(red, green, blue, alpha);
    }

    void GraphicsDevice::Clear()
    {
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    }

    void GraphicsDevice::DrawIndexed(const std::shared_ptr<VertexArray>& vtxArray)
    {
		glDrawElements(GL_TRIANGLES, vtxArray->GetIndexBuffer()->GetIndexCount(), GL_UNSIGNED_INT, nullptr);
    }
}