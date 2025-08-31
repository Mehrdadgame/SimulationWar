using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[BurstCompile]
public partial struct AdvancedAISystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float currentTime = (float)SystemAPI.Time.ElapsedTime;

        // Collect all potential targets
        var targetQuery = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, UnitTypeComponent>()
            .WithNone<DeadTag>()
            .Build();

        var aiQuery = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, AIComponent, UnitTypeComponent, AttackRangeComponent>()
            .WithNone<DeadTag, PlayerUnitTag>()
            .Build();

        int targetCount = targetQuery.CalculateEntityCount();
        int aiCount = aiQuery.CalculateEntityCount();

        if (targetCount == 0 || aiCount == 0) return;

        var targetEntities = targetQuery.ToEntityArray(Allocator.TempJob);
        var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var targetUnitTypes = targetQuery.ToComponentDataArray<UnitTypeComponent>(Allocator.TempJob);

        var aiEntities = aiQuery.ToEntityArray(Allocator.TempJob);
        var aiTransforms = aiQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var aiComponents = aiQuery.ToComponentDataArray<AIComponent>(Allocator.TempJob);
        var aiUnitTypes = aiQuery.ToComponentDataArray<UnitTypeComponent>(Allocator.TempJob);
        var attackRanges = aiQuery.ToComponentDataArray<AttackRangeComponent>(Allocator.TempJob);

        var findTargetsJob = new FindTargetsJob
        {
            PotentialTargets = targetEntities,
            TargetTransforms = targetTransforms,
            TargetUnitTypes = targetUnitTypes,
            AIEntities = aiEntities,
            AITransforms = aiTransforms,
            AIComponents = aiComponents,
            AIUnitTypes = aiUnitTypes,
            AttackRanges = attackRanges
        };

        var jobHandle = findTargetsJob.Schedule(state.Dependency);
        jobHandle.Complete();

        // Apply results back to entities
        for (int i = 0; i < aiEntities.Length; i++)
        {
            SystemAPI.SetComponent(aiEntities[i], attackRanges[i]);
        }

        // Dispose arrays
        targetEntities.Dispose();
        targetTransforms.Dispose();
        targetUnitTypes.Dispose();
        aiEntities.Dispose();
        aiTransforms.Dispose();
        aiComponents.Dispose();
        aiUnitTypes.Dispose();
        attackRanges.Dispose();
    }
}