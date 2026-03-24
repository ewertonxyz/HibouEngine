#pragma once

#include <string>
#include <glm/glm.hpp>

namespace Engine::Graphics::OpenGL::Core
{
    class ENGINE_API Shader
    {
    public:
        Shader(const std::string& vertexSource, const std::string& fragmentSource);
        ~Shader();

        void Bind() const;
        void Unbind() const;

        void SetMat4(const std::string& name, const glm::mat4& value) const;
        void SetFloat3(const std::string& name, const glm::vec3& value) const;
        void SetFloat(const std::string& name, float value) const;
        void SetInt(const std::string& name, int value) const;

    private:
        uint32_t shaderProgramId;
    };
}