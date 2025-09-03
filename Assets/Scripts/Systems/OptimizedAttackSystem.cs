using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct OptimizedAttackSystem : ISystem
{
    private ComponentTypeHandle<AttackRangeComponent> attackRangeHandle;
    private ComponentTypeHandle<DamageComponent> damageHandle;
    private ComponentTypeHandle<LocalTransform> transformHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        attackRangeHandle = state.GetComponentTypeHandle<AttackRangeComponent>();
        damageHandle = state.GetComponentTypeHandle<DamageComponent>();
        transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        attackRangeHandle.Update(ref state);
        damageHandle.Update(ref state);
        transformHandle.Update(ref state);

        var job = new ChunkAttackJob
        {
            AttackRangeHandle = attackRangeHandle,
            DamageHandle = damageHandle,
            TransformHandle = transformHandle,
            CurrentTime = (float)SystemAPI.Time.ElapsedTime
        };

        // درست کردن scheduling برای IJobChunk
        var query = SystemAPI.QueryBuilder()
            .WithAll<AttackRangeComponent, DamageComponent, LocalTransform>()
            .WithNone<DeadTag>()
            .Build();

        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}