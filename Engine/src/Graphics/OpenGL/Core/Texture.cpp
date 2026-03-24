#include "Core/EnginePCH.h"
#include "Graphics/OpenGL/Core/Texture.h"

namespace Engine::Graphics::OpenGL::Core
{
    Texture::Texture(const unsigned char* data, int width, int height, int channels, bool isSRGB)
    {
        glGenTextures(1, &m_TextureId);
        glBindTexture(GL_TEXTURE_2D, m_TextureId);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        GLenum internalFormat = GL_RGBA8;
        GLenum dataFormat     = GL_RGBA;

        if (channels == 4)
        {
            internalFormat = isSRGB ? GL_SRGB8_ALPHA8 : GL_RGBA8;
            dataFormat     = GL_RGBA;
        }
        else if (channels == 3)
        {
            internalFormat = isSRGB ? GL_SRGB8 : GL_RGB8;
            dataFormat     = GL_RGB;
        }
        else if (channels == 2)
        {
            internalFormat = GL_RG8;
            dataFormat     = GL_RG;
        }
        else if (channels == 1)
        {
            internalFormat = GL_R8;
            dataFormat     = GL_RED;
        }

        glTexImage2D(GL_TEXTURE_2D, 0, internalFormat, width, height, 0, dataFormat, GL_UNSIGNED_BYTE, data);
        glGenerateMipmap(GL_TEXTURE_2D);

        glBindTexture(GL_TEXTURE_2D, 0);
    }

    Texture::~Texture()
    {
        glDeleteTextures(1, &m_TextureId);
    }

    void Texture::Bind(uint32_t slot) const
    {
        glActiveTexture(GL_TEXTURE0 + slot);
        glBindTexture(GL_TEXTURE_2D, m_TextureId);
    }
}
