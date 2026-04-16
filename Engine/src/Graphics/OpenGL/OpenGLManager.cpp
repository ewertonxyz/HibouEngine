#include "Core/EnginePCH.h"
#include "Graphics/OpenGL/OpenGLManager.h"
#include "Graphics/OpenGL/Core/GraphicsDevice.h"

void* GetOpenGLFunctionAddress(const char* functionName)
{
    void* functionPointer = (void*)wglGetProcAddress(functionName);

    if (functionPointer == nullptr || functionPointer == (void*)0x1 || functionPointer == (void*)0x2 || functionPointer == (void*)0x3 || functionPointer == (void*)-1)
    {
        HMODULE openglModule = GetModuleHandleA("opengl32.dll");
        functionPointer = (void*)GetProcAddress(openglModule, functionName);
    }

    return functionPointer;
}

namespace Engine::Graphics::OpenGL
{
    OpenGLManager::OpenGLManager()
    {
        deviceContext = nullptr;
        renderContext = nullptr;
    }

    OpenGLManager::~OpenGLManager()
    {
    }

    bool OpenGLManager::Initialize(HWND windowHandle)
    {
        deviceContext = GetDC(windowHandle);
        if (!deviceContext)
        {
            OutputDebugStringA("[HibouEngine] OpenGLManager: GetDC failed\n");
            return false;
        }

        PIXELFORMATDESCRIPTOR pixelFormatDescriptor = {};
        pixelFormatDescriptor.nSize = sizeof(PIXELFORMATDESCRIPTOR);
        pixelFormatDescriptor.nVersion = 1;
        pixelFormatDescriptor.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
        pixelFormatDescriptor.iPixelType = PFD_TYPE_RGBA;
        pixelFormatDescriptor.cColorBits = 32;
        pixelFormatDescriptor.cDepthBits = 24;
        pixelFormatDescriptor.cStencilBits = 8;

        int pixelFormat = ChoosePixelFormat(deviceContext, &pixelFormatDescriptor);
        if (!pixelFormat || !SetPixelFormat(deviceContext, pixelFormat, &pixelFormatDescriptor))
        {
            OutputDebugStringA("[HibouEngine] OpenGLManager: ChoosePixelFormat/SetPixelFormat failed\n");
            return false;
        }

        renderContext = wglCreateContext(deviceContext);
        if (!renderContext)
        {
            OutputDebugStringA("[HibouEngine] OpenGLManager: wglCreateContext failed\n");
            return false;
        }

        if (!wglMakeCurrent(deviceContext, renderContext))
        {
            OutputDebugStringA("[HibouEngine] OpenGLManager: wglMakeCurrent failed\n");
            return false;
        }

        if (!gladLoadGLLoader((GLADloadproc)GetOpenGLFunctionAddress))
        {
            OutputDebugStringA("[HibouEngine] OpenGLManager: gladLoadGLLoader failed — no valid OpenGL context\n");
            return false;
        }

        Engine::Graphics::OpenGL::Core::GraphicsDevice::Initialize();
        return true;
    }

    void OpenGLManager::Resize(int viewportWidth, int viewportHeight)
    {
        if (renderContext != nullptr)
        {
            Engine::Graphics::OpenGL::Core::GraphicsDevice::SetViewport(viewportWidth, viewportHeight);
        }
    }

    void OpenGLManager::Clear(float red, float green, float blue, float alpha)
    {
        if (renderContext != nullptr)
        {
            Engine::Graphics::OpenGL::Core::GraphicsDevice::SetClearColor(red, green, blue, alpha);
            Engine::Graphics::OpenGL::Core::GraphicsDevice::Clear();
        }
    }

    void OpenGLManager::Render()
    {
        if (renderContext != nullptr)
        {
            SwapBuffers(deviceContext);
        }
    }

    void OpenGLManager::Shutdown()
    {
        if (renderContext != nullptr)
        {
            wglMakeCurrent(nullptr, nullptr);
            wglDeleteContext(renderContext);
            renderContext = nullptr;
        }
    }
}