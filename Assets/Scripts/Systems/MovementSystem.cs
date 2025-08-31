using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct MovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, movement) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MovementComponent>>()
                    .WithNone<DeadTag>())
        {
            if (!movement.ValueRO.HasDestination) continue;

            float3 currentPos = transform.ValueRO.Position;
            float3 destination = movement.ValueRO.Destination;
            float3 direction = math.normalize(destination - currentPos);
            float distance = math.distance(currentPos, destination);

            if (distance <= movement.ValueRO.StoppingDistance)
            {
                continue;
            }

            float3 newPosition = currentPos + direction * movement.ValueRO.Speed * deltaTime;
            transform.ValueRW.Position = newPosition;

            // Face movement direction
            if (math.lengthsq(direction) > 0.01f)
            {
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }
        }
    }
}
