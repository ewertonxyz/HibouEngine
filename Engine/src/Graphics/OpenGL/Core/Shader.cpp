#include "Core/EnginePCH.h"
#include "Graphics/OpenGL/Core/Shader.h"

#include <windows.h>
#include <fstream>
#include <sstream>

namespace Engine::Graphics::OpenGL::Core
{
    static std::string ReadShaderFile(const std::string& filePath)
    {
        std::ifstream file(filePath);
        if (!file.is_open())
            return "";
        std::stringstream ss;
        ss << file.rdbuf();
        return ss.str();
    }

    static uint32_t CompileStage(GLenum stage, const std::string& source)
    {
        uint32_t id = glCreateShader(stage);
        const char* src = source.c_str();
        glShaderSource(id, 1, &src, nullptr);
        glCompileShader(id);

        int success = 0;
        glGetShaderiv(id, GL_COMPILE_STATUS, &success);
        if (!success)
        {
            char log[1024];
            glGetShaderInfoLog(id, sizeof(log), nullptr, log);
            OutputDebugStringA("[Shader] Compile error: ");
            OutputDebugStringA(log);
            OutputDebugStringA("\n");
        }
        return id;
    }

    Shader::Shader(const std::string& vertexPath, const std::string& fragmentPath)
    {
        std::string vertSource = ReadShaderFile(vertexPath);
        std::string fragSource = ReadShaderFile(fragmentPath);

        uint32_t vertId = CompileStage(GL_VERTEX_SHADER,   vertSource);
        uint32_t fragId = CompileStage(GL_FRAGMENT_SHADER, fragSource);

        shaderProgramId = glCreateProgram();
        glAttachShader(shaderProgramId, vertId);
        glAttachShader(shaderProgramId, fragId);
        glLinkProgram(shaderProgramId);

        int success = 0;
        glGetProgramiv(shaderProgramId, GL_LINK_STATUS, &success);
        if (!success)
        {
            char log[1024];
            glGetProgramInfoLog(shaderProgramId, sizeof(log), nullptr, log);
            OutputDebugStringA("[Shader] Link error: ");
            OutputDebugStringA(log);
            OutputDebugStringA("\n");
        }

        glDeleteShader(vertId);
        glDeleteShader(fragId);
    }

    Shader::~Shader()
    {
        glDeleteProgram(shaderProgramId);
    }

    void Shader::Bind() const   { glUseProgram(shaderProgramId); }
    void Shader::Unbind() const { glUseProgram(0); }

    void Shader::SetMat4(const std::string& name, const glm::mat4& value) const
    {
        glUniformMatrix4fv(glGetUniformLocation(shaderProgramId, name.c_str()), 1, GL_FALSE, &value[0][0]);
    }

    void Shader::SetFloat3(const std::string& name, const glm::vec3& value) const
    {
        glUniform3fv(glGetUniformLocation(shaderProgramId, name.c_str()), 1, &value[0]);
    }

    void Shader::SetFloat(const std::string& name, float value) const
    {
        glUniform1f(glGetUniformLocation(shaderProgramId, name.c_str()), value);
    }

    void Shader::SetInt(const std::string& name, int value) const
    {
        glUniform1i(glGetUniformLocation(shaderProgramId, name.c_str()), value);
    }
}
