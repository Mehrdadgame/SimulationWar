using Unity.Entities;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public partial class RTSBootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        // Add our custom systems as managed systems
        var simulationSystemGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();

        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<OptimizedMovementSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<AIBehaviorSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<AttackSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<ProjectileSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<HealthSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<CommandSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<PathfindingSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<EffectSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<CleanupSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<CollisionDetectionSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<GroupCommandSystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<AdvancedAISystem>());
        simulationSystemGroup.AddSystemToUpdateList(world.CreateSystem<FormationSystem>());

        var presentationSystemGroup = world.GetExistingSystemManaged<PresentationSystemGroup>();
        presentationSystemGroup.AddSystemToUpdateList(world.CreateSystemManaged<UnitSelectionSystem>());
        presentationSystemGroup.AddSystemToUpdateList(world.CreateSystemManaged<PerformanceMonitorSystem>());

        simulationSystemGroup.SortSystems();
        presentationSystemGroup.SortSystems();

        return true;
    }
}