using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public struct ChunkAttackJob : IJobChunk
{
    public ComponentTypeHandle<AttackRangeComponent> AttackRangeHandle;
    public ComponentTypeHandle<DamageComponent> DamageHandle;
    [ReadOnly] public ComponentTypeHandle<LocalTransform> TransformHandle;
    public float CurrentTime;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex,
                       bool useEnabledMask, in EnabledMask chunkEnabledMask)
    {
        var attackRanges = chunk.GetNativeArray(ref AttackRangeHandle);
        var damages = chunk.GetNativeArray(ref DamageHandle);
        var transforms = chunk.GetNativeArray(ref TransformHandle);

        for (int i = 0; i < chunk.Count; i++)
        {
            var attackRange = attackRanges[i];
            var damage = damages[i];
            var transform = transforms[i];


        }
    }

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        throw new System.NotImplementedException();
    }
}