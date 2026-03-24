#pragma once

#include <entt/entt.hpp>
#include "Entities/Scene.h"

namespace Engine::Entities
{
    class Entity
    {
    public:
        Entity();
        Entity(entt::entity entityHandle, Scene* sceneReference);
        ~Entity();

        template<typename ComponentType, typename... Arguments>
        ComponentType& AddComponent(Arguments&&... componentArguments)
        {
            return EntitySceneReference->GetRegistry().emplace<ComponentType>(EntityHandle, std::forward<Arguments>(componentArguments)...);
        }

        template<typename ComponentType>
        ComponentType& GetComponent()
        {
            return EntitySceneReference->GetRegistry().get<ComponentType>(EntityHandle);
        }

        template<typename ComponentType>
        bool HasComponent() const
        {
            return EntitySceneReference->GetRegistry().all_of<ComponentType>(EntityHandle);
        }

        template<typename ComponentType>
        void RemoveComponent()
        {
            EntitySceneReference->GetRegistry().remove<ComponentType>(EntityHandle);
        }

        bool IsValid() const;

        bool operator==(const Entity& otherEntity) const;
        bool operator!=(const Entity& otherEntity) const;

        entt::entity GetHandle() const;

    private:
        entt::entity EntityHandle;
        Scene* EntitySceneReference;
    };
}