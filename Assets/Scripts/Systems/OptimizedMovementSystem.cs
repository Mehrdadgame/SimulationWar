using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct OptimizedMovementSystem : ISystem
{
    private ComponentTypeHandle<LocalTransform> transformHandle;
    private ComponentTypeHandle<MovementComponent> movementHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        transformHandle = state.GetComponentTypeHandle<LocalTransform>();
        movementHandle = state.GetComponentTypeHandle<MovementComponent>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        transformHandle.Update(ref state);
        movementHandle.Update(ref state);

        var movementJob = new MovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            TransformHandle = transformHandle,
            MovementHandle = movementHandle
        };

        var query = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, MovementComponent>()
            .WithNone<DeadTag>()
            .Build();

        state.Dependency = movementJob.ScheduleParallel(query, state.Dependency);
    }
}