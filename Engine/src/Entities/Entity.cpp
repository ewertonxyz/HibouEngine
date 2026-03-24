#include "Core/EnginePCH.h"
#include "Entities/Entity.h"

namespace Engine::Entities
{
    Entity::Entity()
    {
        EntityHandle = entt::null;
        EntitySceneReference = nullptr;
    }

    Entity::Entity(entt::entity entityHandle, Scene* sceneReference)
    {
        EntityHandle = entityHandle;
        EntitySceneReference = sceneReference;
    }

    Entity::~Entity()
    {
    }

    bool Entity::IsValid() const
    {
        if (EntitySceneReference != nullptr)
        {
            return EntitySceneReference->GetRegistry().valid(EntityHandle);
        }
        return false;
    }

    bool Entity::operator==(const Entity& otherEntity) const
    {
        return EntityHandle == otherEntity.EntityHandle && EntitySceneReference == otherEntity.EntitySceneReference;
    }

    bool Entity::operator!=(const Entity& otherEntity) const
    {
        return !(*this == otherEntity);
    }

    entt::entity Entity::GetHandle() const
    {
        return EntityHandle;
    }
}