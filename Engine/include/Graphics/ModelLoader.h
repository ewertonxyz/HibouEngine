#pragma once

#include <string>
#include <vector>
#include "Graphics/SubMesh.h"

namespace Engine::Graphics
{
    class ModelLoader
    {
    public:
        static std::vector<SubMesh> LoadGLTF(const std::string& filePath);
    };
}
