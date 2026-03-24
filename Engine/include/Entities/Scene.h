#pragma once

#include <entt/entt.hpp>
#include <string>

namespace Engine::Entities
{
    class Entity;

    class Scene
    {
    public:
        Scene();
        ~Scene();

        Entity CreateEntity(const std::string& entityName);
        void DestroyEntity(Entity entityToDestroy);

        entt::registry& GetRegistry();

    private:
        entt::registry SceneRegistry;
    };
}