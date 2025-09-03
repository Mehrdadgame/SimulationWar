using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct TargetFindingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var targetJob = new FindTargetJob
        {
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            UnitTypeLookup = SystemAPI.GetComponentLookup<UnitTypeComponent>(true),
            DeadTagLookup = SystemAPI.GetComponentLookup<DeadTag>(true)
        };

        state.Dependency = targetJob.ScheduleParallel(state.Dependency);
    }
}