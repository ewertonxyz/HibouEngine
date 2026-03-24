#include "Core/EnginePCH.h"
#include "Entities/Scene.h"
#include "Entities/Entity.h"
#include "Entities/Components.h"

namespace Engine::Entities
{
    Scene::Scene()
    {
    }

    Scene::~Scene()
    {
    }

    Entity Scene::CreateEntity(const std::string& entityName)
    {
        Entity createdEntity = Entity(SceneRegistry.create(), this);
        createdEntity.AddComponent<Components::TagComponent>(entityName);
        return createdEntity;
    }

    void Scene::DestroyEntity(Entity entityToDestroy)
    {
        SceneRegistry.destroy(entityToDestroy.GetHandle());
    }

    entt::registry& Scene::GetRegistry()
    {
        return SceneRegistry;
    }
}