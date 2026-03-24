#pragma once

#include <cstdint>

namespace Engine::Graphics::OpenGL::Core
{
    class ENGINE_API Texture
    {
    public:
        Texture(const unsigned char* data, int width, int height, int channels, bool isSRGB = false);
        ~Texture();

        void Bind(uint32_t slot) const;

    private:
        uint32_t m_TextureId = 0;
    };
}
