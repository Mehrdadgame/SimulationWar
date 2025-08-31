using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct MovementJob : IJobChunk
{
    public float DeltaTime;
    public ComponentTypeHandle<LocalTransform> TransformHandle;
    [ReadOnly] public ComponentTypeHandle<MovementComponent> MovementHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        var transforms = chunk.GetNativeArray(ref TransformHandle);
        var movements = chunk.GetNativeArray(ref MovementHandle);

        for (int i = 0; i < chunk.Count; i++)
        {
            var movement = movements[i];
            if (!movement.HasDestination) continue;

            var transform = transforms[i];
            float3 currentPos = transform.Position;
            float3 destination = movement.Destination;
            float3 direction = math.normalize(destination - currentPos);
            float distance = math.distance(currentPos, destination);

            if (distance <= movement.StoppingDistance) continue;

            float3 newPosition = currentPos + direction * movement.Speed * DeltaTime;
            transform.Position = newPosition;

            if (math.lengthsq(direction) > 0.01f)
            {
                transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }

            transforms[i] = transform;
        }
    }


}
