using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct AttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var attackJob = new AttackJob
        {
            CurrentTime = (float)SystemAPI.Time.ElapsedTime,
            ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                          .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            HealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(false),
            DeadTagLookup = SystemAPI.GetComponentLookup<DeadTag>(true)
        };

        // اجرای Job به صورت موازی
        state.Dependency = attackJob.ScheduleParallel(state.Dependency);
    }



}